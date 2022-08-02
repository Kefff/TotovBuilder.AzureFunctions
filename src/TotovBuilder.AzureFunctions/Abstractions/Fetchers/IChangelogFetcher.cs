﻿using System.Collections.Generic;
using TotovBuilder.Model;

namespace TotovBuilder.AzureFunctions.Abstractions.Fetchers
{
    /// <summary>
    /// Provides the functionalities of a changelog fetcher.
    /// </summary>
    public interface IChangelogFetcher : IApiFetcher<IEnumerable<ChangelogEntry>>
    {
    }
}