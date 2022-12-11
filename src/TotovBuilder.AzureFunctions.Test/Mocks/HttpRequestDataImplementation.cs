using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using Microsoft.Azure.Functions.Worker.Http;

namespace TotovBuilder.AzureFunctions.Test.Mocks
{
    public class HttpRequestDataImplementation : HttpRequestData
    {
        public HttpRequestDataImplementation() : base(FunctionContextImplementation.CreateMock().Object) { }

        public override HttpResponseData CreateResponse()
        {
            throw new NotImplementedException();
        }

        public override Stream Body => throw new NotImplementedException();

        public override HttpHeadersCollection Headers => throw new NotImplementedException();

        public override IReadOnlyCollection<IHttpCookie> Cookies => throw new NotImplementedException();

        public override Uri Url => throw new NotImplementedException();

        public override IEnumerable<ClaimsIdentity> Identities => throw new NotImplementedException();

        public override string Method => throw new NotImplementedException();
    }
}
