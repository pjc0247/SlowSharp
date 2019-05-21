using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class TypeResolutionTest
    {
        [TestMethod]
        public void BasicArray()
        {
            TypeResolver resolver = new TypeResolver(
                new RunContext(RunConfig.Default),
                AppDomain.CurrentDomain.GetAssemblies());

            Assert.AreEqual(
                typeof(int[]),
                resolver.GetType("int[]").CompiledType);
        }
    }
}
