using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class EqTest
    {
        [TestMethod]
        public void SameValueSameType()
        {
            Assert.AreEqual(
                true,
                TestRunner.Run(@"return 1 == 1"));
            Assert.AreEqual(
                false,
                TestRunner.Run(@"return 1 == 5"));

            Assert.AreEqual(
                true,
                TestRunner.Run(@"return true == true"));
            Assert.AreEqual(
                false,
                TestRunner.Run(@"return true == false"));

            Assert.AreEqual(
                true,
                TestRunner.Run(@"return ""bb"" == ""bb"""));
            Assert.AreEqual(
                false,
                TestRunner.Run(@"return ""bb"" == ""aa"""));
            Assert.AreEqual(
                false,
                TestRunner.Run(@"return ""bb"" == null"));
        }

        [TestMethod]
        public void SameValueDifferentType()
        {
            Assert.AreEqual(
                true,
                TestRunner.Run(@"return 1 == 1.0f"));
            Assert.AreEqual(
                false,
                TestRunner.Run(@"return (new object()) == null"));
        }

        [TestMethod]
        public void NullVsNull()
        {
            Assert.AreEqual(
                true,
                TestRunner.Run(@"return null == null"));
            Assert.AreEqual(
                false,
                TestRunner.Run(@"return null != null"));
        }
    }
}
