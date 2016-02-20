using System;
using handyNews.Domain.Services;
using handyNews.UWP.Services;
using Microsoft.ApplicationInsights;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace handyNews.Domain.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var localStorageManager = new LocalStorageManager(new ApplicationInsightsTelemetryManager(new TelemetryClient()));
            var t = localStorageManager.LoadSubscriptionsAsync().Result;
        }
    }
}
