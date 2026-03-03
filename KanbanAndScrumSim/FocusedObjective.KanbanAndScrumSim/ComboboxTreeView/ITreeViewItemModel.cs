using System.Collections.Generic;
using System.ComponentModel;

namespace FocusedObjective.KanbanSim
{
    public interface ITreeViewItemModel : INotifyPropertyChanged
    {
        object SelectedValuePath { get; }

        string DisplayValuePath { get; }

        bool IsExpanded { get; set; }

        bool IsSelected { get; set; }

        IEnumerable<ITreeViewItemModel> GetHierarchy();

        IEnumerable<ITreeViewItemModel> GetChildren();
    }
}