using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Dx29.Jobs
{
    partial class JobDispatcher
    {
        public async Task RunAsync(int minutes, CancellationToken stoppingToken)
        {
            // Last message received: Now
            LastMessageDateTime = DateTime.UtcNow;

            // Start receiving messages
            ServiceBus.RegisterMessageHandler(Options, MessageHandlerAsync, ExceptionHandler);

            // Wait x seconds with no activity to exit
            while (IsBusy || (DateTime.UtcNow - LastMessageDateTime).TotalMinutes < minutes)
            {
                if (stoppingToken.WaitHandle.WaitOne(5000))
                {
                    break;
                }
            }

            // Stop receiving messages
            await ServiceBus.CloseAsync();

            // Ensure dispatcher is idle
            await Task.Delay(5000);
            while (IsBusy)
            {
                await Task.Delay(5000);
            }
        }

        virtual protected async Task MessageHandlerAsync(Message message, CancellationToken cancellationToken)
        {
            // Set IsBusy to avoid exit process
            IsBusy = true;

            try
            {
                // Deserialize message
                string json = Encoding.UTF8.GetString(message.Body);
                var jobInfo = json.Deserialize<JobInfo>();

                // Create storage and logger
                var storage = new JobStorage(Storage, JobName, jobInfo.Token);

                // Process message
                if (await OnMessageAsync(storage, message, jobInfo))
                {
                    await ServiceBus.CompleteAsync(message);
                }
                else
                {
                    await ServiceBus.AbandonAsync(message);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("MessageHandlerAsync exception. Message: {message}. Exception: {exception}", message, ex);
                await ServiceBus.AbandonAsync(message);
            }

            // Register last message time
            LastMessageDateTime = DateTimeOffset.UtcNow;

            // Dispatcher is idle
            IsBusy = false;
        }

        protected async Task UpdateStatusAsync(JobStorage storage, Result result, string errorCode = null)
        {
            try
            {
                if (result.Success)
                {
                    await UpdateStatusAsync(storage, CommonStatus.Succeeded, result.Message);
                }
                else
                {
                    await storage.UpdateStatusAsync(result, errorCode);
                    storage.Logger.Error(result.Message, result.Details);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        protected async Task UpdateStatusAsync(JobStorage storage, CommonStatus status, string message = null)
        {
            try
            {
                await storage.UpdateStatusAsync(status.ToString(), message);
                if (message == null)
                {
                    storage.Logger.Info($"Update status: {status}...");
                }
                else
                {
                    storage.Logger.Info($"Update status: {status}. Message: {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        protected async Task UpdateStatusAsync(JobStorage storage, Exception exception)
        {
            try
            {
                await storage.UpdateStatusAsync(exception);
                storage.Logger.Error(exception);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        protected async Task<string> ReadInputStringAsync(JobStorage storage, string filename)
        {
            storage.Logger.Info($"Reading input file '{filename}'...");
            return await storage.DownloadInputStringAsync(filename);
        }
        protected async Task<byte[]> ReadInputFileAsync(JobStorage storage, string filename)
        {
            storage.Logger.Info($"Reading input file '{filename}'...");
            using (var contentStream = new MemoryStream())
            {
                var stream = await storage.DownloadInputStreamAsync(filename);
                await stream.CopyToAsync(contentStream);
                return contentStream.ToArray();
            }
        }

        protected async Task UploadOutputStringAsync(JobStorage storage, string content, string filename = "results.txt")
        {
            storage.Logger.Info($"Uploading results '{filename}'...");
            await storage.UploadOutputAsync(filename, content);
        }
    }
}
