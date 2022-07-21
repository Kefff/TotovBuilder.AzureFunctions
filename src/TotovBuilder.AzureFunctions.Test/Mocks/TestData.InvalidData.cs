namespace TotovBuilder.AzureFunctions.Test.Mocks
{
    /// <summary>
    /// Represents test data.
    /// </summary>
    public static partial class TestData
    {
        public const string EmptyApiData1 = @"{
  ""data"": {
    ""tasks"": {}
  }
}";

        public const string EmptyApiData2 = @"{
  ""data"": {
    ""tasks"": []
  }
}";

        public const string EmptyApiData3 = @"{
  ""data"": {
    ""tasks"": """"
  }
}";

        public const string InvalidApiData1 = @"{
  data"": {
    ""tasks"": null
  }
}";

        public const string InvalidApiData2 = @"{
  ""invalid"": {
    ""tasks"": null
  }
}";

        public const string InvalidApiData3 = @"{
  ""data"": {
    ""invalid"": null
  }
}";
    }
}
