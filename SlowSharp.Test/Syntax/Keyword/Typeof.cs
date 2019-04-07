using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class TypeofTest
    {
        [TestMethod]
        public void Premitive()
        {
            Assert.AreEqual(
                typeof(int),
                TestRunner.Run(@"return typeof(int);"));
            Assert.AreEqual(
                typeof(string),
                TestRunner.Run(@"return typeof(string);"));
            Assert.AreEqual(
                typeof(object[]),
                TestRunner.Run(@"return typeof(object[]);"));
        }

        [TestMethod]
        public void Generic()
        {
            Assert.AreEqual(
                typeof(List<int>),
                TestRunner.Run(@"return typeof(List<int>);"));
            Assert.AreEqual(
                typeof(Dictionary<string, string>),
                TestRunner.Run(@"return typeof(Dictionary<string, string>);"));
        }
    }
}
