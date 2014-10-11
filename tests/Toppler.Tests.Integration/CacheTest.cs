﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Toppler.Core;
using System.Diagnostics;
using System.Threading.Tasks;
using Toppler.Tests.Integration.TestHelpers;
using Toppler.Extensions;

namespace Toppler.Tests.Integration
{
    [TestClass]
    public class CacheTest : TestBase
    {
        #region TestInit & CleanUp
        [TestInitialize]
        public void TestInit()
        {
            this.Reset();
            this.StartMonitor();
        }


        [TestCleanup]
        public void TestCleanUp()
        {
            this.StopMonitor();
        }
        #endregion

        [TestMethod]
        [TestCategory("Integration")]
        public void BasicCache()
        {
            var now = DateTime.UtcNow;

            var current = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, DateTimeKind.Utc);

            foreach (var i in Enumerable.Range(1, 10))
            {
                Topp.Counter.HitAsync(new string[] {this.TestEventSource},1L, new string[] {this.TestDimension},  current);
                current = current.AddHours(-i);
            }

            var res1 = Topp.Ranking.GetTops(Granularity.Hour, 5, dimension: this.TestDimension).Result;
           Assert.IsNotNull(res1);
           var res2 = Topp.Ranking.GetTops(Granularity.Hour, 5, dimension: this.TestDimension).Result;
           var res3 = Topp.Ranking.GetTops(Granularity.Hour, 5, dimension: this.TestDimension).Result;


        }
    }
}
