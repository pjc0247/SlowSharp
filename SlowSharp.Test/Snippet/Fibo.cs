using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class FiboTest
    {
        [TestMethod]
        public void FibonacciIterative()
        {
            Assert.AreEqual(
                5,
                TestRunner.RunRaw(
                    @"
using System;

class Program
{
    public static int Fibonacci(int n)
    {
        int a = 0;
        int b = 1;
        // In N steps compute Fibonacci sequence iteratively.
        for (int i = 0; i < n; i++)
        {
            int temp = a;
            a = b;
            b = temp + b;
        }
        return a;
    }

    public static int Main()
    {
        return Fibonacci(5);
    }
}
"));
        }

        [TestMethod]
        public void FibonacciRecursive()
        {
            Assert.AreEqual(
                5,
                TestRunner.RunRaw(
                    @"
using System;

class Program
{
    public static int Fibonacci(int n) {
        return (n < 2)? n : Fibonacci(n - 1) + Fibonacci(n - 2);
    }

    public static int Main()
    {
        return Fibonacci(5);
    }
}
"));
        }
    }
}
