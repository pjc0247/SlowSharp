using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    public class FooForSubTest
    {
        public int value;
        public static FooForSubTest operator -(FooForSubTest a, FooForSubTest b)
        {
            return new FooForSubTest() { value = a.value - b.value };
        }
    }

    [TestClass]
    public class SubTest
    {
        [TestMethod]
        public void BasicAdd()
        {
            Assert.AreEqual(
                0,
                TestRunner.Run(@"return 1 - 1"));
            Assert.AreEqual(
                2,
                TestRunner.Run(@"return 1 - (-1)"));
        }

        [TestMethod]
        public void SubOperator()
        {
            Assert.AreEqual(
                -15,
                ((FooForSubTest)TestRunner.Run(@"
var foo10 = new FooForSubTest() { value = 10 };
var foo25 = new FooForSubTest() { value = 25 };
return foo10 - foo25;")).value
);
        }
    }
}
