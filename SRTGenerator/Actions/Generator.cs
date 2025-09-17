using System.Text.Json;
using SRTGenerator.JSON;

namespace SRTGenerator.Actions
{
    public class Generator
    {
        readonly string _baseDir = AppDomain.CurrentDomain.BaseDirectory;
        readonly GeneratorArgumentsModel _requestModel;
        readonly string _workDir = @"work";
        
        readonly Process.Python _pythonProcess;
        readonly Process.FFMpeg _ffmpegProcess;

        string whisperModelFile = null;
        string voskModelFolder = null;
        string inputFile = null;
        string audioFile = null;

        string splitAudioDir = null;
        string splitAudioOutput = null;

        string jobName = null;
        string subtitleDir = null;

        string workDir = null;
        string jobDir = null;
        string demucsOutputDir = null;
        string vocalsFile = null;
        string vadChunks = null;
        string vadChunksJson = null;

        public Generator(GeneratorArgumentsModel requestModel)
        {
            _pythonProcess = new Process.Python();
            _ffmpegProcess = new Process.FFMpeg();

            _requestModel = requestModel;
            if (_requestModel == null)
                throw new ArgumentNullException(nameof(requestModel));
        }

        public async Task Run()
        {
            try
            {
                var startTime = DateTime.Now;

                if (!string.IsNullOrEmpty(_requestModel.Input))
                    inputFile = new FileInfo(_requestModel.Input).FullName;

                if (!string.IsNullOrEmpty(_requestModel.WhisperModelFile))
                    whisperModelFile = new FileInfo(_requestModel.WhisperModelFile).FullName;
                if (!string.IsNullOrEmpty(_requestModel.VoskModelFolder))
                    voskModelFolder = new DirectoryInfo(_requestModel.VoskModelFolder).FullName;

                // Create job dir and initialize full paths
                Console.WriteLine("Preparing job directory...");
                InitializePaths();
                _pythonProcess.SetEnvironment();

                if (Constants.AudioExtensions.Contains(Path.GetExtension(inputFile), StringComparer.OrdinalIgnoreCase))
                    audioFile = inputFile;
            
                try
                {
                    // Extract audio
                    if (!File.Exists(audioFile))
                    {
                        Console.WriteLine("Extracting audio...");
                        _ffmpegProcess.Execute($"-i \"{inputFile}\" -q:a 0 -map a \"{audioFile}\"", jobDir);
                    }
                    else
                        Console.WriteLine("Audio file found. Skipping FFMpeg...");

                    // Execute Demucs to extract vocals
                    if (!File.Exists(vocalsFile))
                    {
                        if (File.Exists(audioFile))
                        {
                            // Split audio (optional)
                            if (_requestModel.Split > 0)
                            {
                                Console.WriteLine("Split audio...");
                                if (!Directory.Exists(splitAudioDir))
                                    Directory.CreateDirectory(splitAudioDir);
                                _ffmpegProcess.Execute($"-i \"{audioFile}\" -f segment -segment_time {_requestModel.Split * 60} \"{splitAudioOutput}%03d.wav\"", jobDir);

                                // Multiple files
                                Console.WriteLine("Executing Demucs split...");
                                var audioFiles = Directory.GetFiles(splitAudioDir, "*.wav");
                                var demucsFiles = string.Concat(audioFiles.Select(c => $"\"{c}\" "));
                                _pythonProcess.Execute($"-m demucs {(_requestModel.CUDA ? "-d cuda --segment 7" : "")} --two-stems=vocals -o demucs {demucsFiles}", jobDir);

                                // Merge outputs
                                audioFiles = Directory.GetFiles(demucsOutputDir, "vocals.wav", SearchOption.AllDirectories);
                                var mergeInputs = string.Concat(audioFiles.Select(c => $"-i \"{c}\" "));
                                var filterComplex = "";
                                for (int i = 0; i < audioFiles.Length; i++)
                                    filterComplex += $"[{i}:0]";
                                filterComplex += $"concat=n={audioFiles.Length}:v=0:a=1[out]";

                                Console.WriteLine("Merge audio vocal files...");
                                _ffmpegProcess.Execute($"{mergeInputs} -filter_complex {filterComplex} -map [out] \"{vocalsFile}\"", jobDir);
                            }
                            else
                            {
                                // One file
                                Console.WriteLine("Executing Demucs...");
                                _pythonProcess.Execute($"-m demucs {(_requestModel.CUDA ? "-d cuda --segment 7" : "")} --two-stems=vocals -o demucs \"{audioFile}\"", jobDir);
                            }
                        }
                        else
                            throw new Exception("Audio file not found... something went wrong");
                    }
                    else
                        Console.WriteLine("Vocals file found. Skipping Demucs...");

                    // Execute Silero to split speechs
                    if (!File.Exists(vadChunksJson))
                    {
                        if (File.Exists(vocalsFile))
                        {
                            Console.WriteLine("Executing Silero...");
                            _pythonProcess.Execute($"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", "silero.py")} -f \"{vocalsFile}\"", jobDir);
                        }
                        else
                            throw new Exception("Vocals file not found... something went wrong");
                    }
                    else
                        Console.WriteLine("VAD chunks found. Skipping Silero...");

                    if (File.Exists(vadChunksJson))
                    {
                        var chunks = ReadChunks();

                        foreach (var engine in _requestModel.Engines)
                        {
                            var subtitleName = $"{jobName}.{engine.ToLower()}.{_requestModel.Language[..3]}.srt";
                            var subtitleFileName = Path.Combine(subtitleDir, subtitleName);

                            if (_requestModel.Overwrite || !File.Exists(subtitleFileName))
                            {
                                switch (engine)
                                {
                                    case Generators.Engines.VOSK:
                                        var voskGenerator = new Generators.Vosk(chunks, vadChunks);
                                        await voskGenerator.GenerateSubtitles(voskModelFolder, subtitleFileName);
                                        break;
                                    case Generators.Engines.WHISPER:
                                        var whisperGenerator = new Generators.Whisper(chunks, vadChunks);
                                        await whisperGenerator.GenerateSubtitles(_requestModel.Language, whisperModelFile, subtitleFileName);
                                        break;
                                }
                            }
                            else
                                Console.WriteLine($"Skipping {subtitleName} already exists.");
                        }
                    }
                    else
                        throw new Exception("Silero chunks not found... something went wrong.");

                    var endTime = DateTime.Now;
                    var totalTime = endTime - startTime;
                    Console.WriteLine($"Total job time: {totalTime:hh\\:mm\\:ss\\,fff}");

                    // Remove jobdir
                    if (!_requestModel.Keep)
                        Directory.Delete(jobDir, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
                finally
                {
                    // Don't remove anything if there are errors
                    _pythonProcess.ResetEnvironment();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void InitializePaths()
        {
            jobName = Path.GetFileNameWithoutExtension(inputFile);
            subtitleDir = Path.GetDirectoryName(inputFile);

            // Sanitize job name due to problem with non-ascii characters on demucs
            var jobNameDemucs = RegularExpressions.Expressions.SanitizeName().Replace(jobName, "_");

            workDir = Path.Combine(_baseDir, _workDir);
            if (!Directory.Exists(workDir))
                Directory.CreateDirectory(workDir);

            jobDir = Path.Combine(workDir, jobNameDemucs);
            if (!Directory.Exists(jobDir))
                Directory.CreateDirectory(jobDir);

            splitAudioDir = Path.Combine(jobDir, $"split");
            splitAudioOutput = Path.Combine(splitAudioDir, $"output_");

            audioFile = Path.Combine(jobDir, $"{jobNameDemucs}.mp3");
            demucsOutputDir = Path.Combine(jobDir, "demucs", "htdemucs");
            if (_requestModel.Split > 0)
                vocalsFile = Path.Combine(jobDir, "vocals.wav");
            else
                vocalsFile = Path.Combine(jobDir, $"{demucsOutputDir}\\{jobNameDemucs}\\vocals.wav");
            vadChunks = Path.Combine(jobDir, "vad_chunks");
            vadChunksJson = Path.Combine(vadChunks, "chunk_timestamps.json");
        }

        private List<List<SileroChunk>> ReadChunks()
        {
            var jsonData = File.ReadAllText(vadChunksJson);
            List<List<SileroChunk>> sensorList = JsonSerializer.Deserialize<List<List<SileroChunk>>>(jsonData);

            return sensorList;
        }
    }
}
