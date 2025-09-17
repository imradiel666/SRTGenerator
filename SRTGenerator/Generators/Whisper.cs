using Whisper.net;
using Whisper.net.Ggml;
using SRTGenerator.JSON;

namespace SRTGenerator.Generators
{
    internal class Whisper
    {
        readonly List<List<SileroChunk>> _chunks;
        readonly string _vadChunksDir;

        public Whisper(List<List<SileroChunk>> chunks, string vadChunksDir)
        {
            _chunks = chunks;
            _vadChunksDir = vadChunksDir;
        }

        public async Task GenerateSubtitles(string language, string modelFile, string subtitlesFilename)
        {
            Console.WriteLine($"Starting Whisper transcription.");

            await PrepareWhisper(modelFile);
            using var whisperFactory = WhisperFactory.FromPath(modelFile);

            var processorBuilder = whisperFactory.CreateBuilder()
                .WithLanguage(language);

            using var processor = processorBuilder.Build();
            // Loop all the wav
            var srtText = "";
            var counter = 1;
            for (int i = 0; i < _chunks.Count; i++)
            {
                Console.WriteLine($"Processing chunk {i + 1} of {_chunks.Count}...");
                var offset = _chunks[i].Where(c => c.Start.HasValue).Min(c => c.Start).Value;
                using var wavStream = new MemoryStream(File.ReadAllBytes(Path.Combine(_vadChunksDir, $"{i}.wav")));

                // This section processes the audio file and prints the results (start time, end time and text) to the console.
                await foreach (var result in processor.ProcessAsync(wavStream))
                {
                    var startLine = result.Start.Add(TimeSpan.FromSeconds(offset));
                    var endLine = result.End.Add(TimeSpan.FromSeconds(offset));
                    Console.WriteLine($"{startLine:hh\\:mm\\:ss\\,fff}->{endLine:hh\\:mm\\:ss\\,fff}: {result.Text}");

                    srtText += counter.ToString() + Environment.NewLine;
                    srtText += $"{startLine:hh\\:mm\\:ss\\,fff}  -->  {endLine:hh\\:mm\\:ss\\,fff}" + Environment.NewLine;
                    srtText += result.Text + Environment.NewLine;
                    srtText += Environment.NewLine;

                    counter++;

                    File.WriteAllText(subtitlesFilename, srtText);
                }
            }
        }

        private static async Task PrepareWhisper(string modelFile)
        {
            if (!File.Exists(modelFile))
            {
                Console.WriteLine("Downloading Whisper model...");
                using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(GgmlType.LargeV3Turbo, QuantizationType.Q8_0);
                using var fileWriter = File.OpenWrite(modelFile);
                await modelStream.CopyToAsync(fileWriter);
            }
        }
    }
}
