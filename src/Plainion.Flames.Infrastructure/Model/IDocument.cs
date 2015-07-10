using System.IO;

namespace Plainion.Flames.Infrastructure.Model
{
    public interface IDocument
    {
        void Serialize(Stream stream);

        void Deserialize(Stream stream);
    }
}
