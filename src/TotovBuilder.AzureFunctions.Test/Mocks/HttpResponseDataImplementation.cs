using System;
using System.IO;
using System.Net;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;

namespace TotovBuilder.AzureFunctions.Test.Mocks
{
    public class HttpResponseDataImplementation : HttpResponseData
    {
        public static Mock<HttpResponseDataImplementation> CreateMock()
        {
            Mock<HttpResponseDataImplementation> mock = new();

            return mock;
        }

        public HttpResponseDataImplementation()
            : base(FunctionContextImplementation.CreateMock().Object)
        { }

        public override HttpStatusCode StatusCode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override HttpHeadersCollection Headers { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override Stream Body { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override HttpCookies Cookies => throw new NotImplementedException();
    }
}
