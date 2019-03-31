using System;
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
var a = new List<int>() {4,3,2,1}
return a.ToArray();
"));
        }
    }
}
