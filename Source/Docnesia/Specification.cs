using System;
using System.Collections.Generic;

namespace Docnesia
{
    public class Specification
    {
        public readonly string Name;
        public readonly List<Type> Dependencies = new List<Type>();
        public string Solution = "";

        public Specification(string name)
        {
            Name = name;
        }

        public Specification From<T>()
        {
            if (!Dependencies.Contains(typeof(T)))
                Dependencies.Add(typeof(T));
            return this;
        }

        public Specification WithSln(string sln)
        {
            Solution = sln;
            return this;
        }

        public CodeSnapshot Snapshot()
        {
            return CodeSnapshot.Build(Solution, Dependencies);
        }
    }
}