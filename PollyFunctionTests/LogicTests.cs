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

namespace PollyFunctionTests {
    public class LogicTests : IDisposable {
        public LogicTests() {
            _pollyMock = new Mock<IAmazonPolly>();
            _s3Mock = new Mock<IAmazonS3>();
            _snsMock = new Mock<IAmazonSimpleNotificationService>();
            _loggerMock = new Mock<ILogger>();
            _logic = new Logic(_pollyMock.Object, _s3Mock.Object, _snsMock.Object, _loggerMock.Object, "dummybucket", "dummysns");
        }

        private readonly Mock<IAmazonPolly> _pollyMock;
        private readonly Mock<IAmazonS3> _s3Mock;
        private readonly Mock<IAmazonSimpleNotificationService> _snsMock;
        private readonly Mock<ILogger> _loggerMock;
        private Logic _logic;

        [Fact]
        public async Task CanSendToPolly() {
            //arrange
            _pollyMock.Setup(x => x.SynthesizeSpeechAsync(It.IsAny<SynthesizeSpeechRequest>(), It.IsAny<CancellationToken>()))
                        .Returns(Task.FromResult(new SynthesizeSpeechResponse() { HttpStatusCode = (HttpStatusCode)200 }));

            _s3Mock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutObjectResponse {
                HttpStatusCode = (HttpStatusCode)200
            });

            //act
            //await _logic.AddItem(new ConvertTextRequest { Content = "foo bar", FileName = "baz" });
        }

        public void Dispose() {
            
            // Assert
            _pollyMock.VerifyAll();
            _s3Mock.VerifyAll();
            _snsMock.VerifyAll();
        }
    }
}
