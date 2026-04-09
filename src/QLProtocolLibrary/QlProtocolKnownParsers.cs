namespace QLProtocolLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    /// <summary>
    /// Provides high-level typed parsers for common business registers.
    /// </summary>
    public static class QlProtocolKnownParsers
    {
        public static bool TryParseDeviceNo(QlProtocolFrame frame, out string? value)
        {
            return TryParseUtf8(frame, QlKnownRegisters.DeviceNo.Address, out value);
        }

        public static bool TryParseAnalyzerCode(QlProtocolFrame frame, out string? value)
        {
            return TryParseUtf8(frame, QlKnownRegisters.AnalyzerCode.Address, out value);
        }

        public static bool TryParseDeviceTime(QlProtocolFrame frame, out DateTime value)
        {
            if (!IsReadableRegister(frame, QlKnownRegisters.DeviceTime.Address, 6))
            {
                value = default;
                return false;
            }

            value = frame.ReadBcdDateTime();
            return true;
        }

        public static bool TryParseConcentration(QlProtocolFrame frame, out float value)
        {
            if (!IsReadableRegister(frame, QlKnownRegisters.Concentration.Address, 4))
            {
                value = default;
                return false;
            }

            value = frame.ReadSingle();
            return true;
        }

        public static bool TryParseRunStatus(QlProtocolFrame frame, out QlRunStatusInfo? value)
        {
            if (!IsReadableRegister(frame, QlKnownRegisters.RunStatus.Address, 16))
            {
                value = null;
                return false;
            }

            value = new QlRunStatusInfo
            {
                Status = frame.ReadUInt16(0),
                SubStatus = frame.ReadUInt16(2),
                RunMode = frame.ReadUInt16(4),
                MeasureMode = frame.ReadUInt16(6),
                WarnCode = frame.ReadUInt32(8),
                FaultCode = frame.Payload.Length >= 16 ? frame.ReadUInt32(12) : 0U
            };
            return true;
        }

        public static bool TryParseMeterStrongLight(QlProtocolFrame frame, out QlMeterStrongLightInfo? value)
        {
            if (!IsReadableRegister(frame, QlKnownRegisters.MeterStrongLight.Address, 4))
            {
                value = null;
                return false;
            }

            value = new QlMeterStrongLightInfo
            {
                Channel1 = frame.ReadUInt16(0),
                Channel2 = frame.ReadUInt16(2)
            };
            return true;
        }

        public static bool TryParseKbInfo(QlProtocolFrame frame, out QlKbInfo? value)
        {
            if (!IsReadableRegister(frame, QlKnownRegisters.KbInfo.Address, 28))
            {
                value = null;
                return false;
            }

            DateTime sampleTime;
            try
            {
                sampleTime = ParsePackedDateTime(frame.Payload, 0);
            }
            catch
            {
                value = null;
                return false;
            }

            string measureNumberText = QlHexConverter.ToHexString(new[] { frame.Payload[6] }, withSpaces: false);
            value = new QlKbInfo
            {
                SampleTime = sampleTime,
                MeasureNumberText = measureNumberText,
                MeasureNumber = int.TryParse(measureNumberText, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int number) ? number : (int?)null,
                KValue = frame.ReadSingle(8),
                BValue = frame.ReadSingle(12),
                FValue = frame.ReadSingle(16),
                AmendmentK = frame.ReadSingle(20),
                AmendmentB = frame.ReadSingle(24)
            };
            return true;
        }

        public static bool TryParseVersionBundle(QlProtocolFrame frame, out QlVersionBundleInfo? value)
        {
            if (!IsReadableRegister(frame, QlKnownRegisters.VersionBundle.Address, 224))
            {
                value = null;
                return false;
            }

            value = new QlVersionBundleInfo
            {
                AndroidRuntime = ReadUtf8Block(frame.Payload, 0, 32),
                AndroidCommunicationBoard = ReadUtf8Block(frame.Payload, 32, 32),
                CoreControlBoard = ReadUtf8Block(frame.Payload, 64, 32),
                OrpBoard = ReadUtf8Block(frame.Payload, 96, 32),
                MeterBoard = ReadUtf8Block(frame.Payload, 128, 32),
                PlungerPumpBoard = ReadUtf8Block(frame.Payload, 160, 32),
                SpectrometerBoard = ReadUtf8Block(frame.Payload, 192, 32)
            };
            return true;
        }

        public static bool TryParseMeasureResult(QlProtocolFrame frame, out QlMeasureResultInfo? value)
        {
            if (!IsReadableRegister(frame, QlKnownRegisters.MeasureResult.Address, 88))
            {
                value = null;
                return false;
            }

            DateTime? measureTime = null;
            try
            {
                measureTime = ParsePackedDateTime(frame.Payload, 32);
            }
            catch
            {
            }

            List<float> energies = new List<float>();
            for (int offset = 54; offset + 4 <= frame.Payload.Length && offset <= 82; offset += 4)
            {
                uint raw = QlPayloadCodec.DecodeUInt32(frame.Payload, offset);
                if (raw != 0xFFFFFFFF)
                {
                    energies.Add(frame.ReadSingle(offset));
                }
            }

            value = new QlMeasureResultInfo
            {
                Component = ReadAsciiBlock(frame.Payload, 0, 32),
                MeasureTime = measureTime,
                Value = frame.ReadSingle(38),
                Flag = ReadUtf8Block(frame.Payload, 42, 8),
                Absorbancy = frame.ReadSingle(50),
                Energies = energies,
                WorkType = frame.Payload.Length >= 86 ? frame.ReadUInt16(86) : (ushort)0
            };
            return true;
        }

        private static bool TryParseUtf8(QlProtocolFrame frame, ushort address, out string? value)
        {
            if (!IsReadableRegister(frame, address, 1))
            {
                value = null;
                return false;
            }

            value = frame.ReadUtf8();
            return true;
        }

        private static bool IsReadableRegister(QlProtocolFrame frame, ushort address, int minPayloadLength)
        {
            if (frame == null)
            {
                return false;
            }

            return frame.Kind == QlProtocolFrameKind.ReadResponse
                && frame.Address == address
                && frame.Payload != null
                && frame.Payload.Length >= minPayloadLength
                && frame.IsCrcValid;
        }

        private static string ReadUtf8Block(byte[] payload, int offset, int count)
        {
            byte[] buffer = new byte[count];
            Array.Copy(payload, offset, buffer, 0, count);
            return QlPayloadCodec.DecodeUtf8(buffer).Replace("�", string.Empty).Trim();
        }

        private static string ReadAsciiBlock(byte[] payload, int offset, int count)
        {
            byte[] buffer = new byte[count];
            Array.Copy(payload, offset, buffer, 0, count);
            return System.Text.Encoding.ASCII.GetString(buffer).TrimEnd('\0').Trim();
        }

        private static DateTime ParsePackedDateTime(byte[] payload, int offset)
        {
            string year = QlHexConverter.ToHexString(new[] { payload[offset] }, false);
            string month = QlHexConverter.ToHexString(new[] { payload[offset + 1] }, false);
            string day = QlHexConverter.ToHexString(new[] { payload[offset + 2] }, false);
            string hour = QlHexConverter.ToHexString(new[] { payload[offset + 3] }, false);
            string minute = QlHexConverter.ToHexString(new[] { payload[offset + 4] }, false);
            string second = QlHexConverter.ToHexString(new[] { payload[offset + 5] }, false);
            return DateTime.ParseExact("20" + year + month + day + hour + minute + second, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        }
    }
}
