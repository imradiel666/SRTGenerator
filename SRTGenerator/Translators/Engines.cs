namespace SRTGenerator.Translators
{
    internal static class Engines
    {
        public const string GOOGLE = "google";
        public const string DEEPL = "deepl";
        public const string ARGOS = "argos";

        public readonly static List<string> AvailableEngignes = new() { GOOGLE, DEEPL, ARGOS };
    }
}
