using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class AccessControlTest
    {
        [TestInitialize]
        public static void Setup()
        {
            var ac = BlacklistAccessControl.Default;
            TestRunner.config.accessControl = ac;
        }
        [TestCleanup]
        public static void Cleanup()
        {
            TestRunner.config = RunConfig.Default;
        }

        [TestMethod]
        public void DisallowedType()
        {
            Assert.ThrowsException<SandboxException>(() => {
                TestRunner.Run(@"
Task.Delay(100);
");
            });
        }
    }
}
