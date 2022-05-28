using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class CastingTest
    {
        [TestMethod]
        public void Primitive()
        {
            Assert.AreEqual(
                typeof(float).Name,
                TestRunner.Run(@"return (float)1;").GetType().Name);

            Assert.AreEqual(
                typeof(int).Name,
                TestRunner.Run(@"return (int)1.0f;").GetType().Name);
        }

        [TestMethod]
        public void IllegalCasting()
        {
            // FIXME
            // this should throw an exception.
            /*
            Assert.AreEqual(
                typeof(float).Name,
                TestRunner.Run(@"return (string)1;").GetType().Name);
            */
        }
    }
}
