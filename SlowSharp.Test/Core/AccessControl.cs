using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class AccessControlTest
    {
        [TestInitialize]
        public void Setup()
        {
            var ac = BlacklistAccessControl.Default;
            TestRunner.config.AccessControl = ac;
        }
        [TestCleanup]
        public void Cleanup()
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
