using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LambdaSharpChallenge.PollyFunction.Models
{
    public class ConvertTextRequest {

        //--- Properties ---
        [JsonRequired]
        public string Content { get; set; }

        [JsonRequired]
        public string FileName { get; set; }
    }
}
