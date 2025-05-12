using Meadow.Modbus;

namespace Meadow.Foundation;

public partial class T322ai : T3xxx
{
    public T322ai(ModbusRtuClient modbusRtuClient, byte moduleAddress)
    : base(modbusRtuClient, moduleAddress)
    {
    }

    public T322ai(ModbusTcpClient modbusTcpClient)
        : base(modbusTcpClient)
    {
    }
}
