using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace Docnesia
{
    public class CodeSnapshot : Dictionary<string, string[]>
    {
        public static CodeSnapshot Build(string solutionName, List<Type> dependencies)
        {
            var snapshot = new CodeSnapshot();
            var workspace = MSBuildWorkspace.Create();
            var solution = workspace.OpenSolutionAsync(solutionName).Result;
            foreach (var proj in solution.Projects.OrderBy(p => p.AssemblyName))
            {
                var compilation = proj.GetCompilationAsync().Result;
                foreach (var dependency in dependencies.OrderBy(d => d.FullName))
                {
                    var programClass = compilation.GetTypeByMetadataName(dependency.FullName);
                    if (programClass == null) 
                        continue;

                    var fullName = dependency.FullName;
                    var symbols = programClass.GetMembers();
                    var members = symbols
                                    .Select(s => s.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat))
                                    .ToArray();
                    Array.Sort(members);

                    snapshot.Add(fullName, members);
                }
            }
            return snapshot;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var firstLine = true;
            foreach (var codeSignature in this)
            {
                if (!firstLine)
                    sb.AppendLine();

                firstLine = false;
                sb.AppendFormat("{0}:", codeSignature.Key);
                sb.AppendLine();
                var innerFirstLine = true;
                foreach (var member in codeSignature.Value)
                {
                    if (!innerFirstLine)
                        sb.AppendLine();

                    innerFirstLine = false;
                    sb.AppendFormat("\t{0}", member);
                }
            }
            return sb.ToString();
        }
    }
}
