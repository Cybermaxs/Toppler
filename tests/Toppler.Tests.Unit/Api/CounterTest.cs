﻿using Moq;
using StackExchange.Redis;
using System;
using Toppler.Api;
using Toppler.Core;
using Toppler.Extensions;
using Toppler.Redis;
using Xunit;

namespace Toppler.Tests.Unit.Api
{
    public class CounterTest
    {
        [Fact]
        public void Ctor_WhenNullProvider_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                ITopplerContext context = new TopplerContext(Constants.DefaultNamespace, 1, null);
                var api = new Counter(null, context);

            });
        }

        [Fact]
        public void Ctor_WhenNullContext_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var provider = new Mock<IRedisConnection>();
                var api = new Counter(provider.Object, null);
            });
        }

        [Fact]
        public void HitAsync_WhenEmptySource_ShouldFailed()
        {
            var provider = new Mock<IRedisConnection>();
            ITopplerContext context = new TopplerContext(Constants.DefaultNamespace, Constants.DefaultRedisDb, null);
            var api = new Counter(provider.Object, context);

            var execTask = api.HitAsync(new string[] { "" });

            Assert.NotNull(execTask);
            Assert.False(execTask.Result);
        }

        [Fact]
        public void HitAsync_WhenNotUtc_ShouldFailed()
        {
            var provider = new Mock<IRedisConnection>();
            ITopplerContext context = new TopplerContext(Constants.DefaultNamespace, Constants.DefaultRedisDb, null);
            var api = new Counter(provider.Object, context);

            var execTask = api.HitAsync(new string[] { "test" }, 1, occurred: DateTime.Now);

            Assert.NotNull(execTask);
            Assert.False(execTask.Result);
        }


        [Fact]
        public void HitAsync_Default_ShouldPass()
        {
            //basic setups
            var mockOfTransaction = new Mock<ITransaction>();
            mockOfTransaction.Setup(b => b.SetAddAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue[]>(), It.IsAny<CommandFlags>())).ReturnsAsync(1);
            mockOfTransaction.Setup(b => b.SortedSetIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<double>(), It.IsAny<CommandFlags>())).ReturnsAsync(0D);
            mockOfTransaction.Setup(b => b.KeyExpireAsync(It.IsAny<RedisKey>(), It.IsAny<DateTime?>(), It.IsAny<CommandFlags>())).ReturnsAsync(true);
            mockOfTransaction.Setup(b => b.ExecuteAsync(It.IsAny<CommandFlags>())).ReturnsAsync(true);

            var mockOfDatabase = new Mock<IDatabase>();
            mockOfDatabase.Setup(m => m.CreateTransaction(It.IsAny<object>())).Returns(mockOfTransaction.Object);

            var mockOfConnectionProvider = new Mock<IRedisConnection>();
            mockOfConnectionProvider.Setup(p => p.GetDatabase(It.IsAny<int>())).Returns(mockOfDatabase.Object);

            //namespace is set to "ping"
            ITopplerContext context = new TopplerContext("ping", Constants.DefaultRedisDb, new Granularity[] { Granularity.Second, Granularity.Minute, Granularity.Hour, Granularity.Day });

            var api = new Counter(mockOfConnectionProvider.Object, context);

            var ts = 405924910L; // approx. my birthday !
            var dt = ts.ToDateTime();

            var ts_minute = 405924900L;
            var ts_hour = 405921600L;
            var ts_day = 405907200L;

            var execTask = api.HitAsync(new string[] { "pong" }, 10L, new string[] { "mycontext" }, ts.ToDateTime());

            Assert.NotNull(execTask);
            Assert.True(execTask.Result);

            mockOfTransaction.Verify(b => b.ExecuteAsync(It.IsAny<CommandFlags>()), Times.Once);
            //set
            mockOfTransaction.Verify(b => b.SetAddAsync("ping:" + Constants.SetAllDimensions, new RedisValue[] { "mycontext" }, It.IsAny<CommandFlags>()), Times.Once);

            mockOfTransaction.Verify(b => b.SortedSetIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<double>(), It.IsAny<CommandFlags>()), Times.Exactly(4));
            mockOfTransaction.Verify(b => b.SortedSetIncrementAsync("ping:mycontext:second:" + ts, "pong", 10D, It.IsAny<CommandFlags>()), Times.Once);
            mockOfTransaction.Verify(b => b.SortedSetIncrementAsync("ping:mycontext:minute:" + ts_minute, "pong", 10D, It.IsAny<CommandFlags>()), Times.Once);
            mockOfTransaction.Verify(b => b.SortedSetIncrementAsync("ping:mycontext:hour:" + ts_hour, "pong", 10D, It.IsAny<CommandFlags>()), Times.Once);
            mockOfTransaction.Verify(b => b.SortedSetIncrementAsync("ping:mycontext:day:" + ts_day, "pong", 10D, It.IsAny<CommandFlags>()), Times.Once);

            mockOfTransaction.Verify(b => b.KeyExpireAsync(It.IsAny<RedisKey>(), It.IsAny<DateTime?>(), It.IsAny<CommandFlags>()), Times.Exactly(4));
            mockOfTransaction.Verify(b => b.KeyExpireAsync("ping:mycontext:second:" + ts, (ts + 7200).ToDateTime(), It.IsAny<CommandFlags>()), Times.Once);
            mockOfTransaction.Verify(b => b.KeyExpireAsync("ping:mycontext:minute:" + ts_minute, (ts_minute + 172800).ToDateTime(), It.IsAny<CommandFlags>()), Times.Once);
            mockOfTransaction.Verify(b => b.KeyExpireAsync("ping:mycontext:hour:" + ts_hour, (ts_hour + 1209600).ToDateTime(), It.IsAny<CommandFlags>()), Times.Once);
            mockOfTransaction.Verify(b => b.KeyExpireAsync("ping:mycontext:day:" + ts_day, (ts_day + 5184000).ToDateTime(), It.IsAny<CommandFlags>()), Times.Once);
        }

        [Fact]
        public void HitAsync_Default_WhenLocalGranularities_ShouldPass()
        {
            //basic setups
            var mockOfTransaction = new Mock<ITransaction>();
            mockOfTransaction.Setup(b => b.SetAddAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue[]>(), It.IsAny<CommandFlags>())).ReturnsAsync(1);
            mockOfTransaction.Setup(b => b.SortedSetIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<double>(), It.IsAny<CommandFlags>())).ReturnsAsync(0D);
            mockOfTransaction.Setup(b => b.KeyExpireAsync(It.IsAny<RedisKey>(), It.IsAny<DateTime?>(), It.IsAny<CommandFlags>())).ReturnsAsync(true);
            mockOfTransaction.Setup(b => b.ExecuteAsync(It.IsAny<CommandFlags>())).ReturnsAsync(true);

            var mockOfDatabase = new Mock<IDatabase>();
            mockOfDatabase.Setup(m => m.CreateTransaction(It.IsAny<object>())).Returns(mockOfTransaction.Object);

            var mockOfConnectionProvider = new Mock<IRedisConnection>();
            mockOfConnectionProvider.Setup(p => p.GetDatabase(It.IsAny<int>())).Returns(mockOfDatabase.Object);

            //namespace is set to "ping"
            ITopplerContext context = new TopplerContext("ping", Constants.DefaultRedisDb, new Granularity[] { Granularity.Second, Granularity.Minute, Granularity.Hour, Granularity.Day });

            var api = new Counter(mockOfConnectionProvider.Object, context);

            var ts = 405924910L; // approx. my birthday !
            var dt = ts.ToDateTime();

            var ts_day = 405907200L;

            var execTask = api.HitAsync(new string[] { "pong" }, 10L, new string[] { "mycontext" }, ts.ToDateTime(), new Granularity[] { Granularity.Day, Granularity.AllTime });

            Assert.NotNull(execTask);
            Assert.True(execTask.Result);

            mockOfTransaction.Verify(b => b.ExecuteAsync(It.IsAny<CommandFlags>()), Times.Once);
            //set
            mockOfTransaction.Verify(b => b.SetAddAsync("ping:" + Constants.SetAllDimensions, new RedisValue[] { "mycontext" }, It.IsAny<CommandFlags>()), Times.Once);

            mockOfTransaction.Verify(b => b.SortedSetIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<double>(), It.IsAny<CommandFlags>()), Times.Exactly(2));
            mockOfTransaction.Verify(b => b.SortedSetIncrementAsync("ping:mycontext:day:" + ts_day, "pong", 10D, It.IsAny<CommandFlags>()), Times.Once);
            mockOfTransaction.Verify(b => b.SortedSetIncrementAsync("ping:mycontext:alltime:" + 0.ToString(), "pong", 10D, It.IsAny<CommandFlags>()), Times.Once);

            mockOfTransaction.Verify(b => b.KeyExpireAsync(It.IsAny<RedisKey>(), It.IsAny<DateTime?>(), It.IsAny<CommandFlags>()), Times.Exactly(2));
            mockOfTransaction.Verify(b => b.KeyExpireAsync("ping:mycontext:day:" + ts_day, (ts_day + 5184000).ToDateTime(), It.IsAny<CommandFlags>()), Times.Once);
            mockOfTransaction.Verify(b => b.KeyExpireAsync("ping:mycontext:alltime:" + 0, ((long)int.MaxValue).ToDateTime(), It.IsAny<CommandFlags>()), Times.Once);
        }
    }
}
