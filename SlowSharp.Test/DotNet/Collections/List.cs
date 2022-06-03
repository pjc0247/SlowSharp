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
        public void InstantiateWithInitializer()
        {
            var list = (List<int>)TestRunner.Run(@"
return new List<int>() { 1,2,3 };
");
            Assert.IsInstanceOfType(
                list,
                typeof(List<int>));
            CollectionAssert.AreEqual(
                new List<int>() { 1, 2, 3 },
                list);
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
        public void Remove()
        {
            var list = (List<int>)TestRunner.Run(@"
var a = new List<int>();
a.Add(1); a.Add(2); a.Add(3);
a.Remove(2);
return a;
");

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(1, list[0]);
            Assert.AreEqual(3, list[1]);
        }

        [TestMethod]
        public void RemoveAt()
        {
            var list = (List<int>)TestRunner.Run(@"
var a = new List<int>();
a.Add(1); a.Add(2); a.Add(3);
a.RemoveAt(1);
return a;
");

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(1, list[0]);
            Assert.AreEqual(3, list[1]);
        }

        [TestMethod]
        public void RemoveAll()
        {
            var list = (List<int>)TestRunner.Run(@"
var a = new List<int>();
a.Add(1); a.Add(2); a.Add(3); a.Add(4);
a.RemoveAll((int x) => { return x % 2 == 0; });
return a;
");

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(1, list[0]);
            Assert.AreEqual(3, list[1]);
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
