using Amazon.Polly;
using Amazon.Polly.Model;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.Translate;
using LambdaSharpChallenge.PollyFunction.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.Translate.Model;

namespace LambdaSharpChallenge.PollyFunction
{
    public class Logic
    {
        private readonly IAmazonPolly _pollyClient;
        private readonly IAmazonS3 _s3Client;
        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly IAmazonTranslate _translateClient;
        private readonly ILogger _logger;
        private readonly string _s3BucketName;
        private readonly string _notificationSnsTopic;
        private readonly HttpClient _httpClient;

        //--- Constructors ---
        public Logic(IAmazonPolly pollyClient, IAmazonS3 s3Client, IAmazonSimpleNotificationService snsClient, IAmazonTranslate translateClient, ILogger logger, string s3BucketName, string notificationSnsTopic, HttpClient httpClient) {
            _pollyClient = pollyClient ?? throw new ArgumentNullException(nameof(pollyClient));
            _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
            _snsClient = snsClient ?? throw new ArgumentNullException(nameof(snsClient));
            _translateClient = translateClient ?? throw new ArgumentNullException(nameof(translateClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            // All the audio files should be stored here.  You can check it out in the Amazon S3 Console.
            _s3BucketName = s3BucketName ?? throw new ArgumentNullException(nameof(s3BucketName));
			_notificationSnsTopic = notificationSnsTopic ?? throw new ArgumentNullException(nameof(notificationSnsTopic));
        }

        /// <summary>
        /// Convert text to speech to an audio file then save it to a S3 bucket
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ConvertTextResponse> AddTextToSpeechFileToBucket(ConvertTextRequest request) {
            if (string.IsNullOrEmpty(request.Content)) {
                throw new ArgumentNullException("Content cannot be empty");
            }
            _logger.Info($"Content requested to convert: {request.Content}");
            var pollyRequest = new SynthesizeSpeechRequest {
                OutputFormat = OutputFormat.Mp3,
                Text = request.Content,
                TextType = "text",
                VoiceId = VoiceId.Amy, // https://docs.aws.amazon.com/sdkfornet/v3/apidocs/items/Polly/TVoiceId.html
                LanguageCode =  LanguageCode.EnUS // https://docs.aws.amazon.com/sdkfornet/v3/apidocs/items/Polly/TLanguageCode.html
            };
            
            // Send request to Amazon Polly to convert text to audio.
            var synthesizeSpeechResponse = await _pollyClient.SynthesizeSpeechAsync(pollyRequest);
            if (synthesizeSpeechResponse == null
                || synthesizeSpeechResponse.HttpStatusCode != (HttpStatusCode)200) {
                throw new Exception("Text could not be converted to audio.");
            }
            _logger.Info($"Requested characters was: {synthesizeSpeechResponse.RequestCharacters}");

            // Stream the audio from Amazon Polly to a S3 bucket using the filename provided by the request
            PutObjectResponse s3Response;
            using (var memoryStream = new MemoryStream()) {
                synthesizeSpeechResponse.AudioStream.CopyTo(memoryStream);
                s3Response = await _s3Client.PutObjectAsync(new PutObjectRequest {
                    BucketName = _s3BucketName,
                    Key = request.FileName,
                    InputStream = memoryStream
                });
            }
            if (s3Response == null
                || s3Response.HttpStatusCode != (HttpStatusCode)200) {
                throw new Exception("Unable to save audio file to s3");
            }

            // Publish to a SNS topic that a new audio file has been saved.
            var publishRequest = new PublishRequest {
                TopicArn = _notificationSnsTopic,
                Message = "An audio file was saved",
                Subject = "New MP3 File"
            };
            await _snsClient.PublishAsync(publishRequest);
            return new ConvertTextResponse {
                FileName = request.FileName
            };
        }

        public async Task<ConvertTextResponse> AddNewsFeedAudioToBucket() {
            _logger.Info("News feed method invoked");

            // Use this HttpClient to make a request to News feed source of your choice!
            // Some docs: https://docs.microsoft.com/en-us/dotnet/csharp/tutorials/console-webapiclient#processing-the-json-result
            // var response = await _httpClient.GetAsync("https://somenewsfeed");

            // This translate client may be useful later. See example translate https://docs.aws.amazon.com/translate/latest/dg/examples-polly.html
            // translateClient

            return new ConvertTextResponse {
                FileName = "newsfeedsummary.mp3"
            };
        }

        /// <summary>
        /// Get a list of audio files from S3
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<GetFilesResponse>> GetFiles() {
            var response = await _s3Client.GetAllObjectKeysAsync(_s3BucketName, "", null);
            return response.Select(key => new GetFilesResponse { Name = key });
        }
    }
}