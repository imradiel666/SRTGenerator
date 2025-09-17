using Google.Cloud.Translation.V2;
using SRTGenerator.Settings;

namespace SRTGenerator.Translators
{
    public class Google : ITranslator
    {
        public string Engine => Engines.GOOGLE;

        readonly TranslationClient _googleTranslator;

        public Google()
        {
            var googleSettings = Program.Configuration.GetGoogleSettings();
            if (string.IsNullOrEmpty(googleSettings.KeyJSON) || !File.Exists(googleSettings.KeyJSON))
                throw new ArgumentException($"Google key.json not found");

            string credential_path = new FileInfo(googleSettings.KeyJSON).FullName;
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credential_path);
            _googleTranslator = TranslationClient.Create();
        }

        public async Task<string> Translate(string text, string languageFrom = LanguageCodes.Japanese, string languageTo = LanguageCodes.English)
        {
            try
            {
                TranslationResult translated = await _googleTranslator.TranslateTextAsync(text, languageTo, languageFrom, TranslationModel.NeuralMachineTranslation);
                return translated.TranslatedText;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}