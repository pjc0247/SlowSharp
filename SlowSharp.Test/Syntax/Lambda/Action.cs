using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class LambdaActionTest
    {
        [TestMethod]
        public void BasicCreation()
        {
            Assert.AreEqual(
                typeof(Action),
                TestRunner.Run(@"return new Action(() => { }").GetType());

            Assert.AreEqual(
                typeof(Action<int>),
                TestRunner.Run(@"return new Action((int p) => { }").GetType());
        }

        [TestMethod]
        public void Invocation()
        {
            Assert.AreEqual(
                10,
                TestRunner.Run(@"
var b = 0;
var a = new Action(() => {
    b = 10;
});
a();
return b;
"));
        }
    }
}
