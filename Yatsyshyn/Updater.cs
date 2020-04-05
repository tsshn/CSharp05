using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yatsyshyn.Tools;

namespace Yatsyshyn
{
    internal static class Updater
    {
        private static readonly Thread UpdateProcessesList;
        private static readonly Thread UpdateProcessesMetaData;

        private const int Interval1 = 5000;
        private const int Interval2 = 2000;

        internal static Dictionary<int, Provider> ProcessesList { get; }

        static Updater()
        {
            ProcessesList = new Dictionary<int, Provider>();
            UpdateProcessesMetaData = new Thread(UpdateProcessesMetaDataImplementation);
            UpdateProcessesList = new Thread(UpdateProcessedListImplementation);
            UpdateProcessesList.Start();
            UpdateProcessesMetaData.Start();
        }

        internal static void Close()
        {
            UpdateProcessesList.Join(Interval1);
            UpdateProcessesMetaData.Join(Interval2);
        }

        private static async void UpdateProcessedListImplementation()
        {
            while (true)
            {
                await Task.Run(() =>
                {
                    lock (ProcessesList)
                    {
                        var processesList = Process.GetProcesses().ToList();
                        var keysList = ProcessesList.Keys.ToList()
                            .Where(id => processesList.All(proc => proc.Id != id));
                        foreach (var key in keysList) ProcessesList.Remove(key);
                        foreach (var process in processesList)
                            if (!ProcessesList.ContainsKey(process.Id))
                                try
                                {
                                    ProcessesList[process.Id] = new Provider(process);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.Message);
                                }
                    }
                });
                Thread.Sleep(Interval1);
            }
        }

        private static async void UpdateProcessesMetaDataImplementation()
        {
            while (true)
            {
                await Task.Run(() =>
                {
                    lock (ProcessesList)
                    {
                        foreach (var id in ProcessesList.Keys.ToList())
                        {
                            Process process;
                            try
                            {
                                process = Process.GetProcessById(id);
                            }
                            catch (ArgumentException)
                            {
                                ProcessesList.Remove(id);
                                continue;
                            }

                            ProcessesList[id].CpuUsage =
                                Convert.ToInt32(ProcessesList[id].CpuCounter.NextValue() / Environment.ProcessorCount);
                            ProcessesList[id].RamUsage =
                                Math.Round(ProcessesList[id].RamCounter.NextValue() / (1024 * 1024), 2);
                            ProcessesList[id].RamUsagePercent =
                                Math.Round(ProcessesList[id].RamUsage / RamCalculator.Ram * 100, 2);
                            ProcessesList[id].ThreadsNumber = process.Threads.Count;
                        }
                    }
                });
                Thread.Sleep(Interval2);
            }
        }
    }
}