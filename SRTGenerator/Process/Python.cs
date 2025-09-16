using SRTGenerator.Settings;

namespace SRTGenerator.Process
{
    public class Python
    {
        string actualTorchCacheFolder = null;
        string actualNoCudaCaching = null;

        public string PythonExe { get; set; }
        public string TorchCacheFolder { get; set; }
        public string Output {  get; set; }

        public Python()
        {
            var toolsConfiguration = Program.Configuration.GetToolsSettings();
            PythonExe = toolsConfiguration.PythonExe;
            Console.WriteLine($"Python: {PythonExe}"); 
            TorchCacheFolder = toolsConfiguration.TorchCacheFolder;
            Console.WriteLine($"Python Torch cache: {TorchCacheFolder}");

            if (!string.IsNullOrEmpty(PythonExe) && File.Exists(PythonExe))
                PythonExe = new FileInfo(PythonExe).FullName;
            else
                throw new ArgumentException($"Python executable not found");

            if (!string.IsNullOrEmpty(TorchCacheFolder) && Directory.Exists(TorchCacheFolder))
                TorchCacheFolder = new DirectoryInfo(TorchCacheFolder).FullName;
        }

        public void Execute(string commandArgs, string workingDirectory, bool ignoreConsoleOutput = false)
        {
            Output = "";
            var pythonProcess = new System.Diagnostics.Process();

            pythonProcess.StartInfo.WorkingDirectory = workingDirectory;
            pythonProcess.StartInfo.FileName = PythonExe;
            pythonProcess.StartInfo.UseShellExecute = false;
            pythonProcess.StartInfo.Arguments = commandArgs;
            pythonProcess.StartInfo.CreateNoWindow = true;
            pythonProcess.StartInfo.RedirectStandardOutput = true;
            pythonProcess.StartInfo.RedirectStandardError = true;
            pythonProcess.EnableRaisingEvents = true;

            if (!ignoreConsoleOutput)
                pythonProcess.OutputDataReceived += ConsoleProcess_ShowOutputDataReceived;

            pythonProcess.OutputDataReceived += ConsoleProcess_OutputDataReceived;
            pythonProcess.ErrorDataReceived += ConsoleProcess_ErrorDataReceived;
            pythonProcess.Start();

            pythonProcess.BeginOutputReadLine();
            pythonProcess.BeginErrorReadLine();

            pythonProcess.WaitForExit();
        }

        public void SetEnvironment()
        {
            if (!string.IsNullOrEmpty(TorchCacheFolder))
            {
                // Check whether the environment variable exists.
                actualTorchCacheFolder = Environment.GetEnvironmentVariable("TORCH_HOME");

                // If necessary, create it.
                string value = null;
                if (actualTorchCacheFolder == null)
                {
                    Environment.SetEnvironmentVariable("TORCH_HOME", TorchCacheFolder);

                    // Now retrieve it.
                    value = Environment.GetEnvironmentVariable("TORCH_HOME");
                }

                Console.WriteLine($"TORCH_HOME: {value}\n");
            }

            //// Check whether the environment variable exists.
            //actualNoCudaCaching = Environment.GetEnvironmentVariable("PYTORCH_NO_CUDA_MEMORY_CACHING");

            //// If necessary, create it.
            //value = null;
            //if (actualNoCudaCaching == null)
            //{
            //    Environment.SetEnvironmentVariable("PYTORCH_NO_CUDA_MEMORY_CACHING", "1");

            //    // Now retrieve it.
            //    value = Environment.GetEnvironmentVariable("PYTORCH_NO_CUDA_MEMORY_CACHING");
            //}

            //// Display the value.
            //Console.WriteLine($"PYTORCH_NO_CUDA_MEMORY_CACHING: {value}\n");
        }

        public void ResetEnvironment()
        {
            if (!string.IsNullOrEmpty(TorchCacheFolder))
            {
                Environment.SetEnvironmentVariable("TORCH_HOME", actualTorchCacheFolder);

                // Confirm the deletion.
                if (Environment.GetEnvironmentVariable("TORCH_HOME") == actualTorchCacheFolder)
                    Console.WriteLine("TORCH_HOME has been restored or deleted.");
            }

            //Environment.SetEnvironmentVariable("PYTORCH_NO_CUDA_MEMORY_CACHING", actualNoCudaCaching);

            //// Confirm the deletion.
            //if (Environment.GetEnvironmentVariable("PYTORCH_NO_CUDA_MEMORY_CACHING") == actualNoCudaCaching)
            //    Console.WriteLine("PYTORCH_NO_CUDA_MEMORY_CACHING has been restored or deleted.");
        }

        private void ConsoleProcess_ErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        private void ConsoleProcess_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            Output += e.Data;
        }

        private void ConsoleProcess_ShowOutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }
    }
}
