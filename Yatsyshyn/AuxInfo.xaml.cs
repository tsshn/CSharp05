using System.Diagnostics;

namespace Yatsyshyn
{
    internal partial class AuxInfo
    {
        internal AuxInfo(Process process)
        {
            InitializeComponent();
            Title = $"{process.ProcessName} Info";
            DataContext = new ViewModels.AuxInfo(process.Modules, process.Threads);
        }
    }
}