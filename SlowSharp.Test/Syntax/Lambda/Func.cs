using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class LambdaFuncTest
    {
        [TestMethod]
        public void BasicCreation()
        {
            Assert.AreEqual(
                typeof(Func<int>),
                TestRunner.Run(@"return new Func(() => { return 1; }").GetType());

            Assert.AreEqual(
                typeof(Func<int, int>),
                TestRunner.Run(@"return new Func((int p) => { return 1; }").GetType());
        }

        [TestMethod]
        public void Invocation()
        {
            Assert.AreEqual(
                10,
                TestRunner.Run(@"
var a = new Func(() => {
    return 10;
});
return a();
"));
        }
    }
}
