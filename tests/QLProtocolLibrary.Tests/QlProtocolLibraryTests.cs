using System;
using System.Collections.Generic;
using QLProtocolLibrary;
using Xunit;

namespace QLProtocolLibrary.Tests
{
    public sealed class QlProtocolLibraryTests
    {
        [Fact]
        public void BuildRead_KbInfo_UsesUpdatedRegisterCount()
        {
            byte[] command = QlKnownOperations.KbInfo.BuildRead("1001");

            QlProtocolFrame frame = QlProtocolParser.Parse(command);

            Assert.Equal(QlProtocolFrameKind.ReadRequest, frame.Kind);
            Assert.Equal(QlKnownRegisters.KbInfo.Address, frame.Address);
            Assert.Equal((ushort)14, frame.RegisterCount);
            Assert.True(frame.IsCrcValid);
        }

        [Fact]
        public void KnownParser_CanParseDeviceTimeReadResponse()
        {
            DateTime expected = new DateTime(2026, 4, 9, 8, 30, 45);
            byte[] response = BuildReadResponse("1001", QlKnownRegisters.DeviceTime.Address, QlPayloadCodec.EncodeBcdDateTime(expected));

            QlProtocolFrame frame = QlProtocolParser.Parse(response);

            Assert.True(frame.IsCrcValid);
            Assert.True(QlProtocolKnownParsers.TryParseDeviceTime(frame, out DateTime actual));
            Assert.Equal(expected.ToString("yyyyMMddHHmmss"), actual.ToString("yyyyMMddHHmmss"));
        }

        [Fact]
        public void KnownRouter_CanParseRunStatusFrame()
        {
            byte[] payload = Combine(
                QlPayloadCodec.EncodeUInt16(2),
                QlPayloadCodec.EncodeUInt16(7),
                QlPayloadCodec.EncodeUInt16(1),
                QlPayloadCodec.EncodeUInt16(3),
                QlPayloadCodec.EncodeUInt32(1001),
                QlPayloadCodec.EncodeUInt32(0),
                new byte[4]);
            byte[] response = BuildReadResponse("1001", QlKnownRegisters.RunStatus.Address, payload);

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
        public void StreamDecoder_DoesNotSplitWriteFrameWhenPayloadContainsFooterSequence()
        {
            byte[] payload = { 0xBB, 0x55, 0x00, 0x01 };
            byte[] frameBytes = QlProtocolCommandBuilder.BuildWrite("1001", 100, 2, payload);
            QlProtocolStreamDecoder decoder = new QlProtocolStreamDecoder();

            IReadOnlyList<QlProtocolFrame> frames = decoder.Append(frameBytes);

            QlProtocolFrame frame = Assert.Single(frames);
            Assert.Equal(QlProtocolFrameKind.WriteRequest, frame.Kind);
            Assert.Equal((ushort)100, frame.Address);
            Assert.Equal(payload, frame.Payload);
            Assert.True(frame.IsCrcValid);
        }

        [Fact]
        public void KnownParser_RejectsFrameWithInvalidCrc()
        {
            byte[] response = BuildReadResponse("1001", QlKnownRegisters.DeviceTime.Address, QlPayloadCodec.EncodeBcdDateTime(new DateTime(2026, 4, 9, 8, 30, 45)));
            response[response.Length - 4] ^= 0x01;

            QlProtocolFrame frame = QlProtocolParser.Parse(response);

            Assert.False(frame.IsCrcValid);
            Assert.False(QlProtocolKnownParsers.TryParseDeviceTime(frame, out _));
        }

        private static byte[] BuildReadResponse(string mnText, ushort address, byte[] payload)
        {
            byte[] body = Combine(
                ToMnBytes(mnText),
                new byte[] { (byte)QlFunctionCode.Read },
                QlPayloadCodec.EncodeUInt16(address),
                new byte[] { (byte)payload.Length },
                payload);

            ushort crc = QlProtocolCrc16.Compute(body);
            return Combine(
                new byte[] { QlProtocolConstants.HeaderHigh, QlProtocolConstants.HeaderLow },
                body,
                QlProtocolCrc16.GetBytesLowHigh(crc),
                new byte[] { QlProtocolConstants.FooterHigh, QlProtocolConstants.FooterLow });
        }

        private static byte[] ToMnBytes(string mnText)
        {
            ulong mnValue = ulong.Parse(mnText);
            byte[] bytes = BitConverter.GetBytes(mnValue);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return bytes;
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
