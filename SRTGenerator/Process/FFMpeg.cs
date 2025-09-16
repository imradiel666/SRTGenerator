using SRTGenerator.Settings;

namespace SRTGenerator.Process
{
    public class FFMpeg
    {
        public string FFMpegExe { get; set; }

        public FFMpeg()
        {
            var toolsConfiguration = Program.Configuration.GetToolsSettings();
            FFMpegExe = toolsConfiguration.FFMpegExe;
            Console.WriteLine($"FFMpeg: {FFMpegExe}"); 

            if (!string.IsNullOrEmpty(FFMpegExe) && File.Exists(FFMpegExe))
                FFMpegExe = new FileInfo(FFMpegExe).FullName;
            else
                throw new ArgumentException($"FFMpeg executable not found");
        }

        public void Execute(string commandArgs, string workingDirectory)
        {
            var ffmpegProcess = new System.Diagnostics.Process();

            ffmpegProcess.StartInfo.WorkingDirectory = workingDirectory;
            ffmpegProcess.StartInfo.FileName = FFMpegExe;
            ffmpegProcess.StartInfo.UseShellExecute = false;
            ffmpegProcess.StartInfo.Arguments = commandArgs;
            ffmpegProcess.StartInfo.CreateNoWindow = true;
            ffmpegProcess.StartInfo.RedirectStandardOutput = true;
            ffmpegProcess.StartInfo.RedirectStandardError = true;
            ffmpegProcess.EnableRaisingEvents = true;

            ffmpegProcess.OutputDataReceived += ConsoleProcess_OutputDataReceived;
            ffmpegProcess.ErrorDataReceived += ConsoleProcess_ErrorDataReceived;
            ffmpegProcess.Start();

            ffmpegProcess.BeginOutputReadLine();
            ffmpegProcess.BeginErrorReadLine();

            ffmpegProcess.WaitForExit();
        }

        private void ConsoleProcess_ErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        private void ConsoleProcess_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }
    }
}
