using System;

using Dx29.Jobs;

namespace Dx29.Exomiser
{
    static public class ExomiserErrors
    {
        static public string GetErrorCode(Result result)
        {
            if (!result.Success)
            {
                switch (result.Message)
                {
                    case "TribbleException$MalformedFeatureFile":
                        return "ERR_EXOMISER_501";

                    case "IllegalArgumentException":
                        return "WRN_EXOMISER_101";

                    case "SampleMismatchException":
                        return "WRN_EXOMISER_102";

                    case "PedigreeSampleValidator$PedigreeValidationException":
                        return "WRN_EXOMISER_103";

                    case "PedFiles$PedFilesParseException":
                        return "WRN_EXOMISER_103";

                    default:
                        return "ERR_EXOMISER_000";
                }
            }
            return null;
        }

        static public ErrorDescription GetErrorDescription(JobStatus jobStatus, string lan)
        {
            string code = jobStatus.ErrorCode;
            string severity = "Error";
            string message = jobStatus.Message;
            string description = jobStatus.Details;

            switch (message)
            {
                case "TribbleException$MalformedFeatureFile":
                    severity = "Error";
                    message = "Invalid input VCF file. Your input file has a malformed header.";
                    description = "Unable to parse header: We never saw the required CHROM header line (starting with one #) for the input VCF file.";
                    break;

                case "IllegalArgumentException":
                    severity = "Warning";
                    if (jobStatus.Details.Contains("not a valid HPO identifier"))
                    {
                        message = "Some HPO identifiers are invalid for Exomiser.";
                    }
                    break;

                case "SampleMismatchException":
                    severity = "Warning";
                    if (jobStatus.Details.Contains("Proband sample name not specified"))
                    {
                        message = "Missing Proband sample name.";
                    }
                    else if (jobStatus.Details.Contains("Proband sample name"))
                    {
                        message = "Invalid Proband sample name.";
                    }
                    break;

                case "PedigreeSampleValidator$PedigreeValidationException":
                    severity = "Warning";
                    message = "Missing or invalid pedigree file.";
                    break;

                case "PedFiles$PedFilesParseException":
                    severity = "Warning";
                    message = "Invalid pedigree file.";
                    break;

                default:
                    break;
            }

            return new ErrorDescription
            {
                Code = code,
                Severity = severity,
                Message = message,
                Description = description,
                Language = lan
            };
        }
    }
}
