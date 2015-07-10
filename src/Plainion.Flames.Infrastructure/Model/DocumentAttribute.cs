using System;
using System.ComponentModel.Composition;

namespace Plainion.Flames.Infrastructure.Model
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DocumentAttribute : ExportAttribute
    {
        public DocumentAttribute(string name)
            : base(typeof(IDocument))
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}
