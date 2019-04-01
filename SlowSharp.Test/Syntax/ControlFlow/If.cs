using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class IfTest
    {
        [TestMethod]
        public void BasicIfStatement()
        {
            Assert.AreEqual(TestRunner.Run("if (5 > 1) return true;"), true);
            Assert.AreEqual(TestRunner.Run("if (1 >= 1) return true;"), true);
            Assert.AreEqual(TestRunner.Run("if (true) return true;"), true);
            Assert.AreEqual(TestRunner.Run("if (1) return true;"), true);

            Assert.AreEqual(TestRunner.Run("if (false) return true;"), null);
            Assert.AreEqual(TestRunner.Run("if (0) return true;"), null);
            Assert.AreEqual(TestRunner.Run("if (1 > 5) return true;"), null);
            Assert.AreEqual(TestRunner.Run("if (1 >= 2) return true;"), null);
        }

        [TestMethod]
        public void IfElse()
        {
            Assert.AreEqual(TestRunner.Run("if (1 > 5) return true; else return false;"), false);
            Assert.AreEqual(TestRunner.Run("if (5 > 1) return true; else return false;"), true);
        }
    }
}
