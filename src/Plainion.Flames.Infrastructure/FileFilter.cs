using Plainion;

namespace Plainion.Flames.Infrastructure
{
    public class FileFilter
    {
        public FileFilter( string extension, string description )
        {
            Contract.RequiresNotNullNotEmpty( extension, "extension" );
            Contract.RequiresNotNullNotEmpty( description, "description" );

            Extension = extension;
            Description = description;
        }

        public string Extension { get; private set; }
        public string Description { get; private set; }
    }
}
