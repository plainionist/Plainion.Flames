using System.Collections.ObjectModel;
using System.ComponentModel.Composition;

namespace Plainion.Flames.Viewer.Model
{
    /// <summary>
    /// Root entity for all loaded projects
    /// </summary>
    [Export]
    class Solution
    {
        public Solution()
        {
            Projects = new ObservableCollection<Project>();
        }

        public ObservableCollection<Project> Projects { get; private set; }
    }
}
