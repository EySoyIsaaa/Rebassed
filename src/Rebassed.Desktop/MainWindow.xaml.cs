using System.Windows;
using Rebassed.Desktop.ViewModels;

namespace Rebassed.Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
