using System.Windows;
using WpfApp.ViewModels;

namespace WpfApp.Views;

public partial class MainWindow : Window
{
    public MainWindow(UserFormViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
