using System;
using System.Collections.Generic;
using System.Text;

namespace LambdaSharpChallenge.PollyFunction
{
    public interface ILogger {
        void Error(Exception ex);
        void Warn(string message);
        void Info(string message);
    }
}
