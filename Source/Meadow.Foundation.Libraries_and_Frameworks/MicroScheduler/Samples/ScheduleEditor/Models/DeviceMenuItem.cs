namespace ScheduleEditor.Models;

/// <summary>
/// UI model for a device menu item.
/// </summary>
public class DeviceMenuItem
{
    public string Header { get; set; } = string.Empty;
    public System.Windows.Input.ICommand? Command { get; set; }
    public object? CommandParameter { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string? Icon { get; set; }
}
