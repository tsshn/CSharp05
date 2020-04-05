using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Management;
using System.Runtime.CompilerServices;
using Yatsyshyn.Tools;

namespace Yatsyshyn
{
    internal class Provider : INotifyPropertyChanged
    {
        private double _cpuUsage;
        private double _ramUsage;
        private double _ramUsagePercent;

        internal PerformanceCounter RamCounter { get; }
        internal PerformanceCounter CpuCounter { get; }

        public string Name { get; }
        public int Id { get; }
        public bool IsActive { get; }

        public double CpuUsage
        {
            get => _cpuUsage;
            set
            {
                _cpuUsage = value;
                OnPropertyChanged();
            }
        }

        public double RamUsage
        {
            get => _ramUsage;
            set
            {
                _ramUsage = value;
                OnPropertyChanged();
            }
        }

        public double RamUsagePercent
        {
            get => _ramUsagePercent;
            set
            {
                _ramUsagePercent = value;
                OnPropertyChanged();
            }
        }

        public int ThreadsNumber { get; set; }
        public string Username { get; }
        public string FilePath { get; }
        public string StartTime { get; }

        internal Provider(Process systemProcess)
        {
            RamCounter = new PerformanceCounter("Process", "Private Bytes", systemProcess.ProcessName);
            CpuCounter = new PerformanceCounter("Process", "% Processor Time", systemProcess.ProcessName);
            Name = systemProcess.ProcessName;
            Id = systemProcess.Id;
            IsActive = systemProcess.Responding;
            CpuUsage = CpuCounter.NextValue() / Environment.ProcessorCount / 100f;
            RamUsage = Math.Round(RamCounter.NextValue() / (1024 * 1024), 2);
            RamUsagePercent = Math.Round(RamUsage / RamCalculator.Ram * 100, 2);
            ThreadsNumber = systemProcess.Threads.Count;
            Username = GetProcessOwner(systemProcess.Id);
            try
            {
                if (systemProcess.MainModule != null) FilePath = systemProcess.MainModule.FileName;
            }
            catch (Win32Exception e)
            {
                FilePath = e.Message;
            }

            try
            {
                StartTime = systemProcess.StartTime.ToString();
            }
            catch (Win32Exception e)
            {
                StartTime = e.Message;
            }
        }

        private static string GetProcessOwner(int processId)
        {
            var query = "Select * From Win32_Process Where ProcessID = " + processId;
            var finder = new ManagementObjectSearcher(query);
            var processList = finder.Get();
            foreach (var o in processList)
            {
                var process = (ManagementObject) o;
                object[] argumentsList = {string.Empty, string.Empty};
                var returnVal = Convert.ToInt32(process.InvokeMethod("GetOwner", argumentsList));
                if (returnVal == 0)
                    return (string) argumentsList[0];
            }
            return "No owner";
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}