using CommandLine;
using Microsoft.Extensions.Configuration;

namespace SRTGenerator
{
    internal class Program
    {
        public static IConfigurationRoot Configuration { get; private set; }

        static async Task Main(string[] args)
        {
            try
            {
                Console.OutputEncoding = System.Text.Encoding.Unicode;
                Console.InputEncoding = System.Text.Encoding.Unicode;

                var builder = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                Configuration = builder.Build();

                await Parser.Default.ParseArguments<GeneratorOptions, TranslatorOptions>(args)
                    .MapResult(
                        async (GeneratorOptions opts) => {
                            var generatorOptions = MapOptions(opts);
                            generatorOptions.Validate();

                            if (generatorOptions.IsFolder)
                            {
                                string[] mediaExtensions = Constants.VideoExtensions.Union(Constants.AudioExtensions).ToArray();
                                var mediaFiles = Directory.GetFiles(generatorOptions.Input)
                                    .Where(v => mediaExtensions.Contains(Path.GetExtension(v), StringComparer.OrdinalIgnoreCase))
                                    .ToList();
                                for (int i = 0; i < mediaFiles.Count; i++)
                                {
                                    Console.WriteLine($"Processing folder media file {(i + 1)} of {mediaFiles.Count}");
                                    Console.WriteLine();

                                    generatorOptions.Input = mediaFiles[i];
                                    await new Actions.Generator(generatorOptions).Run();
                                }
                            }
                            else
                                await new Actions.Generator(generatorOptions).Run();
                        },
                        async (TranslatorOptions opts) => {
                            var translatorOptions = MapOptions(opts);
                            translatorOptions.Validate();

                            if (translatorOptions.IsFolder)
                            {
                                string[] mediaExtensions = {
                                    ".jap.srt"
                                };
                                var subtitleFiles = Directory.GetFiles(translatorOptions.Input)
                                    .Where(v => mediaExtensions.Any(me => v.ToLowerInvariant().EndsWith(me)))
                                    .ToList();
                                for (int i = 0; i < subtitleFiles.Count; i++)
                                {
                                    Console.WriteLine($"Processing folder subtitle {(i + 1)} of {subtitleFiles.Count}");
                                    Console.WriteLine();

                                    translatorOptions.Input = subtitleFiles[i];
                                    await new Actions.Translator(translatorOptions).Run();
                                }
                            }
                            else
                                await new Actions.Translator(translatorOptions).Run();
                        },
                        errs => throw new Exception("Invalid params. Use --help"));
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"See Readme or use --help to view command arguments.");
                Environment.Exit(-1);
            }
        }

        private static GeneratorArgumentsModel MapOptions(GeneratorOptions options)
        {
            return new GeneratorArgumentsModel()
            {
                Input = options.Input,
                WhisperModelFile = options.ModelFile,
                CUDA = options.CUDA,
                Split = options.Split,
                Keep = options.Keep,
                Engines = options.Engines?.Select(c => c.ToLower()),
                Language = options.Language,
                Overwrite = options.Overwrite
            };
        }

        private static TranslatorArgumentsModel MapOptions(TranslatorOptions options)
        {
            return new TranslatorArgumentsModel()
            {
                Input = options.Input,
                Engines = options.Engines?.Select(c => c.ToLower()),
                Overwrite = options.Overwrite
            };
        }
    }
}
