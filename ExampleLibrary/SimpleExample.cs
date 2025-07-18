using System;

namespace ExampleLibrary;

internal enum DeviceStatus { Off, On, Standby, Error }

public interface IDevice
{
    #region Interface properties
    
    bool IsActive { get; }
    
    #endregion // =================================================================================
    
    #region Interface methods
    
    void TurnOn();
    void TurnOff();
    
    #endregion
}

public class Device : IDevice
{
    #region Fields
    
    private DeviceStatus _status = DeviceStatus.Off;
    
    #endregion // =================================================================================
    
    #region Properties
    
    public string Name { get; set; } = "MyDevice";
    
    public bool IsActive => _status == DeviceStatus.On;
    
    #endregion // =================================================================================
    
    #region Methods
    
    public int GetStatusCode() => (int)_status;
    public void PrintStatus() => Console.WriteLine($"Device '{Name}' is currently: {_status}");
    public string SetStatus(int value)
    {
        var oldStatus = _status;
        _status = (DeviceStatus)value;
        Console.WriteLine($"Status changed from {oldStatus} to {_status}");
        return $"Status updated to {value}";
    }
    public void TurnOn() => _status = DeviceStatus.On;
    public void TurnOff() => _status = DeviceStatus.Off;
    
    #endregion
}