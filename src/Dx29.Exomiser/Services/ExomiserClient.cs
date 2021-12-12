using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using Dx29.Jobs;
using Dx29.Exomiser;

namespace Dx29.Services
{
    public class ExomiserClient : JobClient
    {
        public ExomiserClient(BlobStorage blobStorage, ServiceBus serviceBus) : base(blobStorage, serviceBus)
        {
        }

        public override string JobName => "Exomiser";

        public async Task<JobInfo> PrepareJobAsync(ExomiserRequest request, string vcfSource, string vcfExtension, string pedSource)
        {
            var analysis = request.AsExomiserAnalysis();
            analysis.analysis.vcf = null;
            analysis.analysis.ped = null;

            var jobInfo = await CreateNewAsync("Process", request.UserId, request.CaseId, request.ResourceId, request.NotificationUrl);

            string genotype = $"genotype{vcfExtension}";
            analysis.analysis.vcf = $"/app/working/{jobInfo.Token}/{genotype}";
            var operationId = await CopyInputFromUriAsync(jobInfo.Token, genotype, new Uri(vcfSource));
            jobInfo.CopyOperation = new CopyOperation { Id = operationId, Name = genotype };

            if (pedSource != null)
            {
                string ped = $"genotype.ped";
                analysis.analysis.ped = $"/app/working/{jobInfo.Token}/{ped}";
                await SyncCopyInputFromUriAsync(jobInfo.Token, ped, new Uri(pedSource));
            }

            analysis.outputOptions.outputPrefix = $"/app/working/{jobInfo.Token}/output/results";

            await UploadInputAsync(jobInfo.Token, "request.json", request.Serialize());
            await UploadInputAsync(jobInfo.Token, "analysis.yaml", analysis.Serialize());

            return jobInfo;
        }

        public async Task<JobInfo> PrepareJobAsync(ExomiserRequest request, IList<IFormFile> files)
        {
            var analysis = request.AsExomiserAnalysis();
            analysis.analysis.vcf = null;
            analysis.analysis.ped = null;

            var jobInfo = await CreateNewAsync("Process", request.UserId, request.CaseId, request.ResourceId, request.NotificationUrl);

            foreach (var file in files)
            {
                string fn = file.FileName.ToLower();
                if (fn.EndsWith(".vcf"))
                {
                    analysis.analysis.vcf = $"/app/working/{jobInfo.Token}/genotype.vcf";
                    await UploadInputAsync(jobInfo.Token, "genotype.vcf", file);
                }
                else if (fn.EndsWith(".vcf.gz"))
                {
                    analysis.analysis.vcf = $"/app/working/{jobInfo.Token}/genotype.vcf.gz";
                    await UploadInputAsync(jobInfo.Token, "genotype.vcf.gz", file);
                }
                else if (fn.EndsWith(".ped"))
                {
                    analysis.analysis.ped = $"/app/working/{jobInfo.Token}/genotype.ped";
                    await UploadInputAsync(jobInfo.Token, "genotype.ped", file);
                }
            }
            analysis.outputOptions.outputPrefix = $"/app/working/{jobInfo.Token}/output/results";

            await UploadInputAsync(jobInfo.Token, "request.json", request.Serialize());
            await UploadInputAsync(jobInfo.Token, "analysis.yaml", analysis.Serialize());

            return jobInfo;
        }
    }
}
