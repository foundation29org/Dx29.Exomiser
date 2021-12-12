using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using Dx29.Services;
using Dx29.Tools;

namespace Dx29.Jobs
{
    abstract public class JobClient
    {
        public JobClient(BlobStorage blobStorage, ServiceBus serviceBus)
        {
            ServiceBus = serviceBus;
            BlobStorage = blobStorage;
        }

        abstract public string JobName { get; }

        public ServiceBus ServiceBus { get; }
        public BlobStorage BlobStorage { get; }

        public async Task<JobInfo> CreateNewAsync(string command, string userId, string caseId, string resourceId, string notificationUrl, params (string, string)[] args)
        {
            var token = IDGenerator.GenerateToken();
            var argumens = args?.Select(r => new KeyValuePair<string, string>(r.Item1, r.Item2));
            var jobInfo = new JobInfo
            {
                Name = JobName,
                Token = token,
                Command = command,
                UserId = userId,
                CaseId = caseId,
                ResourceId = resourceId,
                NotificationUrl = notificationUrl,
                Args = new Dictionary<string, string>(argumens)
            };
            var jobStorage = new JobStorage(BlobStorage, JobName, token);
            await jobStorage.InitializeJobAsync(jobInfo);
            return jobInfo;
        }

        public async Task<JobInfo> GetJobInfoAsync(string token)
        {
            var jobStorage = new JobStorage(BlobStorage, JobName, token);
            return await jobStorage.DownloadJobInfoAsync();
        }

        public async Task<JobStatus> SendMessageAsync(JobInfo jobInfo)
        {
            await ServiceBus.SendMessageAsync(jobInfo);
            return await GetStatusAsync(jobInfo.Token);
        }

        public async Task<JobStatus> GetStatusAsync(string token)
        {
            var jobStorage = new JobStorage(BlobStorage, JobName, token);
            return await jobStorage.GetJobStatusAsync();
        }

        public async Task<string> CopyInputFromUriAsync(string token, string name, Uri sourceUri)
        {
            var jobStorage = new JobStorage(BlobStorage, JobName, token);
            return await jobStorage.CopyInputFromUriAsync(name, sourceUri);
        }

        public async Task<bool> WaitForCopyInputOperationAsync(string token, string name, string operationId, int timeout = 5 * 60)
        {
            var jobStorage = new JobStorage(BlobStorage, JobName, token);
            return await jobStorage.WaitForCopyInputOperationAsync(name, operationId, timeout);
        }

        public async Task<long> SyncCopyInputFromUriAsync(string token, string name, Uri sourceUri)
        {
            var jobStorage = new JobStorage(BlobStorage, JobName, token);
            return await jobStorage.SyncCopyInputFromUriAsync(name, sourceUri);
        }

        public async Task UploadInputAsync(string token, string name, object obj)
        {
            var jobStorage = new JobStorage(BlobStorage, JobName, token);
            await jobStorage.UploadInputAsync(name, obj);
        }
        public async Task UploadInputAsync(string token, string name, string content)
        {
            var jobStorage = new JobStorage(BlobStorage, JobName, token);
            await jobStorage.UploadInputAsync(name, content);
        }
        public async Task UploadInputAsync(string token, string name, IFormFile file)
        {
            using (var stream = file.OpenReadStream())
            {
                await UploadInputAsync(token, name, stream);
            }
        }
        public async Task UploadInputAsync(string token, string name, Stream stream)
        {
            var jobStorage = new JobStorage(BlobStorage, JobName, token);
            await jobStorage.UploadInputAsync(name, stream);
        }

        public async Task<T> DownloadOutputObjectAsync<T>(string token, string name)
        {
            var jobStorage = new JobStorage(BlobStorage, JobName, token);
            return await jobStorage.DownloadOutputObjectAsync<T>(name);
        }
        public async Task<string> DownloadOutputStringAsync(string token, string name)
        {
            var jobStorage = new JobStorage(BlobStorage, JobName, token);
            return await jobStorage.DownloadOutputStringAsync(name);
        }
        public async Task<Stream> DownloadOutputStreamAsync<T>(string token, string name)
        {
            var jobStorage = new JobStorage(BlobStorage, JobName, token);
            return await jobStorage.DownloadOutputStreamAsync(name);
        }
    }
}
