using System.ComponentModel;

namespace ScheduleEditor.Models;

/// <summary>
/// UI model for a serial port with selection state.
/// </summary>
public class SerialPortModel : INotifyPropertyChanged
{
    private bool _isSelected;

    public SerialPortModel(string portName)
    {
        PortName = portName;
    }

    public string PortName { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            OnPropertyChanged(nameof(IsSelected));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
