using Avalonia.Controls;
using ScheduleEditor.ViewModels;

namespace ScheduleEditor.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}