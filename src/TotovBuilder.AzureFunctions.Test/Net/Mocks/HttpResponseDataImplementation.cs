using System;
using System.IO;
using System.Net;
using Microsoft.Azure.Functions.Worker.Http;
using TotovBuilder.AzureFunctions.Test.Functions.Mocks;

namespace TotovBuilder.AzureFunctions.Test.Net.Mocks
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
