using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class InvokeTest
    {
        [TestMethod]
        public void InvokeWithImplicitCasting()
        {
            Assert.AreEqual(TestRunner.RunRaw(@"
class Foo {
    static float MakeDouble(float a) {
        return a * 2;
    }

    public static void Main() {
        return MakeDouble((int)1);
    }
}
"), 2);
        }
    }
}
