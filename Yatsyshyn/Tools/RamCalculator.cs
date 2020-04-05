using System;
using System.Management;
using ObjectQuery = System.Management.ObjectQuery;

namespace Yatsyshyn.Tools
{
    internal static class RamCalculator
    {
        public static readonly double Ram = Initialize();

        private static double Initialize()
        {
            var objectQuery = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
            var finder = new ManagementObjectSearcher(objectQuery);
            var results = finder.Get();
            double total = 0;
            foreach (var o in results)
            {
                var result = (ManagementObject) o;
                var temp = Convert.ToDouble(result["TotalVisibleMemorySize"]);
                total += Math.Round(temp / 1024, 2);
            }

            return total;
        }
    }
}