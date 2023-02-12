using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SaladimQBot.Shared.Tests;

[TestClass]
public class ExpirableTests
{
    public readonly TimeSpan Time = TimeSpan.FromSeconds(0.25);

    [TestMethod]
    public void IndependentExpirableTest()
    {
        IndependentExpirable<int> expInt = new(45, () => 28, Time);
        Assert.AreEqual(expInt.Value, 45);
        Thread.Sleep(300);
        Assert.AreEqual(expInt.Value, 28);

        int callTimes = 0;
        IndependentExpirable<long> expLong = new(() => callTimes++, Time);
        Assert.AreEqual(expLong.Value, 0);
        Assert.AreEqual(callTimes, 1);
        Thread.Sleep(300);
        Assert.AreEqual(expLong.Value, 1);
        Assert.AreEqual(callTimes, 2);
    }

    [TestMethod]
    public void DependencyExpirableTest()
    {
        IndependentExpirable<ClassInteger> expInt = new(25, () => 114, Time);

        SourceExpirable<ClassInteger> sourceExpirable = new(expInt);
        DependencyExpirable<ClassInteger> dependencyExpirable1 = DependencyExpirable<ClassInteger>.Create(sourceExpirable, i => i + 1);
        DependencyExpirable<ClassInteger> dependencyExpirable2 = DependencyExpirable<ClassInteger>.Create(dependencyExpirable1, i => i + 2);
        {
            int sourceValue = sourceExpirable.Value;
            Assert.AreEqual(25, sourceValue);
            int dependencyValue = dependencyExpirable1.Value;
            Assert.AreEqual(26, dependencyValue);
            int dependency2Value = dependencyExpirable2.Value;
            Assert.AreEqual(28, dependency2Value);
        }

        Thread.Sleep(300);

        {
            Assert.AreEqual(true, sourceExpirable.IsExpired);
            int sourceValue = sourceExpirable.Value;
            Assert.AreEqual(114, sourceValue);
            Assert.AreEqual(false, sourceExpirable.IsExpired);


            Assert.AreEqual(false, dependencyExpirable1.IsExpired);
            int dependencyValue = dependencyExpirable1.Value;
            Assert.AreEqual(115, dependencyValue);

            Assert.AreEqual(false, dependencyExpirable2.IsExpired);
            int dependency2Value = dependencyExpirable2.Value;
            Assert.AreEqual(117, dependency2Value);
        }

        Thread.Sleep(300);

        {
            Assert.AreEqual(true, dependencyExpirable1.IsExpired);
            int dependencyValue = dependencyExpirable1.Value;
            Assert.AreEqual(115, dependencyValue);

            Assert.AreEqual(false, dependencyExpirable2.IsExpired);
            int dependency2Value = dependencyExpirable2.Value;
            Assert.AreEqual(117, dependency2Value);
        }
    }

    public sealed class ClassInteger
    {
        public int Value { get; set; }

        public ClassInteger(int v)
        {
            Value = v;
        }

        public static implicit operator int(ClassInteger ci) => ci.Value;
        public static implicit operator ClassInteger(int i) => new(i);
    }
}