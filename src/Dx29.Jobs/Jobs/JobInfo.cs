using System;
using System.Collections.Generic;

namespace Dx29.Jobs
{
    public class JobInfo
    {
        public string Name { get; set; }
        public string Token { get; set; }

        public string Command { get; set; }
        public CopyOperation CopyOperation { get; set; }

        public string UserId { get; set; }
        public string CaseId { get; set; }
        public string ResourceId { get; set; }
        public string NotificationUrl { get; set; }

        public IDictionary<string, string> Args { get; set; }

        public string GetParameter(string name) => Args.TryGetValue(name);
    }

    public class CopyOperation
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
