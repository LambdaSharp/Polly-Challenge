using Amazon.Lambda.Core;
using Amazon.Polly;
using Amazon.S3;
using LambdaSharp;
using LambdaSharp.ApiGateway;
using Amazon.SimpleNotificationService;
using System.Threading.Tasks;
using System;
using LambdaSharpChallenge.PollyFunction.Models;
using System.Collections.Generic;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace LambdaSharpChallenge.PollyFunction
{

    public class Function : ALambdaApiGatewayFunction, ILogger
    {
        Logic Logic { get; set; }

        //--- Methods ---
        public override Task InitializeAsync(LambdaConfig config) {

            // NOTE: All the logic of the lambda function can be found in this `Logic` class.  Please leave this file untouched.
            Logic = new Logic(new AmazonPollyClient(),
            new AmazonS3Client(),
            new AmazonSimpleNotificationServiceClient(),
            this,
            config.ReadS3BucketName("ArticlesBucket"),
            config.ReadText("ArticlesTopic"));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Incoming request from API Gateway that is redirect to a Logic class for text to audio conversion
        /// </summary>
		/// <see cref="Logic.AddTextToAudio(ConvertTextRequest)">Forwards request to Logic class</see>
		public async Task<ConvertTextResponse> AddTextToAudio(ConvertTextRequest request) => await Logic.AddTextToAudio(request);

        /// <summary>
        /// Incoming request to get all files listed in S3 bucket. 
        /// </summary>
        /// <see cref="Logic.GetFiles()">Forwards request to Logic class</see>
        public async Task<IEnumerable<GetFilesResponse>> GetFiles() => await Logic.GetFiles();

        // Logging function mapping to lambda function
        public void Error(Exception ex) => LogError(ex);

        public void Warn(string message) => LogWarn(message);

        void ILogger.Info(string message) => LogInfo(message);
    }
}