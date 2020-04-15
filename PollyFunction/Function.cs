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
using System.Net.Http;
using Amazon.Translate;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace LambdaSharpChallenge.PollyFunction
{

    public class Function : ALambdaApiGatewayFunction, ILogger
    {
        static HttpClient _httpClient = new HttpClient();
        Logic _logic { get; set; }

        //--- Methods ---
        public override Task InitializeAsync(LambdaConfig config) {

            // NOTE: All the logic of the lambda function can be found in this `Logic` class.
            _logic = new Logic(new AmazonPollyClient(),
            new AmazonS3Client(),
            new AmazonSimpleNotificationServiceClient(),
            new AmazonTranslateClient(),
            this,
            config.ReadS3BucketName("ArticlesBucket"),
            config.ReadText("ArticlesTopic"), 
            _httpClient);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Incoming request from API Gateway that is redirect to a Logic class for text to audio conversion
        /// </summary>
		/// <see cref="Logic.AddTextToSpeechFileToBucket(ConvertTextRequest)">Forwards request to Logic class</see>
		public async Task<ConvertTextResponse> AddTextToSpeechFileToBucket(ConvertTextRequest request) => await _logic.AddTextToSpeechFileToBucket(request);

        /// <summary>
        /// Incoming request from API Gateway to get all files listed in S3 bucket. 
        /// </summary>
        /// <see cref="Logic.GetFiles()">Forwards request to Logic class</see>
        public async Task<IEnumerable<GetFilesResponse>> GetFiles() => await _logic.GetFiles();

        /// <summary>
        /// Incoming request from API Gateway to save newsfeed text to a s3 bucket.
        /// </summary>
        /// <see cref="Logic.AddNewsFeedAudioToBucket()">Forwards request to Logic class</see>
        public async Task<ConvertTextResponse> AddNewsFeedAudioToBucket() => await _logic.AddNewsFeedAudioToBucket();

        #region Logging
        // Logging function mapping to lambda function
        public void Error(Exception ex) => LogError(ex);

        public void Warn(string message) => LogWarn(message);

        void ILogger.Info(string message) => LogInfo(message);
        #endregion
    }
}