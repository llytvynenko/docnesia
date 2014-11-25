using System;
using NUnit.Framework;

namespace Docnesia.Tests
{
    [TestFixture]
    public class DocnesiaFixture
    {
        private Specification spec;

        [SetUp]
        public void SetUp()
        {
            spec = BuildSpec();
        }

        private static Specification BuildSpec()
        {
            return Docnesia.Spec("deployment")
                .WithSln("../Docnesia.sln")
                .From<TestClass1>()
                .From<TestClass2>();
        }

        [Test]
        public void Should_configure()
        {
            Assert.That(Docnesia.Spec("deployment"), Is.SameAs(spec));
            Assert.That(spec.Dependencies, Contains.Item(typeof(TestClass1)));
            Assert.That(spec.Dependencies, Contains.Item(typeof(TestClass2)));
        }

        [Test]
        public void Should_fail_when_code_changed()
        {
            var emptySnapshot = new CodeSnapshot();
            Assert.That(spec.Snapshot(), Is.Not.EqualTo(emptySnapshot));
        }

        [Test]
        public void Should_pass_when_code_is_unchanged()
        {
            var snapshot = new CodeSnapshot
            {
                {"Docnesia.Tests.TestClass1", new []{"int TestClass1.Bar(int bar)", "TestClass1.TestClass1()", "void TestClass1.Foo(string foo)"}},
                {"Docnesia.Tests.TestClass2", new []{"int TestClass2.Buz()", "int[] TestClass2.Qux(int[] qux)", "string TestClass2.Buz(string[] buz)", "TestClass2.TestClass2()"}}
            };
            CollectionAssert.AreEquivalent(spec.Snapshot().Keys, snapshot.Keys);
            CollectionAssert.AreEquivalent(spec.Snapshot().Values, snapshot.Values);
        }
    }

    public class TestClass1
    {
        public void Foo(string foo) { }
        public int Bar(int bar) { throw new NotImplementedException(); }
    }

    public class TestClass2
    {
        public int Buz() { throw new NotImplementedException(); }
        public string Buz(string[] buz) { throw new NotImplementedException(); }
        public int[] Qux(int[] qux) { throw new NotImplementedException(); }
    }
}
