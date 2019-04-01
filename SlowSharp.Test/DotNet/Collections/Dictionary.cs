using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class DictionaryTest
    {
        [TestMethod]
        public void Instantiate()
        {
            Assert.IsInstanceOfType(
                TestRunner.Run(@"
return new Dictionary<int, int>();
"),
            typeof(Dictionary<int, int>));
        }
        [TestMethod]
        public void InstantiateWithInitializer()
        {
            var dict = (Dictionary<int, int>)TestRunner.Run(@"
return new Dictionary<int, int>() { [1] = 1, [2] = 2, [3] = 3 };
");
            Assert.IsInstanceOfType(
                dict,
                typeof(Dictionary<int, int>));
            CollectionAssert.AreEqual(
                new Dictionary<int, int>() { [1] = 1, [2] = 2, [3] = 3 },
                dict);
        }

        [TestMethod]
        public void Add()
        {
            var dict = (Dictionary<int, int>)TestRunner.Run(@"
var a = new Dictionary<int, int>();
a.Add(1, 1); a.Add(2, 2); a.Add(3, 3);
return a;
");

            Assert.AreEqual(3, dict.Count);
            Assert.AreEqual(1, dict[1]);
            Assert.AreEqual(2, dict[2]);
            Assert.AreEqual(3, dict[3]);
        }

        [TestMethod]
        public void Count()
        {
            var count = TestRunner.Run(@"
var a = new Dictionary<int, int>();
a.Add(1,1); a.Add(2,2); a.Add(3,3);
return a.Count;
");

            Assert.AreEqual(3, count);
        }

        [TestMethod]
        public void GetByIndex()
        {
            var one = TestRunner.Run(@"
var a = new Dictionary<int, int>();
a.Add(1,1); a.Add(2,2); a.Add(3,3);
return a[1];
");

            Assert.AreEqual(1, one);
        }
    }
}
