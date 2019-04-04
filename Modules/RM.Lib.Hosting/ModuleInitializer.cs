using System;
using System.Collections.Generic;
using System.Text;

namespace RM.Lib.Hosting
{
    public sealed class ModuleInitializer
    {
        public ModuleInitializer(Delegate initializer, Type sectionType, bool isArray)
        {
            Initializer = initializer;
            SectionType = sectionType;
            IsArray = isArray;
        }

        public Delegate Initializer { get; }

        public Type SectionType { get; }

        public bool IsArray { get; }

        public void Deconstruct(out Delegate initDelegate, out Type type, out bool isArray)
        {
            initDelegate = Initializer;
            type = SectionType;
            isArray = IsArray;
        }
    }
}
