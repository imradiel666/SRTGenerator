namespace SRTGenerator.Generators
{
    internal static class Engines
    {
        public const string WHISPER = "whisper";
        public const string VOSK = "vosk";

        public readonly static List<string> AvailableEngignes = new() { WHISPER, VOSK };
    }
}
