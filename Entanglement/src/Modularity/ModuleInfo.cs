using System;

namespace Entanglement.Modularity {
    [System.AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    public sealed class EntanglementModuleInfo : Attribute {
        public readonly Type moduleType;
        public readonly string name, author, version, abbreviation;

        public EntanglementModuleInfo(Type moduleType, string name, string version = null, string author = null, string abbreviation = null) {
            this.moduleType = moduleType;
            this.name = name;
            this.author = author;
            this.version = version;

            if (version == null)
                this.version = "0.0.0";

            if (author == null)
                this.author = "Unknown";

            this.abbreviation = abbreviation;
        }
    }
}
