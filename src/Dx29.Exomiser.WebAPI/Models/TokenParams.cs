using System;

using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Dx29.Models
{
    public class TokenParam
    {
        [BindRequired]
        public string Token { get; set; }
    }

    public class GetResultParam
    {
        [BindRequired]
        public string Token { get; set; }

        public string Filename { get; set; } = "results.json";
    }
}
