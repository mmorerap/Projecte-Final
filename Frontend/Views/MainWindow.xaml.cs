using System.Windows;
using OCRDesktop.ViewModels;

namespace OCRDesktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
