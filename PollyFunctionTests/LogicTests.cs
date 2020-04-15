using System;
using Amazon.S3;
using Moq;
using Xunit;
using Amazon.Polly;
using Amazon.Polly.Model;
using Amazon.S3.Model;
using LambdaSharpChallenge.PollyFunction;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Amazon.SimpleNotificationService;
using Amazon.Translate;
using System.Net.Http;
using LambdaSharpChallenge.PollyFunction.Models;
using System.IO;
using Amazon.SimpleNotificationService.Model;
using System.Collections.Generic;
using System.Linq;

namespace PollyFunctionTests {
    public class LogicTests : IDisposable {
        public LogicTests() {
            _pollyMock = new Mock<IAmazonPolly>();
            _s3Mock = new Mock<IAmazonS3>();
            _snsMock = new Mock<IAmazonSimpleNotificationService>();
            _translateMock = new Mock<IAmazonTranslate>();
            _loggerMock = new Mock<ILogger>();
            _requestHandlerMock = new Mock<HttpMessageHandler>();
            _logic = new Logic(_pollyMock.Object, _s3Mock.Object, _snsMock.Object, _translateMock.Object, _loggerMock.Object, "dummybucket", "dummysns", new HttpClient(_requestHandlerMock.Object));
        }

        private readonly Mock<IAmazonPolly> _pollyMock;
        private readonly Mock<IAmazonS3> _s3Mock;
        private readonly Mock<IAmazonTranslate> _translateMock;
        private readonly Mock<IAmazonSimpleNotificationService> _snsMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<HttpMessageHandler> _requestHandlerMock;
        private Logic _logic;

        [Fact]
        public async Task Can_convert_text_to_speech_then_save_to_s3() {
            
            // Arrange
            _pollyMock.Setup(x => x.SynthesizeSpeechAsync(It.IsAny<SynthesizeSpeechRequest>(), It.IsAny<CancellationToken>()))
                        .Returns(Task.FromResult(new SynthesizeSpeechResponse() { 
                            HttpStatusCode = (HttpStatusCode)200, 
                            AudioStream = new MemoryStream() }));

            _s3Mock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutObjectResponse {
                HttpStatusCode = (HttpStatusCode)200
            });
            _snsMock.Setup(x => x.PublishAsync(It.IsAny<PublishRequest>(), It.IsAny<CancellationToken>()));

            // Act
            await _logic.AddTextToSpeechFileToBucket(new ConvertTextRequest { Content = "Hello! This is a test!", FileName = "test.mp3" });
        }

        [Fact]
        public async Task Can_get_list_of_files() {

            // Arrange

            _s3Mock.Setup(x => x.GetAllObjectKeysAsync("dummybucket","", null))
            .ReturnsAsync(new List<string> { "file01", "file02" });

            // Act
            var response = await _logic.GetFiles();

            // Assert
            Assert.Equal(2, response.Count());
        }

        public void Dispose() {
            
            // Assert
            _pollyMock.VerifyAll();
            _s3Mock.VerifyAll();
            _snsMock.VerifyAll();
            _translateMock.VerifyAll();
        }
    }
}
