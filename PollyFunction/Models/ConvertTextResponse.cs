using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LambdaSharpChallenge.PollyFunction.Models
{
    public class ConvertTextResponse {
        
        //--- Properties ---
        [JsonRequired]
        public string FileName { get; set; }
    }
}
