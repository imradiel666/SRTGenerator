using CommandLine;

namespace SRTGenerator
{
    [Verb("generate", HelpText = "Transcribe audio/video to srt")]
    public class GeneratorOptions
    {
        [Option("input", Required = true, HelpText = "Audio/Video file to convert or folder containing files to process")]
        public string Input { get; set; }

        [Option("model", Required = false, HelpText = "Overrides default model file on settings")]
        public string ModelFile { get; set; }

        [Option("cuda", Required = false, HelpText = "Decrypt with CUDA processing")]
        public bool CUDA { get; set; }

        [Option("split", Required = false, HelpText = "Split audio to process in minutes (prevents memory errors on demucs - low RAM devices)")]
        public int? Split { get; set; }

        [Option("keep", Required = false, HelpText = "Keep temporary files on work folder")]
        public bool Keep { get; set; }

        [Option("engines", Required = true, Default = "whisper", HelpText = "Space separated engines to use on subtitle generation. Available engines: whisper, vosk (whisper by default)")]
        public IEnumerable<string> Engines { get; set; }

        [Option("lang", Required = false, Default = "japanese", HelpText = "Language of audio/video (japanese by default)")]
        public string Language { get; set; }

        [Option("overwrite", Required = false, Default = false, HelpText = "Overwrites and reprocess transcription if the subtitle already exists")]
        public bool Overwrite { get; set; }

    }

    [Verb("translate", HelpText = "Translate SRT subtitle or (*.jap.srt folder subtitles) to english")]
    public class TranslatorOptions
    {
        [Option("input", Required = true, HelpText = "Subtitle to translate or folder with *.jap.srt files to process")]
        public string Input { get; set; }

        [Option("engines", Required = false, Default = "google", HelpText = "Space separated engines used to translate. Available engines: google, deepl, argos (google by default)")]
        public IEnumerable<string> Engines { get; set; }

        [Option("overwrite", Required = false, Default = false, HelpText = "Overwrites and reprocess translation if the translated subtitle already exists")]
        public bool Overwrite { get; set; }
    }
}
