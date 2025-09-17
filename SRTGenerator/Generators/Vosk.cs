using SRTGenerator.JSON;
using System.Text.Json;
using Vosk;

namespace SRTGenerator.Generators
{
    internal class Vosk
    {
        readonly List<List<SileroChunk>> _chunks;
        readonly string _vadChunksDir;

        public Vosk(List<List<SileroChunk>> chunks, string vadChunksDir)
        {
            _chunks = chunks;
            _vadChunksDir = vadChunksDir;
        }

        public Task GenerateSubtitles(string modelFolder, string subtitlesFilename)
        {
            Console.WriteLine($"Starting Vosk transcription.");

            // You can set to -1 to disable logging messages
            global::Vosk.Vosk.SetLogLevel(-1);

            var model = new Model(modelFolder);

            // Demo byte buffer
            var rec = new VoskRecognizer(model, 16000.0f);
            rec.SetMaxAlternatives(0);
            rec.SetWords(true);
            
            // Loop all the wavs
            var srtText = "";
            double processedDuration = 0;
            var counter = 1;
            for (int i = 0; i < _chunks.Count; i++)
            {
                var chunkFile = Path.Combine(_vadChunksDir, $"{i}.wav");

                byte[] allBytes = File.ReadAllBytes(chunkFile);
                double byterate = BitConverter.ToInt32(new[] { allBytes[28], allBytes[29], allBytes[30], allBytes[31] }, 0);
                double duration = (allBytes.Length - 8) / byterate;

                Console.WriteLine($"Processing chunk {i + 1} of {_chunks.Count}...");
                var offset = _chunks[i].Where(c => c.Start.HasValue).Min(c => c.Start).Value;
                using (Stream source = File.OpenRead(chunkFile))
                {
                    string tempRec = null;
                    VoskResult result = null;

                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        if (rec.AcceptWaveform(buffer, bytesRead))
                        {
                            tempRec = rec.Result();
                            result = JsonSerializer.Deserialize<VoskResult>(tempRec);
                            if (result.Words?.Any() == true)
                            {
                                var startLine = TimeSpan.FromSeconds(result.Words.Min(c => c.Start).Value).Add(TimeSpan.FromSeconds(offset - processedDuration));
                                var endLine = TimeSpan.FromSeconds(result.Words.Max(c => c.End).Value).Add(TimeSpan.FromSeconds(offset - processedDuration));
                                Console.WriteLine($"{startLine:hh\\:mm\\:ss\\,fff}->{endLine:hh\\:mm\\:ss\\,fff}: {string.Join("", result.Words.Select(c => c.Word))}");

                                srtText += counter.ToString() + Environment.NewLine;
                                srtText += $"{startLine:hh\\:mm\\:ss\\,fff} --> {endLine:hh\\:mm\\:ss\\,fff}" + Environment.NewLine;
                                srtText += string.Join("", result.Words.Select(c => c.Word)) + Environment.NewLine;
                                srtText += Environment.NewLine;

                                counter++;

                                File.WriteAllText(subtitlesFilename, srtText);
                            }
                        }
                        else
                            tempRec = rec.PartialResult();
                    }

                    tempRec = rec.FinalResult();
                    result = JsonSerializer.Deserialize<VoskResult>(tempRec);
                    if (result.Words?.Any() == true)
                    {
                        var startLine = TimeSpan.FromSeconds(result.Words.Min(c => c.Start).Value).Add(TimeSpan.FromSeconds(offset - processedDuration));
                        var endLine = TimeSpan.FromSeconds(result.Words.Max(c => c.End).Value).Add(TimeSpan.FromSeconds(offset - processedDuration));
                        Console.WriteLine($"{startLine:hh\\:mm\\:ss\\,fff}->{endLine:hh\\:mm\\:ss\\,fff}: {string.Join("", result.Words.Select(c => c.Word))}");

                        srtText += counter.ToString() + Environment.NewLine;
                        srtText += $"{startLine:hh\\:mm\\:ss\\,fff} --> {endLine:hh\\:mm\\:ss\\,fff}" + Environment.NewLine;
                        srtText += string.Join("", result.Words.Select(c => c.Word)) + Environment.NewLine;
                        srtText += Environment.NewLine;

                        counter++;

                        File.WriteAllText(subtitlesFilename, srtText);
                    }

                    processedDuration += duration;
                }
            }

            return Task.CompletedTask;
        }
    }
}
