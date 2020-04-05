using System.ComponentModel;
using System.Windows;

namespace Yatsyshyn.Tools
{
    internal interface ILoaderOwner : INotifyPropertyChanged
    {
        Visibility LoaderVisibility { set; }
        bool IsControlEnabled { set; }
    }
}