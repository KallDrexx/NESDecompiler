using System.Collections.ObjectModel;
using System.ComponentModel;

namespace NESDecompiler.GUI.ViewModels
{
    /// <summary>
    /// Types of project items
    /// </summary>
    public enum ProjectItemType
    {
        Project,
        ROM,
        ROMInfo,
        Disassembly,
        CCode,
        Header,
        Variables,
        Functions
    }

    /// <summary>
    /// View model for a project item in the tree view
    /// </summary>
    public class ProjectItemViewModel : INotifyPropertyChanged
    {
        private string name = "";
        private ProjectItemType type;
        private string path = "";

        /// <summary>
        /// The name of this item
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public class RecentProjectInfo
        {
            public string FilePath { get; set; }
            public string Name { get; set; }
            public DateTime LastOpened { get; set; }
        }

        /// <summary>
        /// The type of this item
        /// </summary>
        public ProjectItemType Type
        {
            get => type;
            set
            {
                type = value;
                OnPropertyChanged(nameof(Type));
                OnPropertyChanged(nameof(Icon));
            }
        }

        /// <summary>
        /// The file path of this item
        /// </summary>
        public string Path
        {
            get => path;
            set
            {
                path = value;
                OnPropertyChanged(nameof(Path));
            }
        }

        /// <summary>
        /// The icon for this item type
        /// </summary>
        public string Icon
        {
            get
            {
                return Type switch
                {
                    ProjectItemType.ROM => "/Resources/Icons/rom.png",
                    ProjectItemType.ROMInfo => "/Resources/Icons/info.png",
                    ProjectItemType.Disassembly => "/Resources/Icons/asm.png",
                    ProjectItemType.CCode => "/Resources/Icons/c.png",
                    ProjectItemType.Header => "/Resources/Icons/h.png",
                    ProjectItemType.Variables => "/Resources/Icons/var.png",
                    ProjectItemType.Functions => "/Resources/Icons/func.png",
                    _ => "/Resources/Icons/file.png"
                };
            }
        }

        /// <summary>
        /// Child items for this item
        /// </summary>
        public ObservableCollection<ProjectItemViewModel> Children { get; } = new ObservableCollection<ProjectItemViewModel>();

        private bool isExpanded = false;

        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));

                if (Tag is ROMProject romProject)
                {
                    romProject.IsExpanded = value;
                }
            }
        }

        public object Tag { get; set; }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}