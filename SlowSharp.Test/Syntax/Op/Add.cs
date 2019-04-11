using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    public class FooForAddTest
    {
        public int value;
        public static FooForAddTest operator +(FooForAddTest a, FooForAddTest b)
        {
            return new FooForAddTest() { value = a.value + b.value };
        }
    }

    [TestClass]
    public class AddTest
    {
        [TestMethod]
        public void BasicAdd()
        {
            Assert.AreEqual(
                2,
                TestRunner.Run(@"return 1 + 1"));
            Assert.AreEqual(
                0,
                TestRunner.Run(@"return 1 + (-1)"));
        }

        [TestMethod]
        public void AddOperator()
        {
            Assert.AreEqual(
                35,
                ((FooForAddTest)TestRunner.Run(@"
var foo10 = new FooForAddTest() { value = 10 };
var foo25 = new FooForAddTest() { value = 25 };
return foo10 + foo25;")).value
);
        }
    }
}
