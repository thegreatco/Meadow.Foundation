using Meadow.Hardware;
using Meadow.Modbus;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Meadow.Foundation;

public abstract partial class T3xxx
    : IPinController
{
    private T3ModuleModel? _model = null;
    private int? _serialNumber = null;
    private float? _firmware = null;
    private byte? _hardwareRevision = null;

    protected IModbusBusClient ModbusClient { get; }
    public byte ModbusAddress { get; private set; }

    protected T3xxx(ModbusRtuClient modbusRtuClient, byte moduleAddress)
    {
        ModbusClient = modbusRtuClient;
        ModbusAddress = moduleAddress;
    }

    public T3xxx(ModbusTcpClient modbusTcpClient)
    {
        ModbusClient = modbusTcpClient;
        ModbusAddress = 1;
    }

    private async Task ReadAndParseHeaderRegisters()
    {
        if (_model == null)
        {
            var registers = await ModbusClient.ReadHoldingRegisters(ModbusAddress, 0, 9);
            _serialNumber = registers[0] | registers[1] << 8 | registers[2] << 16 | registers[3] << 24;
            _firmware = (registers[4] / 10f);
            ModbusAddress = (byte)registers[6];
            _model = (T3ModuleModel)registers[7];
            _hardwareRevision = (byte)registers[8];
        }
    }

    internal async Task WriteHoldingRegister(ushort register, ushort value)
    {
        Debug.WriteLine($"write {register} = {value}");

        await ModbusClient.WriteHoldingRegister(this.ModbusAddress, register, value);
    }

    internal async Task<ushort> ReadHoldingRegister(ushort register)
    {
        return (await ModbusClient.ReadHoldingRegisters(this.ModbusAddress, register, 1))[0];
    }

    public async ValueTask<byte> ReadModbusAddress()
    {
        await ReadAndParseHeaderRegisters();
        return ModbusAddress;
    }

    public async Task<int> ReadSerialNumber()
    {
        await ReadAndParseHeaderRegisters();
        return _serialNumber!.Value;
    }

    public async Task<float> ReadFirmwareVersion()
    {
        await ReadAndParseHeaderRegisters();
        return _firmware!.Value;
    }

    public async Task<T3ModuleModel> ReadModel()
    {
        await ReadAndParseHeaderRegisters();
        return _model!.Value;
    }

    public async Task<byte> ReadHardwareRevision()
    {
        await ReadAndParseHeaderRegisters();
        return _hardwareRevision!.Value;
    }
}
