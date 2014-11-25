using System.Collections.Generic;


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
}
