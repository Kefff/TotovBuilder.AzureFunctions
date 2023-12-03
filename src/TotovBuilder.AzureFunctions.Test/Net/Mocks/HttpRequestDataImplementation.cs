using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using Microsoft.Azure.Functions.Worker.Http;
using TotovBuilder.AzureFunctions.Test.Functions.Mocks;

namespace TotovBuilder.AzureFunctions.Test.Net.Mocks
{
    public class HttpRequestDataImplementation : HttpRequestData
    {
        public HttpRequestDataImplementation() : base(FunctionContextImplementation.CreateMock().Object) { }

        public override HttpResponseData CreateResponse()
        {
            throw new NotImplementedException();
        }

        public override Stream Body
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override HttpHeadersCollection Headers
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override IReadOnlyCollection<IHttpCookie> Cookies
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override Uri Url
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override IEnumerable<ClaimsIdentity> Identities
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string Method
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
