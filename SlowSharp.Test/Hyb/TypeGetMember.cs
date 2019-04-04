using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class HybTypeGetMemberTest
    {
        public class Foo
        {
            public void Public() { }
            protected void Protected() { }
            private void Private() { }
        }

        [TestMethod]
        public void GetMethod()
        {
            var typeInt = new HybType(typeof(int));

            // derivered method
            Assert.AreEqual(
                true, 
                typeInt.GetMethods()
                .Where(x => x.id == nameof(int.GetType)).Count() > 0);
            // class method
            Assert.AreEqual(
                true,
                typeInt.GetMethods()
                .Where(x => x.id == nameof(int.ToString)).Count() > 0);
        }
        [TestMethod]
        public void GetStaticMethod()
        {
            var typeInt = new HybType(typeof(int));

            // static method
            Assert.AreEqual(
                true,
                typeInt.GetStaticMethods()
                .Where(x => x.id == nameof(int.TryParse)).Count() > 0);
            
            // instance method should not be visible
            Assert.AreEqual(
                false,
                typeInt.GetStaticMethods()
                .Where(x => x.id == nameof(int.ToString)).Count() > 0);
        }

        [TestMethod]
        public void GetMethodAndAccessor()
        {
            var typeInt = new HybType(typeof(Foo));

            Assert.AreEqual(
                true,
                typeInt.GetMethods()
                .Where(x => x.id == nameof(Foo.Public)).Count() > 0);
            Assert.AreEqual(
                true,
                typeInt.GetMethods()
                .Where(x => x.id == "Protected").Count() > 0);
            Assert.AreEqual(
                false,
                typeInt.GetMethods()
                .Where(x => x.id == "Private").Count() > 0);
        }
    }
}
