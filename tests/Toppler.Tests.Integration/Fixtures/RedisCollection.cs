﻿using Xunit;

namespace Toppler.Tests.Integration.Fixtures
{
    [CollectionDefinition("RedisServer")]
    public class RedisCollection : ICollectionFixture<RedisServerFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
