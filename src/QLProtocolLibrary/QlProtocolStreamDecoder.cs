namespace QLProtocolLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

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
                int headerIndex = FindSequence(_buffer, QlProtocolConstants.HeaderHigh, QlProtocolConstants.HeaderLow, 0);
                if (headerIndex < 0)
                {
                    TrimToPartialHeader();
                    break;
                }

                if (headerIndex > 0)
                {
                    _buffer.RemoveRange(0, headerIndex);
                }

                FrameBoundaryState boundaryState = TryResolveFrameLength(out int frameLength);
                if (boundaryState == FrameBoundaryState.NeedMoreData)
                {
                    break;
                }

                if (boundaryState == FrameBoundaryState.Invalid)
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

        private FrameBoundaryState TryResolveFrameLength(out int frameLength)
        {
            frameLength = 0;
            if (_buffer.Count < QlProtocolConstants.MinimumFrameLength)
            {
                return FrameBoundaryState.NeedMoreData;
            }

            switch (_buffer[10])
            {
                case 0x03:
                    if (_buffer.Count < 14)
                    {
                        return FrameBoundaryState.NeedMoreData;
                    }

                    return TryResolveCandidateFrameLength(frameLengthCandidates: new[] { 18 + _buffer[13], 19 }, out frameLength);
                case 0x10:
                    if (_buffer.Count < 16)
                    {
                        return FrameBoundaryState.NeedMoreData;
                    }

                    return TryResolveCandidateFrameLength(frameLengthCandidates: new[] { 20 + _buffer[15], 19 }, out frameLength);
                case 0x06:
                    return TryResolveCandidateFrameLength(frameLengthCandidates: new[] { 19 }, out frameLength);
                case 0x83:
                case 0x86:
                case 0x90:
                    return TryResolveCandidateFrameLength(frameLengthCandidates: new[] { 18 }, out frameLength);
                default:
                    int footerIndex = FindSequence(_buffer, QlProtocolConstants.FooterHigh, QlProtocolConstants.FooterLow, 2);
                    if (footerIndex < 0)
                    {
                        return FrameBoundaryState.NeedMoreData;
                    }

                    frameLength = footerIndex + 2;
                    return FrameBoundaryState.Ready;
            }
        }

        private FrameBoundaryState TryResolveCandidateFrameLength(IEnumerable<int> frameLengthCandidates, out int frameLength)
        {
            frameLength = 0;
            bool needsMoreData = false;

            foreach (int candidate in frameLengthCandidates.Distinct())
            {
                if (candidate < QlProtocolConstants.MinimumFrameLength)
                {
                    continue;
                }

                if (_buffer.Count < candidate)
                {
                    needsMoreData = true;
                    continue;
                }

                if (!HasFooter(candidate))
                {
                    continue;
                }

                if (CanParseFrame(candidate))
                {
                    frameLength = candidate;
                    return FrameBoundaryState.Ready;
                }
            }

            return needsMoreData ? FrameBoundaryState.NeedMoreData : FrameBoundaryState.Invalid;
        }

        private bool HasFooter(int frameLength)
        {
            return _buffer[frameLength - 2] == QlProtocolConstants.FooterHigh
                && _buffer[frameLength - 1] == QlProtocolConstants.FooterLow;
        }

        private bool CanParseFrame(int frameLength)
        {
            byte[] rawFrame = _buffer.GetRange(0, frameLength).ToArray();
            return QlProtocolParser.TryParse(rawFrame, out QlProtocolFrame? frame) && frame != null && frame.IsCrcValid;
        }

        private void TrimToPartialHeader()
        {
            if (_buffer.Count == 0)
            {
                return;
            }

            if (_buffer[_buffer.Count - 1] == QlProtocolConstants.HeaderHigh)
            {
                byte last = _buffer[_buffer.Count - 1];
                _buffer.Clear();
                _buffer.Add(last);
                return;
            }

            _buffer.Clear();
        }

        private static int FindSequence(List<byte> buffer, byte first, byte second, int startIndex)
        {
            for (int i = startIndex; i < buffer.Count - 1; i++)
            {
                if (buffer[i] == first && buffer[i + 1] == second)
                {
                    return i;
                }
            }

            return -1;
        }

        private enum FrameBoundaryState
        {
            NeedMoreData,
            Ready,
            Invalid
        }
    }
}
