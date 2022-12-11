using System;
using System.IO;
using System.Net;
using Microsoft.Azure.Functions.Worker.Http;

namespace TotovBuilder.AzureFunctions.Test.Mocks
{
    public class HttpResponseDataImplementation : HttpResponseData
    {
        public HttpResponseDataImplementation()
            : base(FunctionContextImplementation.CreateMock().Object)
        { }

        public override HttpStatusCode StatusCode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override HttpHeadersCollection Headers { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override Stream Body { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override HttpCookies Cookies => throw new NotImplementedException();
    }
}
