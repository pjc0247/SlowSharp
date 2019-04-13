using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class LinqTest
    {
        [TestMethod]
        public void Count()
        {
            Assert.AreEqual(
                4,
                TestRunner.Run(@"
var a = new int[] {1,2,3,4}
return a.Count();
"));
        }
        [TestMethod]
        public void CountWithPred()
        {
            Assert.AreEqual(
                2,
                TestRunner.Run(@"
var a = new int[] {1,2,3,4}
return a.Count((int x) => { return x > 2; });
"));
        }

        [TestMethod]
        public void Any()
        {
            Assert.AreEqual(
                true,
                TestRunner.Run(@"
var a = new int[] {1,2,3,4}
return a.Any((int x) => { return x > 2; });
"));
            Assert.AreEqual(
                false,
                TestRunner.Run(@"
var a = new int[] {1,2,3,4}
return a.Any((int x) => { return x > 22; });
"));
        }

        [TestMethod]
        public void Reverse()
        {
            CollectionAssert.AreEqual(
                new int[] { 4, 3, 2, 1 },
                (int[])TestRunner.Run(@"
var a = new int[] {1,2,3,4}
return a.Reverse().ToArray();
"));
        }

        [TestMethod]
        public void ToArray()
        {
            CollectionAssert.AreEqual(
                new int[] { 4, 3, 2, 1 },
                (int[])TestRunner.Run(@"
var a = new List<int>() {4,3,2,1};
return a.ToArray();
"));
        }
    }
}
