using System;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;

using Dx29.Jobs;
using Dx29.Services;

namespace Dx29.Exomiser
{
    public class ExomiserDispatcher : JobDispatcher
    {
#if  DEBUG
        const int DELAY_MINUTES = 1;
#else
        const int DELAY_MINUTES = 20;
#endif

        public ExomiserDispatcher(ExomiserService annotationService, ServiceBus serviceBus, BlobStorage storage, ILogger<ExomiserDispatcher> logger) : base(serviceBus, storage, logger)
        {
            ExomiserService = annotationService;
        }

        public ExomiserService ExomiserService { get; }

        public override string JobName => "Exomiser";

        protected override async Task<bool> OnMessageAsync(JobStorage jobStorage, Message message, JobInfo jobInfo)
        {
            try
            {
                // Get JobStatus
                var jobStatus = await jobStorage.GetJobStatusAsync();
                if (jobStatus != null)
                {
                    if (await HandleExistingJobAsync(jobStorage, jobStatus, jobInfo))
                    {
                        return true;
                    }
                }

                // Preparing
                await UpdateStatusAsync(jobStorage, CommonStatus.Preparing);
                await WaitForCopyOperationAsync(jobStorage, jobInfo);
                (string input, string output) = await ExomiserService.PrepareAsync(jobStorage, jobInfo.Token);

                // Execute
                await UpdateStatusAsync(jobStorage, CommonStatus.Running);
                var result = await ExomiserService.ExecuteAsync(jobStorage, input, output);

                // Update status by Result
                if (result.Success)
                {
                    await UpdateStatusAsync(jobStorage, result);
                }
                else
                {
                    await UpdateStatusAsync(jobStorage, result, ExomiserErrors.GetErrorCode(result));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("OnMessageAsync Exception: {exception}", ex);

                // Log exception
                jobStorage.Logger.Error(ex);

                // Update status: Failed
                await UpdateStatusAsync(jobStorage, ex);
            }

            // Send complete notification
            await SendNotificationAsync(jobInfo);

            // Complete
            return true;
        }

        private async Task WaitForCopyOperationAsync(JobStorage jobStorage, JobInfo jobInfo)
        {
            if (jobInfo.CopyOperation != null)
            {
                bool complete = await jobStorage.WaitForCopyInputOperationAsync(jobInfo.CopyOperation.Name, jobInfo.CopyOperation.Id, timeout: 300);
                if (!complete)
                {
                    throw new TimeoutException("Timeout waiting for copy operation");
                }
            }
        }

        private async Task<bool> HandleExistingJobAsync(JobStorage jobStorage, JobStatus status, JobInfo jobInfo)
        {
            if (status.Status == "Created") return false;

            if (status.Status == "Succeeded") return true;
            if (status.Status == "Failed") return true;

            var elapsed = DateTime.UtcNow - status.LastUpdate;
            if (elapsed.TotalMinutes > 6)
            {
                // Set job as failed
                await UpdateStatusAsync(jobStorage, CommonStatus.Failed, "No response");
                // Send complete notification
                await SendNotificationAsync(jobInfo);
            }
            else
            {
                await ServiceBus.SendMessageAsync(jobInfo, delayMinutes: DELAY_MINUTES);
            }
            return true;
        }

        public async Task SendNotificationAsync(JobInfo jobInfo)
        {
            if (!String.IsNullOrEmpty(jobInfo.NotificationUrl))
            {
                var notification = new ExomiserNotification
                {
                    UserId = jobInfo.UserId,
                    CaseId = jobInfo.CaseId,
                    ResourceId = jobInfo.ResourceId,
                    Token = jobInfo.Token
                };
                for (int n = 0; n < 3; n++)
                {
                    try
                    {
                        var http = new HttpClient { BaseAddress = new Uri(jobInfo.NotificationUrl) };
                        await http.POSTAsync("", notification);
                        return;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    await Task.Delay(10_000);
                }
            }
        }
    }
}
