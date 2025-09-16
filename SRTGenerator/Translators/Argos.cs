using SRTGenerator.Process;

namespace SRTGenerator.Translators
{
    public class Argos : ITranslator
    {
        public string Engine => Engines.ARGOS;

        readonly Python _pythonProcess;

        public Argos()
        {
            _pythonProcess = new Python();
        }

        public async Task<string> Translate(string text, string languageFrom = "ja", string languageTo = "en")
        {
            try
            {
                _pythonProcess.Execute($"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", "argos.py")} -t \"{text.Replace("\"", "\"\"")}\" -from_lang \"{languageFrom}\" -to_lang \"{languageTo}\"", AppDomain.CurrentDomain.BaseDirectory, true);
                return await Task.FromResult(_pythonProcess.Output);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}