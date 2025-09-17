using SRTGenerator.Translators;

namespace SRTGenerator.Actions
{
    public class Translator
    {
        const int MINIMUM_STRIP_TEXT = 30;
        const int MAXIMUM_LINE = 100;
        const int MINIMUM_LINE = 10;

        string subtitleFileName = null;
        string jobName = null;
        string subtitleDir = null;

        readonly TranslatorArgumentsModel _requestModel;
        readonly List<ITranslator> _translators = new();

        public Translator(TranslatorArgumentsModel requestModel)
        {
            _requestModel = requestModel;

            foreach (var engine in _requestModel.Engines)
            {
                switch (engine) {
                    case Engines.GOOGLE:
                        _translators.Add(new Translators.Google());
                        break;
                    case Engines.DEEPL:
                        _translators.Add(new Translators.DeepL());
                        break;
                    case Engines.ARGOS:
                        _translators.Add(new Argos());
                        break;
                }
            }
        }

        public async Task Run()
        {
            if (_requestModel == null)
                throw new ArgumentNullException(nameof(_requestModel));

            try
            {
                var startTime = DateTime.Now;

                if (!string.IsNullOrEmpty(_requestModel.Input))
                    subtitleFileName = new FileInfo(_requestModel.Input).FullName;

                // Create job dir and initialize full paths
                Console.WriteLine("Preparing job...");
                InitializePaths();
                
                var subRegex = RegularExpressions.Expressions.SRTTimming(); // is number or srt range line

                try
                {
                    string[] subtitlesText = File.ReadAllLines(subtitleFileName);
                    foreach (var translator in _translators)
                    {
                        Console.WriteLine($"Using {translator.Engine} engine...");
                        Console.WriteLine();

                        var srtText = "";

                        var translatedSubtitleName = $"{jobName}.{translator.Engine}.eng.srt";
                        var translatedSubtitleFilename = Path.Combine(subtitleDir, translatedSubtitleName);

                        if (_requestModel.Overwrite || !File.Exists(translatedSubtitleFilename))
                        {
                            foreach (var line in subtitlesText)
                            {
                                var translatedText = line;
                                if (line.Trim() != string.Empty && !subRegex.IsMatch(line))
                                {
                                    // Hallucinations
                                    if (line != "ご視聴ありがとうございました" &&
                                        line != "ご視聴ありがとうございました。" &&
                                        line != "ありがとうございました。")
                                    {
                                        string engineTranslated = await translator.Translate(line, "ja",
                                            translator is Translators.DeepL ? DeepL.LanguageCode.EnglishAmerican : "en");

                                        // add to next context (no context... repeat a lot of phrases)
                                        //context += Environment.NewLine + line;

                                        // Divide words for better reading
                                        var middle = engineTranslated.Length / 2;
                                        if (engineTranslated.Length > MINIMUM_STRIP_TEXT && middle < MAXIMUM_LINE)
                                        {
                                            translatedText = "";
                                            for (int i = 0; i < engineTranslated.Length; i++)
                                            {
                                                if (i > middle && engineTranslated[i] == ' ' ||
                                                    i > middle - 1 && engineTranslated[i] == ' ')
                                                {
                                                    translatedText += Environment.NewLine + engineTranslated[(i + 1)..];

                                                    // Reset if it's too short
                                                    if (engineTranslated[(i + 1)..].Length < MINIMUM_LINE)
                                                        translatedText = "";

                                                    break;
                                                }
                                                translatedText += engineTranslated[i];
                                            }

                                            // Try backwards
                                            if (translatedText == "")
                                            {
                                                for (int i = engineTranslated.Length - 1; i >= 0; i--)
                                                {
                                                    if (i > middle && engineTranslated[i] == ' ' ||
                                                        i > middle - 1 && engineTranslated[i] == ' ')
                                                    {
                                                        translatedText = engineTranslated[0..(i - 1)] + Environment.NewLine + translatedText;

                                                        // Reset if it's too short
                                                        if (engineTranslated[0..(i - 1)].Length < MINIMUM_LINE)
                                                            translatedText = "";

                                                        break;
                                                    }
                                                    translatedText = engineTranslated[i] + translatedText;
                                                }
                                            }

                                            // None is good option
                                            if (translatedText == "")
                                                translatedText = engineTranslated;
                                        }
                                        else
                                            translatedText = engineTranslated;
                                    }
                                    else
                                        translatedText = " ";
                                }

                                Console.WriteLine(translatedText);
                                srtText += translatedText + Environment.NewLine;
                            }

                            // Save at the end
                            File.WriteAllText(translatedSubtitleFilename, srtText);
                        }
                        else
                            Console.WriteLine($"Skiping {translatedSubtitleName} already exists");
                    }

                    var endTime = DateTime.Now;
                    var totalTime = endTime - startTime;
                    Console.WriteLine($"Total job time: {totalTime:hh\\:mm\\:ss\\,fff}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void InitializePaths()
        {
            jobName = Path.GetFileNameWithoutExtension(subtitleFileName);
            if (jobName.EndsWith(".jap"))
                jobName = jobName[..^".jap".Length];

            subtitleDir = Path.GetDirectoryName(subtitleFileName);
        }
    }
}
