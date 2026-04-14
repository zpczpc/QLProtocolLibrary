namespace QLProtocolLibrary
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Decodes the optional wrapped stream format defined by the protocol document.
    /// Bare packets do not carry boundaries and therefore are not stream-splittable.
    /// </summary>
    public sealed class QlProtocolStreamDecoder
    {
        private readonly List<byte> _buffer = new List<byte>();

        public IReadOnlyList<QlProtocolFrame> Append(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            _buffer.AddRange(data);
            List<QlProtocolFrame> frames = new List<QlProtocolFrame>();

            while (true)
            {
                int headerIndex = FindHeader();
                if (headerIndex < 0)
                {
                    TrimToPossibleHeaderPrefix();
                    break;
                }

                if (headerIndex > 0)
                {
                    _buffer.RemoveRange(0, headerIndex);
                }

                if (_buffer.Count < 6)
                {
                    break;
                }

                ushort packetLength = QlPayloadCodec.DecodeUInt16(_buffer.ToArray(), 4);
                int frameLength = 4 + 2 + packetLength + 2;
                if (_buffer.Count < frameLength)
                {
                    break;
                }

                if (_buffer[frameLength - 2] != QlProtocolConstants.EnvelopeFooter1
                    || _buffer[frameLength - 1] != QlProtocolConstants.EnvelopeFooter2)
                {
                    _buffer.RemoveAt(0);
                    continue;
                }

                byte[] rawFrame = _buffer.GetRange(0, frameLength).ToArray();
                _buffer.RemoveRange(0, frameLength);
                if (QlProtocolParser.TryParse(rawFrame, out QlProtocolFrame? frame) && frame != null && frame.IsCrcValid)
                {
                    frames.Add(frame);
                }
            }

            return frames;
        }

        public void Clear()
        {
            _buffer.Clear();
        }

        private int FindHeader()
        {
            for (int i = 0; i <= _buffer.Count - 4; i++)
            {
                if (_buffer[i] == QlProtocolConstants.EnvelopeHeader1
                    && _buffer[i + 1] == QlProtocolConstants.EnvelopeHeader2
                    && _buffer[i + 2] == QlProtocolConstants.EnvelopeHeader3
                    && _buffer[i + 3] == QlProtocolConstants.EnvelopeHeader4)
                {
                    return i;
                }
            }

            return -1;
        }

        private void TrimToPossibleHeaderPrefix()
        {
            int maxPrefixLength = Math.Min(3, _buffer.Count);
            for (int prefixLength = maxPrefixLength; prefixLength > 0; prefixLength--)
            {
                bool match = true;
                for (int i = 0; i < prefixLength; i++)
                {
                    byte expected = i switch
                    {
                        0 => QlProtocolConstants.EnvelopeHeader1,
                        1 => QlProtocolConstants.EnvelopeHeader2,
                        2 => QlProtocolConstants.EnvelopeHeader3,
                        _ => QlProtocolConstants.EnvelopeHeader4
                    };

                    if (_buffer[_buffer.Count - prefixLength + i] != expected)
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    byte[] tail = _buffer.GetRange(_buffer.Count - prefixLength, prefixLength).ToArray();
                    _buffer.Clear();
                    _buffer.AddRange(tail);
                    return;
                }
            }

            _buffer.Clear();
        }
    }
}
