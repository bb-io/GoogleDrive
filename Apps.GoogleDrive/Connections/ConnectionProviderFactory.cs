﻿using Blackbird.Applications.Sdk.Common;
using System.Collections.Generic;

namespace Apps.GoogleDrive.Connections
{
    public class ConnectionProviderFactory : IConnectionProviderFactory
    {
        public IEnumerable<IConnectionProvider> Create()
        {
            yield return new ConnectionProvider();
        }
    }
}
