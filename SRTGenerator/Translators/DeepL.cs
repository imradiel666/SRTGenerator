using DeepL;
using SRTGenerator.Settings;

namespace SRTGenerator.Translators
{
    public class DeepL : ITranslator
    {
        public string Engine => Engines.DEEPL;

        readonly Translator _deepTranslator;

        public DeepL()
        {
            var deeplSettings = Program.Configuration.GetDeepLSettings();
            if (string.IsNullOrEmpty(deeplSettings.Key))
                throw new ArgumentException($"DeepL api key not found");

            _deepTranslator = new Translator(deeplSettings.Key);
        }

        public async Task<string> Translate(string text, string languageFrom = LanguageCode.Japanese, string languageTo = LanguageCode.EnglishAmerican)
        {
            try
            {
                // TODO: When using context as all translated text results are not well defined (maybe using only the last X improves)
                // var context = "";

                // Translate text into a target language
                var translated = await _deepTranslator.TranslateTextAsync(
                      text,
                      languageFrom,
                      languageTo, new TextTranslateOptions()
                      {
                          // Context = context,
                          ModelType = ModelType.PreferQualityOptimized,
                          SentenceSplittingMode = SentenceSplittingMode.NoNewlines,
                      });


                return translated.Text;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}