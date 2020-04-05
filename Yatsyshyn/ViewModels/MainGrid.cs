using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Yatsyshyn.Tools;
using Yatsyshyn.Tools.Managers;

namespace Yatsyshyn.ViewModels
{
    internal class MainGrid : BaseViewModel, ILoaderOwner
    {
        private Visibility _loaderVisibility = Visibility.Hidden;
        private bool _isControlEnabled = true;

        private ObservableCollection<Provider> _processes;
        private readonly Thread _updateThread;
        private Provider _selectedProcess;
        private RelayCommand _endTaskCommand;
        private RelayCommand _getMetaDataCommand;
        private RelayCommand _openFileLocationCommand;

        public bool Selected => SelectedProcess != null;

        public Visibility LoaderVisibility
        {
            get => _loaderVisibility;
            set
            {
                _loaderVisibility = value;
                OnPropertyChanged();
            }
        }

        public bool IsControlEnabled
        {
            get => _isControlEnabled;
            set
            {
                _isControlEnabled = value;
                OnPropertyChanged();
            }
        }

        public Provider SelectedProcess
        {
            get => _selectedProcess;
            set
            {
                _selectedProcess = value;
                OnPropertyChanged();
                OnPropertyChanged("IsItemSelected");
            }
        }

        public ObservableCollection<Provider> Processes
        {
            get => _processes;
            private set
            {
                _processes = value;
                OnPropertyChanged();
            }
        }

        internal MainGrid()
        {
            LoaderManager.Instance.Initialize(this);
            _updateThread = new Thread(UpdateUsers);
            var initializationThread = new Thread(InitializeProcesses);
            initializationThread.Start();
        }

        private async void InitializeProcesses()
        {
            LoaderManager.Instance.ShowLoader();
            await Task.Run(() => { Processes = new ObservableCollection<Provider>(Updater.ProcessesList.Values); });
            _updateThread.Start();
            while (Updater.ProcessesList.Count == 0)
                Thread.Sleep(5000);
            LoaderManager.Instance.HideLoader();
        }

        internal void Close()
        {
            _updateThread.Join(2000);
        }

        public RelayCommand EndTaskCommand =>
            _endTaskCommand ?? (_endTaskCommand = new RelayCommand(EndTaskImplementation));

        public RelayCommand GetInfoCommand =>
            _getMetaDataCommand ?? (_getMetaDataCommand = new RelayCommand(GetInfoImplementation));

        public RelayCommand OpenFileLocationCommand => _openFileLocationCommand ??
                                                       (_openFileLocationCommand =
                                                           new RelayCommand(OpenLocationImplementation));

        private void EndTaskImplementation(object obj)
        {
            if (Application.Current.Dispatcher != null)
                Application.Current.Dispatcher.Invoke(delegate
                {
                    var process = Process.GetProcessById(SelectedProcess.Id);
                    try
                    {
                        process.Kill();
                    }
                    catch (Win32Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                });
        }

        private async void GetInfoImplementation(object obj)
        {
            try
            {
                await Task.Run(() =>
                {
                    if (Application.Current.Dispatcher != null)
                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            var process = Process.GetProcessById(SelectedProcess.Id);
                            NavigationManager.Close();
                            try
                            {
                                NavigationManager.Navigate(process);
                            }
                            catch (Win32Exception e)
                            {
                                MessageBox.Show(e.Message);
                            }
                        });
                });
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private async void OpenLocationImplementation(object o)
        {
            await Task.Run(() =>
            {
                var process = Process.GetProcessById(SelectedProcess.Id);
                try
                {
                    if (process.MainModule == null) return;
                    var fullPath = process.MainModule.FileName;
                    Process.Start("explorer.exe", fullPath.Remove(fullPath.LastIndexOf('\\')));
                }
                catch (Win32Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            });
        }

        private async void UpdateUsers()
        {
            while (true)
            {
                await Task.Run(() =>
                {
                    if (Application.Current.Dispatcher != null)
                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            try
                            {
                                lock (Processes)
                                {
                                    var toRemove =
                                        new List<Provider>(
                                            Processes.Where(proc => !Updater.ProcessesList.ContainsKey(proc.Id)));
                                    foreach (var proc in toRemove) Processes.Remove(proc);

                                    var toAdd =
                                        new List<Provider>(
                                            Updater.ProcessesList.Values.Where(proc => !Processes.Contains(proc)));
                                    foreach (var proc in toAdd) Processes.Add(proc);
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        });
                });
                Thread.Sleep(5000);
            }
        }
    }
}