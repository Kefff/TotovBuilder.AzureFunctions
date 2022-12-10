using System;
using System.Collections.Generic;
using Microsoft.Azure.Functions.Worker;
using Moq;

namespace TotovBuilder.AzureFunctions.Test.Mocks
{
    public class FunctionContextImplementation : FunctionContext
    {
        public static Mock<FunctionContextImplementation> CreateMock()
        {
            Mock<FunctionContextImplementation> mock = new();

            return mock;
        }

        public FunctionContextImplementation()
            : base()
        { }

        public override string InvocationId => throw new NotImplementedException();

        public override string FunctionId => throw new NotImplementedException();

        public override TraceContext TraceContext => throw new NotImplementedException();

        public override BindingContext BindingContext => throw new NotImplementedException();

        public override RetryContext RetryContext => throw new NotImplementedException();

        public override IServiceProvider InstanceServices { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override FunctionDefinition FunctionDefinition => throw new NotImplementedException();

        public override IDictionary<object, object> Items { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override IInvocationFeatures Features => throw new NotImplementedException();
    }
}
