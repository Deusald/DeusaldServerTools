using System;

namespace DeusaldServerToolsClient
{
    public class VerificationException : Exception
    {
        public VerificationException(string message) : base(message) { }
    }
}