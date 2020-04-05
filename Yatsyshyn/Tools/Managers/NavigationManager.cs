using System.Diagnostics;

namespace Yatsyshyn.Tools.Managers
{
    internal static class NavigationManager
    {
        private static AuxInfo _auxInfo;

        internal static void Navigate(Process process = null)
        {
            if (process != null)
            {
                _auxInfo = new AuxInfo(process);
                _auxInfo.Show();
            }
        }

        internal static void Close()
        {
            _auxInfo?.Close();
        }
    }
}