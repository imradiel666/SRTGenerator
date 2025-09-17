using SRTGenerator.Settings;

namespace SRTGenerator
{
    public class GeneratorArgumentsModel
    {
        public string Input { get; set; }
        public string WhisperModelFile { get; set; }
        public string VoskModelFolder { get; set; }
        public bool CUDA { get; set; }
        public int? Split { get; set; }
        public bool Keep { get; set; }
        public IEnumerable<string> Engines { get; set; }
        public string Language { get; set; }
        public bool Overwrite { get; set; }
        public bool IsFolder { get; set; }

        public void Validate()
        {
            if (string.IsNullOrEmpty(Input))
                throw new ArgumentNullException($"Input file or folder required");

            if (Directory.Exists(Input))
                IsFolder = true;
            else if (File.Exists(Input))
                IsFolder = false;
            else 
                throw new ArgumentException($"Input file or folder not found");

            if (Engines?.Any() != true)
                throw new ArgumentException($"At least one engine is required ({(string.Join(", ", Generators.Engines.AvailableEngignes))})");
            if (Engines.Any(c => !Generators.Engines.AvailableEngignes.Contains(c)))
                throw new ArgumentException($"Only these engines are available ({(string.Join(", ", Generators.Engines.AvailableEngignes))})");

            if (string.IsNullOrEmpty(WhisperModelFile))
            {
                var whisperSettings = Program.Configuration.GetWhisperSettings();
                if (whisperSettings.Models?.ContainsKey(Language) == true)
                    WhisperModelFile = whisperSettings.Models[Language];
                else if (whisperSettings.Models?.ContainsKey("default") == true)
                    WhisperModelFile = whisperSettings.Models["default"];
            }

            if (string.IsNullOrEmpty(VoskModelFolder))
            {
                var voskSettings = Program.Configuration.GetVoskSettings();
                if (voskSettings.Models?.ContainsKey(Language) == true)
                    VoskModelFolder = voskSettings.Models[Language];
                else if (voskSettings.Models?.ContainsKey("default") == true)
                    VoskModelFolder = voskSettings.Models["default"];
            }

            if (Engines.Any(c => c == Generators.Engines.WHISPER) && !string.IsNullOrEmpty(WhisperModelFile) && !File.Exists(WhisperModelFile))
                throw new ArgumentException($"Whisper model file not found");
            if (Engines.Any(c => c == Generators.Engines.VOSK) && !string.IsNullOrEmpty(VoskModelFolder) && !Directory.Exists(VoskModelFolder))
                throw new ArgumentException($"Vosk model folder not found");
        }
    }

    public class TranslatorArgumentsModel
    {
        public string Input { get; set; }
        public IEnumerable<string> Engines { get; set; }
        public bool Overwrite { get; set; }
        public bool IsFolder { get; set; }
        
        public void Validate()
        {
            if (string.IsNullOrEmpty(Input))
                throw new ArgumentNullException($"Input file or folder required");

            if (Directory.Exists(Input))
                IsFolder = true;
            else if (File.Exists(Input))
                IsFolder = false;
            else
                throw new ArgumentException($"Input file or folder not found");

            if (Engines?.Any() != true)
                throw new ArgumentException($"At least one engine is required ({(string.Join(", ", Translators.Engines.AvailableEngignes))})");
            if (Engines.Any(c => !Translators.Engines.AvailableEngignes.Contains(c)))
                throw new ArgumentException($"Only these engines are available ({(string.Join(", ", Translators.Engines.AvailableEngignes))})");

            if (Engines.Any(c => c == Translators.Engines.GOOGLE))
            {
                var googleSettings = Program.Configuration.GetGoogleSettings();
                if (string.IsNullOrEmpty(googleSettings.KeyJSON) || !File.Exists(googleSettings.KeyJSON))
                    throw new ArgumentException($"Google key.json is required to use google translator");
            }
            if (Engines.Any(c => c == Translators.Engines.DEEPL))
            {
                var deepLSettings = Program.Configuration.GetDeepLSettings();
                if (string.IsNullOrEmpty(deepLSettings.Key))
                    throw new ArgumentException($"DeepL api key is required to use DeepL translator");
            }
        }
    }
}
