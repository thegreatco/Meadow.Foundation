using Avalonia.Controls;
using ScheduleEditor.ViewModels;
using ScheduleEditor.Models;
using System.Collections.Generic;
using System.Linq;

namespace ScheduleEditor.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var viewModel = new MainWindowViewModel();
        DataContext = viewModel;
        
        // Subscribe to serial port updates
        viewModel.SerialPortsUpdated += UpdateDeviceMenu;
    }
    
    public void UpdateDeviceMenu(List<SerialPortModel> serialPorts, string? selectedPort)
    {
        // Find the Device menu
        var deviceMenu = DeviceMenu;
        if (deviceMenu == null) return;
        
        // Remove existing port items (keep the first 4: Refresh, Separator, Header, Loading/NoPortsFound)
        while (deviceMenu.Items.Count > 4)
        {
            deviceMenu.Items.RemoveAt(4);
        }
        
        // Remove the "Loading..." item
        if (deviceMenu.Items.Count == 4)
        {
            deviceMenu.Items.RemoveAt(3);
        }
        
        if (serialPorts.Count == 0)
        {
            // Add "No ports found" message
            var noPortsItem = new MenuItem
            {
                Header = "No ports found",
                IsEnabled = false
            };
            deviceMenu.Items.Add(noPortsItem);
        }
        else
        {
            // Add each serial port as a menu item
            foreach (var port in serialPorts.OrderBy(p => p.PortName))
            {
                var portItem = new MenuItem
                {
                    Header = port.PortName,
                    Command = ((MainWindowViewModel)DataContext).SelectSerialPortCommand,
                    CommandParameter = port.PortName
                };
                
                if (port.IsSelected)
                {
                    portItem.Icon = new TextBlock { Text = "✓" };
                }
                
                deviceMenu.Items.Add(portItem);
            }
        }
    }
}