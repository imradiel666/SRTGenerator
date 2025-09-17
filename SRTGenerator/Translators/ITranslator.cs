
namespace SRTGenerator.Translators
{
    public interface ITranslator
    {
        string Engine {  get; }

        Task<string> Translate(string text, string languageFrom = "ja", string languageTo = "en");
    }
}