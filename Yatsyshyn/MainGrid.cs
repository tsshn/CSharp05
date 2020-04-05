namespace Yatsyshyn
{
    internal partial class MainGrid
    {
        internal MainGrid()
        {
            InitializeComponent();
            DataContext = new ViewModels.MainGrid();
        }

        internal void Close()
        {
            ((ViewModels.MainGrid) DataContext).Close();
        }
    }
}