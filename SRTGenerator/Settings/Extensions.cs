using Microsoft.Extensions.Configuration;

namespace SRTGenerator.Settings
{
    public static class Extensions
    {
        public static ToolsSettings GetToolsSettings(this IConfigurationRoot configuration)
        {
            return configuration.GetSection("Tools")?.Get<ToolsSettings>() ?? new ToolsSettings();
        }
        public static WhisperSettings GetWhisperSettings(this IConfigurationRoot configuration)
        {
            return configuration.GetSection("Whisper")?.Get<WhisperSettings>() ?? new WhisperSettings();
        }
        public static VoskSettings GetVoskSettings(this IConfigurationRoot configuration)
        {
            return configuration.GetSection("Vosk")?.Get<VoskSettings>() ?? new VoskSettings();
        }
        public static DeepLSettings GetDeepLSettings(this IConfigurationRoot configuration)
        {
            return configuration.GetSection("DeepL")?.Get<DeepLSettings>() ?? new DeepLSettings();
        }
        public static GoogleSettings GetGoogleSettings(this IConfigurationRoot configuration)
        {
            return configuration.GetSection("Google")?.Get<GoogleSettings>() ?? new GoogleSettings();
        }
    }
}
