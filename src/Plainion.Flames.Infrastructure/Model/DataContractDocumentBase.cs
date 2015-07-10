using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace Plainion.Flames.Infrastructure.Model
{
    [DataContract(Name = "Unknown", Namespace = "https://github.com/ronin4net/Plainion.Flames/Project/Unknown")]
    public abstract class DataContractDocumentBase<T> : IDocument
    {
        public void Serialize(Stream stream)
        {
            var serializer = new DataContractSerializer(GetType(), GetKnownTypes());
            serializer.WriteObject(stream, this);
        }

        protected virtual IEnumerable<Type> GetKnownTypes()
        {
            return Enumerable.Empty<Type>();
        }

        public void Deserialize(Stream stream)
        {
            var serializer = new DataContractSerializer(GetType(), GetKnownTypes());
            OnDeserialized((T)serializer.ReadObject(stream));
        }

        protected abstract void OnDeserialized(T document);
    }
}
