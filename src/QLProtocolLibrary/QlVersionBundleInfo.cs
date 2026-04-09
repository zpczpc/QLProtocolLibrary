namespace QLProtocolLibrary
{
    public sealed class QlVersionBundleInfo
    {
        public string AndroidRuntime { get; set; } = string.Empty;

        public string AndroidCommunicationBoard { get; set; } = string.Empty;

        public string CoreControlBoard { get; set; } = string.Empty;

        public string OrpBoard { get; set; } = string.Empty;

        public string MeterBoard { get; set; } = string.Empty;

        public string PlungerPumpBoard { get; set; } = string.Empty;

        public string SpectrometerBoard { get; set; } = string.Empty;

        public override string ToString()
        {
            return "AndroidRuntime=" + AndroidRuntime + ", CoreControlBoard=" + CoreControlBoard + ", SpectrometerBoard=" + SpectrometerBoard;
        }
    }
}
