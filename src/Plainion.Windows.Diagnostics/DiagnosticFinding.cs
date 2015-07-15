using System.Collections.Generic;
using System.IO;

namespace Plainion.Windows.Diagnostics
{
    class DiagnosticFinding
    {
        public DiagnosticFinding(string description, string resolution)
        {
            Contract.RequiresNotNullNotEmpty(description, "description");
            Contract.RequiresNotNullNotEmpty(resolution, "resolution");

            Description = description;
            Resolution = resolution;

            Locations = new List<string>();
        }

        public string Description { get; private set; }

        public string Resolution { get; private set; }

        public IList<string> Locations { get; private set; }

        public void AddLocation(string fmt, params object[] args)
        {
            Locations.Add(string.Format(fmt, args));
        }

        public void WriteTo(TextWriter writer)
        {
            writer.WriteLine("Description: " + Description);
            writer.WriteLine("Locations:");

            if (Locations.Count == 0)
            {
                writer.WriteLine("  none");
            }
            else
            {
                foreach (var location in Locations)
                {
                    writer.WriteLine("  " + location);
                }
            }
            writer.WriteLine("Resolution: " + Resolution);
        }
    }
}
