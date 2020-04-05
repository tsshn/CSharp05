using System.ComponentModel;

namespace Yatsyshyn
{
    internal partial class MainWindow
    {
        private MainGrid _mainGrid;

        public MainWindow()
        {
            InitializeComponent();
            ShowProcessesListView();
        }

        private void ShowProcessesListView()
        {
            MainGrid.Children.Clear();
            if (_mainGrid == null)
                _mainGrid = new MainGrid();
            MainGrid.Children.Add(_mainGrid);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _mainGrid?.Close();
            Updater.Close();
            base.OnClosing(e);
        }
    }
}