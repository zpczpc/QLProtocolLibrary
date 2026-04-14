using System;
using System.Collections.Generic;
using QLProtocolLibrary;
using Xunit;

namespace QLProtocolLibrary.Tests
{
    public sealed class QlProtocolLibraryTests
    {
        private const uint DeviceAddress = 0x10000001;

        [Fact]
        public void BuildRead_MatchesDocumentExample()
        {
            byte[] command = QlProtocolCommandBuilder.BuildRead(DeviceAddress, 0x0000, 0x0001);

            Assert.Equal("10 00 00 01 03 00 00 00 01 43 21", QlHexConverter.ToHexString(command));
        }

        [Fact]
        public void ParseReadResponseFloat_MatchesDocumentExample()
        {
            QlProtocolFrame frame = QlProtocolParser.ParseHex("10 00 00 01 03 00 00 04 1C 04 1F 41 97 E9");

            Assert.True(frame.IsCrcValid);
            Assert.Equal(DeviceAddress, frame.DeviceAddress);
            Assert.Equal(QlProtocolFrameKind.ReadResponse, frame.Kind);
            Assert.Equal((ushort)0x0000, frame.Address);
            Assert.Equal((byte)4, frame.ByteCount);
            Assert.Equal(9.9385f, frame.ReadSingle(), 4);
        }

        [Fact]
        public void ParseReadResponseWord_MatchesDocumentExample()
        {
            QlProtocolFrame frame = QlProtocolParser.ParseHex("10 00 00 01 03 13 FA 04 E3 07 00 00 A9 66");

            Assert.True(frame.IsCrcValid);
            Assert.Equal((ushort)0x13FA, frame.Address);
            Assert.Equal((ushort)2019, frame.ReadUInt16());
        }

        [Fact]
        public void BuildWriteFloat_MatchesDocumentExample()
        {
            byte[] command = QlProtocolCommandBuilder.BuildWriteFloat(0x10000005, 0x164E, 0.0596f);

            Assert.Equal("10 00 00 05 06 16 4E 00 01 04 21 1F 74 3D 05 E4", QlHexConverter.ToHexString(command));
        }

        [Fact]
        public void BuildWriteRegisters_MatchesDocumentExample()
        {
            byte[] command = QlProtocolCommandBuilder.BuildWriteRegisters(DeviceAddress, 0x14B4, 115);

            Assert.Equal("10 00 00 01 06 14 B4 00 01 04 73 00 00 00 41 20", QlHexConverter.ToHexString(command));
        }

        [Fact]
        public void KnownOperation_BuildReadUsesRegisterCountInFourByteUnits()
        {
            byte[] command = QlKnownOperations.KbInfo.BuildRead(DeviceAddress);
            QlProtocolFrame frame = QlProtocolParser.Parse(command);

            Assert.Equal(QlProtocolFrameKind.ReadRequest, frame.Kind);
            Assert.Equal(QlKnownRegisters.KbInfo.Address, frame.Address);
            Assert.Equal((ushort)7, frame.RegisterCount);
            Assert.True(frame.IsCrcValid);
        }

        [Fact]
        public void KnownRouter_CanParseRunStatusFrame()
        {
            byte[] payload = Combine(
                QlPayloadCodec.EncodeValueUInt16(2),
                QlPayloadCodec.EncodeValueUInt16(7),
                QlPayloadCodec.EncodeValueUInt16(1),
                QlPayloadCodec.EncodeValueUInt16(3),
                QlPayloadCodec.EncodeUInt32(1001),
                QlPayloadCodec.EncodeUInt32(0));
            byte[] response = BuildReadResponse(DeviceAddress, QlKnownRegisters.RunStatus.Address, payload);

            QlProtocolFrame frame = QlProtocolParser.Parse(response);

            Assert.True(QlProtocolKnownRouter.TryParse(frame, out QlKnownParseResult? result));
            Assert.NotNull(result);
            Assert.Equal("RunStatus", result!.Name);

            QlRunStatusInfo typed = Assert.IsType<QlRunStatusInfo>(result.Value);
            Assert.Equal((ushort)2, typed.Status);
            Assert.Equal((ushort)7, typed.SubStatus);
            Assert.Equal((uint)1001, typed.WarnCode);
        }

        [Fact]
        public void KnownParser_RejectsFrameWithInvalidCrc()
        {
            byte[] response = BuildReadResponse(DeviceAddress, QlKnownRegisters.DeviceTime.Address, PadToRegister(QlPayloadCodec.EncodeBcdDateTime(new DateTime(2026, 4, 9, 8, 30, 45))));
            response[^1] ^= 0x01;

            QlProtocolFrame frame = QlProtocolParser.Parse(response);

            Assert.False(frame.IsCrcValid);
            Assert.False(QlProtocolKnownParsers.TryParseDeviceTime(frame, out _));
        }

        [Fact]
        public void StreamDecoder_CanDecodeTwoWrappedFrames()
        {
            byte[] first = BuildReadResponse(DeviceAddress, 0x0000, QlHexConverter.FromHexString("1C 04 1F 41"), includeEnvelope: true);
            byte[] second = BuildReadResponse(DeviceAddress, 0x13FA, QlHexConverter.FromHexString("E3 07 00 00"), includeEnvelope: true);
            QlProtocolStreamDecoder decoder = new QlProtocolStreamDecoder();

            IReadOnlyList<QlProtocolFrame> frames = decoder.Append(Combine(first, second));

            Assert.Equal(2, frames.Count);
            Assert.All(frames, frame => Assert.True(frame.IsCrcValid));
        }

        private static byte[] BuildReadResponse(uint deviceAddress, ushort address, byte[] payload, bool includeEnvelope = false)
        {
            return BuildPacket(
                deviceAddress,
                (byte)QlFunctionCode.Read,
                Combine(QlPayloadCodec.EncodeUInt16(address), new[] { (byte)payload.Length }, payload),
                includeEnvelope);
        }

        private static byte[] BuildPacket(uint deviceAddress, byte functionCode, byte[] functionData, bool includeEnvelope)
        {
            byte[] body = Combine(ToDeviceAddressBytes(deviceAddress), new[] { functionCode }, functionData);
            ushort crc = QlProtocolCrc16.Compute(body);
            byte[] packet = Combine(body, QlProtocolCrc16.GetBytesLowHigh(crc));

            if (!includeEnvelope)
            {
                return packet;
            }

            return Combine(
                new[] { QlProtocolConstants.EnvelopeHeader1, QlProtocolConstants.EnvelopeHeader2, QlProtocolConstants.EnvelopeHeader3, QlProtocolConstants.EnvelopeHeader4 },
                QlPayloadCodec.EncodeUInt16((ushort)packet.Length),
                packet,
                new[] { QlProtocolConstants.EnvelopeFooter1, QlProtocolConstants.EnvelopeFooter2 });
        }

        private static byte[] ToDeviceAddressBytes(uint deviceAddress)
        {
            return new[]
            {
                (byte)((deviceAddress >> 24) & 0xFF),
                (byte)((deviceAddress >> 16) & 0xFF),
                (byte)((deviceAddress >> 8) & 0xFF),
                (byte)(deviceAddress & 0xFF)
            };
        }

        private static byte[] PadToRegister(byte[] payload)
        {
            int registerSize = QlProtocolConstants.RegisterByteLength;
            int targetLength = ((payload.Length + registerSize - 1) / registerSize) * registerSize;
            if (targetLength == payload.Length)
            {
                return payload;
            }

            byte[] result = new byte[targetLength];
            Array.Copy(payload, result, payload.Length);
            return result;
        }

        private static byte[] Combine(params byte[][] arrays)
        {
            int totalLength = 0;
            for (int i = 0; i < arrays.Length; i++)
            {
                totalLength += arrays[i].Length;
            }

            byte[] result = new byte[totalLength];
            int offset = 0;
            for (int i = 0; i < arrays.Length; i++)
            {
                Array.Copy(arrays[i], 0, result, offset, arrays[i].Length);
                offset += arrays[i].Length;
            }

            return result;
        }
    }
}
