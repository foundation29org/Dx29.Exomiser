using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;

using Dx29.Jobs;

namespace Dx29.Services
{
    public class ExomiserService
    {
        public async Task<(string, string)> PrepareAsync(JobStorage storage, string token)
        {
            // Prepare working directory
            string folder = $"/app/working/{token}";
            string output = $"{folder}/output";
            Directory.CreateDirectory(output);

            // Download input files
            await storage.DownloadInputFolderAsync(folder);

            return (folder, output);
        }

        public async Task<Result> ExecuteAsync(JobStorage jobStorage, string inputFolder, string outputFolder)
        {
            // Execute process
            string standardOutput = await ExecuteProcessAsync(jobStorage, inputFolder);
            await jobStorage.WriteLogAsync("output-end.log", standardOutput);

            var result = ParseStandardOutput(standardOutput);
            if (result.Success)
            {
                // Upload files
                await jobStorage.UploadOuputFolderAsync(outputFolder);
            }

            return result;
        }

        private static async Task<string> ExecuteProcessAsync(JobStorage jobStorage, string folder)
        {
            using (var process = new Process())
            {
                using (var writer = new StringWriter())
                {
                    process.StartInfo.FileName = "java";
                    process.StartInfo.Arguments = $"-Xms12g -Xmx12g -jar exomiser-cli-12.1.0.jar --analysis {folder}/analysis.yaml";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.WorkingDirectory = "/exomiser-cli-12.1.0";

                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.OutputDataReceived += (sender, args) => WriteOutput(jobStorage, writer, args.Data);
                    process.ErrorDataReceived += (sender, args) => WriteOutput(jobStorage, writer, args.Data);

                    process.Start();

                    process.BeginOutputReadLine();
                    while (!process.WaitForExit(5 * 60 * 1000))
                    {
                        await jobStorage.UpdateStatusAsync(CommonStatus.Running.ToString());
                    }

                    string output = writer.ToString();
                    Console.WriteLine(output);

                    return output;
                }
            }
        }

        private Result ParseStandardOutput(string output)
        {
            using (var reader = new StringReader(output))
            {
                string line = reader.ReadLine();
                while (line != null)
                {
                    if (line.Contains(" ERROR "))
                        break;
                    line = reader.ReadLine();
                }
                if (line != null)
                {
                    if (line.Contains(" ERROR "))
                    {
                        string lineError = reader.ReadLine();
                        while (lineError != null)
                        {
                            if (lineError.Contains("Exception"))
                                break;
                            lineError = reader.ReadLine();
                        }
                        if (lineError != null)
                        {
                            if (lineError.Contains("Exception"))
                            {
                                return ParseError(lineError);
                            }
                        }
                        return Result.Failed("Unknown Error", line);
                    }
                }
                return Result.Ok();
            }
        }

        private static Result ParseError(string line)
        {
            string exception = line.Split(':')[0];
            string exceptionName = exception.Split('.').Last().Trim();
            string description = line.Substring(exception.Length + 1);
            return Result.Failed(exceptionName.Trim(), description.Trim());
        }

        private static async void WriteOutput(JobStorage jobStorage, StringWriter writer, string data)
        {
            Console.WriteLine(data);
            writer.WriteLine(data);
            await jobStorage.WriteLogAsync("output.log", writer.ToString());
        }
    }
}
