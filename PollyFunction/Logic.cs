using Amazon.Polly;
using Amazon.Polly.Model;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using LambdaSharpChallenge.PollyFunction.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LambdaSharpChallenge.PollyFunction
{
    public class Logic
    {

        private readonly IAmazonPolly _pollyClient;
        private readonly IAmazonS3 _s3Client;
        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly ILogger _logger;
        private readonly string _s3BucketName;
        private readonly string _notificationSnsTopic;

        //--- Constructors ---
        public Logic(IAmazonPolly pollyClient, IAmazonS3 s3Client, IAmazonSimpleNotificationService snsClient, ILogger logger, string s3BucketName, string notificationSnsTopic) {
            _pollyClient = pollyClient ?? throw new ArgumentNullException(nameof(pollyClient));
            _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
            _snsClient = snsClient ?? throw new ArgumentNullException(nameof(snsClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // All the audio files should be stored here.  You can check it out in the Amazon S3 Console.
            _s3BucketName = s3BucketName ?? throw new ArgumentNullException(nameof(s3BucketName));
			_notificationSnsTopic = notificationSnsTopic ?? throw new ArgumentNullException(nameof(notificationSnsTopic));
        }

        /// <summary>
        /// Convert text to audio then save it to S3
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ConvertTextResponse> AddTextToAudio(ConvertTextRequest request) {
            if (string.IsNullOrEmpty(request.Content)) {
                throw new ArgumentNullException("Content cannot be empty");
            }
            _logger.Info($"Content requested to convert: {request.Content}");
            var pollyRequest = new SynthesizeSpeechRequest {
                OutputFormat = OutputFormat.Mp3,
                Text = request.Content,
                TextType = "text",
                VoiceId = VoiceId.Amy
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
                Message = "An audio file was submitted to the S3 bucket",
                Subject = "New MP3 File"
            };
            await _snsClient.PublishAsync(publishRequest);
            return new ConvertTextResponse {
                FileName = request.FileName
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