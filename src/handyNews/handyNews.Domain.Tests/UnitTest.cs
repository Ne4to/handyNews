using handyNews.Domain.Services;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace handyNews.Domain.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var localStorageManager = new LocalStorageManager();
            var t = localStorageManager.LoadSubscriptionsAsync()
                .Result;
        }
    }
}