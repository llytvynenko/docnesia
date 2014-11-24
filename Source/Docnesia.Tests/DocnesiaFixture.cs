using System;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis.MSBuild;
using NUnit.Framework;

using Docnesia;

namespace Docnesia.Tests
{
    [TestFixture]
    public class DocnesiaFixture
    {
        private static Specification BuildSpec()
        {
            return Docnesia.Spec("deployment")
                .WithSln("../Docnesia.sln")
                .WithDoc("github.com/llytvynenko/docnesia/wiki")
                .From<Specification>()
                .From<Docnesia>();
                // TODO: .From(Asset.File("build.cfg"));
        }

        [Test]
        public void Should_configure()
        {
            var spec = BuildSpec();

            Assert.That(Docnesia.Spec("deployment"), Is.SameAs(spec));
            Assert.That(spec.Dependencies, Contains.Item(typeof(Docnesia)));
            Assert.That(spec.Dependencies, Contains.Item(typeof(Specification)));
        }

        [Test]
        public void Should_fail_when_code_changed()
        {
            var spec = BuildSpec();
            spec.CodeSnapshotReader = 
                name => 
                    name + ": " + name.GetHashCode()
                    .ToString(CultureInfo.InvariantCulture);

            Assert.Throws<ApplicationException>(spec.Verify);
        }

        [Test]
        public void Should_pass_when_code_is_unchanged()
        {
            var spec = BuildSpec();
            spec.CodeSnapshotReader =
                name => BuildSnapshot(spec);
            Assert.DoesNotThrow(spec.Verify);
        }

        [Test]
        public void Should_pass_actual_usage()
        {
            var spec = BuildSpec();
            spec.MarkValid();
            Assert.DoesNotThrow(spec.Verify);
        }
        
        [Test]
        public void Should_find_type_only_once()
        {
            var spec = BuildSpec();
            var workspace = MSBuildWorkspace.Create();
            var solution = workspace.OpenSolutionAsync(spec.Solution).Result;
            var projects = solution.Projects
                .Select(s => s.GetCompilationAsync().Result)
                .ToList();
            Assert.True(projects[0].GetTypeByMetadataName(typeof(Docnesia).FullName));
        }

        private string BuildSnapshot(Specification spec)
        {
            var result = spec.Name + ":";
            var workspace = MSBuildWorkspace.Create();
            var solution = workspace.OpenSolutionAsync(spec.Solution).Result;
            foreach (var proj in solution.Projects.OrderBy(p=>p.AssemblyName))
            {
                var compilation = proj.GetCompilationAsync().Result;
                foreach (var dependency in spec.Dependencies.OrderBy(d => d.AssemblyQualifiedName))
                {
                    var programClass = compilation.GetTypeByMetadataName(dependency.FullName);
                    if (programClass != null)
                        result += " " + dependency.AssemblyQualifiedName + ":" + string.Join(", ", programClass.GetMembers());
                }
            }
            return result;
        }
    }
}
