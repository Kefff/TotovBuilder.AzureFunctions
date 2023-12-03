using System;
using System.Collections.Generic;
using Microsoft.Azure.Functions.Worker;
using Moq;

namespace TotovBuilder.AzureFunctions.Test.Functions.Mocks
{
    public class FunctionContextImplementation : FunctionContext
    {
        public static Mock<FunctionContextImplementation> CreateMock()
        {
            Mock<FunctionContextImplementation> mock = new Mock<FunctionContextImplementation>();

            return mock;
        }

        public FunctionContextImplementation()
            : base()
        { }

        public override string InvocationId
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string FunctionId
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override TraceContext TraceContext
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override BindingContext BindingContext
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override RetryContext RetryContext
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override IServiceProvider InstanceServices
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override FunctionDefinition FunctionDefinition
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override IDictionary<object, object> Items
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override IInvocationFeatures Features
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
