using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class ListTest
    {
        [TestMethod]
        public void Instantiate()
        {
            Assert.IsInstanceOfType(
                TestRunner.Run(@"
return new List<int>();
"),
            typeof(List<int>));
        }

        [TestMethod]
        public void Add()
        {
            var list = (List<int>)TestRunner.Run(@"
var a = new List<int>();
a.Add(1); a.Add(2); a.Add(3);
return a;
");

            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(1, list[0]);
            Assert.AreEqual(2, list[1]);
            Assert.AreEqual(3, list[2]);
        }

        [TestMethod]
        public void Count()
        {
            var count = TestRunner.Run(@"
var a = new List<int>();
a.Add(1); a.Add(2); a.Add(3);
return a.Count;
");

            Assert.AreEqual(3, count);
        }

        [TestMethod]
        public void GetByIndex()
        {
            var first = TestRunner.Run(@"
var a = new List<int>();
a.Add(1); a.Add(2); a.Add(3);
return a[0];
");

            Assert.AreEqual(1, first);
        }
    }
}
