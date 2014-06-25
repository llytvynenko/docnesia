using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Microsoft.CodeAnalysis.MSBuild;


namespace Docnesia
{
    public class Docnesia
    {
        private static Dictionary<string, Specification> specs = new Dictionary<string, Specification>();

        public static Specification Spec(string name)
        {
            var spec = AddOrGet(name);
            return spec;
        }

        private static Specification AddOrGet(string name)
        {
            if (specs.ContainsKey(name))
                return specs[name];

            var spec = new Specification(name);
            specs.Add(name, spec);
            return spec;
        }
    }

    public class Specification
    {
        public readonly string Name;
        public readonly List<Type> Dependencies = new List<Type>();
        public string Doc = "";
        public string Solution = "";
        private static string codeSnapshotStorage = "docnesia.txt";

        public Specification(string name)
        {
            Name = name;
        }

        public Func<string, string> CodeSnapshotReader =
            name =>
                File.Exists(codeSnapshotStorage)
                    ? File.ReadAllLines(codeSnapshotStorage)
                        .FirstOrDefault(l => l.StartsWith(name + ":")) ?? ""
                    : "";

        public Specification WithDoc(string situatedAt)
        {
            Doc = situatedAt;
            return this;
        }

        public Specification From<T>()
        {
            Dependencies.Add(typeof(T));
            return this;
        }

        public Specification WithSln(string sln)
        {
            Solution = sln;
            return this;
        }

        private string BuildCodeSnapshot()
        {
            var result = Name + ":";
            var workspace = MSBuildWorkspace.Create();
            var solution = workspace.OpenSolutionAsync(Solution).Result;
            foreach (var proj in solution.Projects.OrderBy(p => p.AssemblyName))
            {
                var compilation = proj.GetCompilationAsync().Result;
                foreach (var dependency in Dependencies.OrderBy(d => d.AssemblyQualifiedName))
                {
                    var programClass = compilation.GetTypeByMetadataName(dependency.FullName);
                    if (programClass != null)
                        result += " " + dependency.AssemblyQualifiedName + ":" + string.Join(", ", programClass.GetMembers());
                }
            }
            return result;
        }

        public void MarkValid()
        {
            var actualSnapshot = BuildCodeSnapshot();
            if (!File.Exists(codeSnapshotStorage))
            {
                File.WriteAllText(codeSnapshotStorage, actualSnapshot);
                return;
            }

            var lines = File.ReadAllLines(codeSnapshotStorage).ToList();
            if (lines.Any(l => l.StartsWith(Name + ":")))
            {
                var newSnapshot =
                        lines.Select(l =>
                            l.StartsWith(Name + ":")
                                ? actualSnapshot
                                : l);
                File.WriteAllLines(codeSnapshotStorage, newSnapshot);
            }
            else
            {
                lines.Add(actualSnapshot);
                File.WriteAllLines(codeSnapshotStorage, lines);
            }
        }

        public void Verify()
        {
            var savedSnapshot = CodeSnapshotReader(Name);
            var actualSnapshot = BuildCodeSnapshot();
            if (actualSnapshot != savedSnapshot)
                throw new ApplicationException(string.Format("Check documentation on {0} {1}", Name, Doc));
        }
    }
}
