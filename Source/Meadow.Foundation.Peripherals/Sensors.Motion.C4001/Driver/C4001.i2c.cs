using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading;

namespace Meadow.Foundation.Sensors.Motion;

public partial class C4001 : II2cPeripheral
{
    /// <inheritdoc/>
    public byte DefaultI2cAddress => (byte)Addresses.Default;

    private II2cCommunications? I2cComms = null;

    /// <summary>
    /// Create a new C4001 object connected to an input pin and IO Device
    /// </summary>
    /// <param name="i2cBus">The I2C bus</param>
    /// <param name="address"> The I2C address of the device</param>
    public C4001(II2cBus i2cBus, byte address = (byte)Addresses.Default)
    {
        I2cComms = new I2cCommunications(i2cBus, address);

        communication = CommunicationType.I2C;
    }

    internal SensorStatus GetStatusI2c()
    {
        var status = new SensorStatus();

        byte val = I2cComms!.ReadRegister((byte)Registers.STATUS);

        status.WorkStatus = (byte)(val & 0x01);
        status.WorkMode = (byte)((val & 0x02) >> 1);
        status.InitStatus = (byte)((val & 0x80) >> 7);

        return status;
    }

    internal bool IsMotionDetectedI2c()
    {
        byte val = I2cComms!.ReadRegister((byte)Registers.RESULT_STATUS);

        return (val & 0x01) != 0;
    }

    internal void SetSensorI2c(SensorCommand command)
    {
        byte register;
        int delayMs;

        switch (command)
        {
            case SensorCommand.Start:
                register = (byte)Registers.CTRL0;
                delayMs = 200;
                break;
            case SensorCommand.Stop:
                register = (byte)Registers.CTRL0;
                delayMs = 200;
                break;
            case SensorCommand.Reset:
                register = (byte)Registers.CTRL0;
                delayMs = 1500;
                break;
            case SensorCommand.SaveParams:
                register = (byte)Registers.CTRL1;
                delayMs = 500;
                break;
            case SensorCommand.Recover:
                register = (byte)Registers.CTRL1;
                delayMs = 800;
                break;
            case SensorCommand.ChangeMode:
                register = (byte)Registers.CTRL1;
                delayMs = 1500;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(command), command, "Unknown sensor command");
        }

        I2cComms!.WriteRegister(register, (byte)command);
        Thread.Sleep(delayMs);
    }

    internal bool SetSensorModeI2c(SensorMode mode)
    {
        var status = GetStatusI2c();

        if (status.WorkMode == (byte)mode)
        {
            return true;
        }

        SetSensorI2c(SensorCommand.ChangeMode);
        status = GetStatusI2c();
        return status.WorkMode == (byte)mode;
    }

    internal bool SetTrigSensitivityI2c(byte sensitivity)
    {
        if (sensitivity > 9)
            return false;

        I2cComms!.WriteRegister((byte)Registers.TRIG_SENSITIVITY, sensitivity);
        SetSensorI2c(SensorCommand.SaveParams);

        return true;
    }

    internal byte GetTrigSensitivityI2c()
    {
        return I2cComms!.ReadRegister((byte)Registers.TRIG_SENSITIVITY);
    }

    internal bool SetKeepSensitivityI2c(byte sensitivity)
    {
        if (sensitivity > 9)
        {
            return false;
        }
        I2cComms!.WriteRegister((byte)Registers.KEEP_SENSITIVITY, sensitivity);
        SetSensorI2c(SensorCommand.SaveParams);
        return true;
    }

    internal byte GetKeepSensitivityI2c()
    {
        return I2cComms!.ReadRegister((byte)Registers.KEEP_SENSITIVITY);
    }

    internal bool SetDelayI2c(byte trig, ushort keep)
    {
        if (trig > 200)
        {
            return false;
        }

        if (keep < 4 || keep > 3000)
        { return false; }

        byte[] data =
        [
            trig,
            (byte)(keep & 0xFF),      // low byte
            (byte)((keep >> 8) & 0xFF), // high byte
        ];
        I2cComms!.WriteRegister((byte)Registers.TRIG_DELAY, data);
        SetSensorI2c(SensorCommand.SaveParams);

        return true;
    }

    internal byte GetTrigDelayI2c()
    {
        return I2cComms!.ReadRegister((byte)Registers.TRIG_DELAY);
    }

    internal ushort GetKeepTimeoutI2c()
    {
        Span<byte> buffer = stackalloc byte[2];
        I2cComms!.ReadRegister((byte)Registers.KEEP_TIMEOUT_L, buffer);

        return (ushort)((buffer[1] << 8) | buffer[0]);
    }

    internal bool SetDetectionRangeI2c(ushort min, ushort max, ushort trig)
    {
        if (max < 240 || max > 2000)
        { return false; }

        if (min < 30 || min > max)
        { return false; }

        if (I2cComms is null)
        { return false; }

        byte[] data =
        [
            (byte)(min & 0xFF),      // min low byte
            (byte)((min >> 8) & 0xFF), // min high byte
            (byte)(max & 0xFF),      // max low byte
            (byte)((max >> 8) & 0xFF), // max high byte
            (byte)(trig & 0xFF),     // trig low byte
            (byte)((trig >> 8) & 0xFF), // trig high byte
        ];
        I2cComms.WriteRegister((byte)Registers.E_MIN_RANGE_L, data);
        SetSensorI2c(SensorCommand.SaveParams);

        return true;
    }

    internal ushort GetTrigRangeI2c()
    {
        Span<byte> buffer = stackalloc byte[2];
        I2cComms!.ReadRegister((byte)Registers.E_TRIG_RANGE_L, buffer);

        return (ushort)(buffer[0] | (buffer[1] << 8));
    }

    internal ushort GetMaxRangeI2c()
    {
        Span<byte> buffer = stackalloc byte[2];
        I2cComms!.ReadRegister((byte)Registers.E_MAX_RANGE_L, buffer);

        return (ushort)(buffer[0] | (buffer[1] << 8));
    }

    internal ushort GetMinRangeI2c()
    {
        Span<byte> buffer = stackalloc byte[2];
        I2cComms!.ReadRegister((byte)Registers.E_MIN_RANGE_L, buffer);

        return (ushort)(buffer[0] | (buffer[1] << 8));
    }

    private void UpdateMotionDataI2c()
    {
        byte[] temp = new byte[7];
        I2cComms!.ReadRegister((byte)Registers.RESULT_OBJ_MUN, temp);

        if (temp[0] == 1)
        {
            motionTimeoutCount = 0;
            motionData.Number = 1;
            motionData.Range = BitConverter.ToInt16(temp, 1) / 100.0f;
            motionData.Speed = BitConverter.ToInt16(temp, 3) / 100.0f;
            motionData.Energy = (uint)BitConverter.ToInt16(temp, 5);
        }
        else
        {
            if (++motionTimeoutCount > 10)
            {
                motionData.Number = 0;
                motionData.Range = 0;
                motionData.Speed = 0;
                motionData.Energy = 0;
            }
        }
    }

    internal byte GetTargetNumberI2c()
    {
        UpdateMotionDataI2c();
        return motionData.Number;
    }

    internal Length GetTargetRangeI2c()
    {
        UpdateMotionDataI2c();
        return new Length(motionData.Range, Length.UnitType.Meters);
    }

    public uint GetTargetEnergyI2c()
    {
        UpdateMotionDataI2c();
        return motionData.Energy;
    }

    internal bool SetDetectThresholdI2c(ushort min, ushort max, ushort threshold)
    {
        if (max > 2500)
            return false;

        if (min > max)
            return false;

        if (I2cComms is null)
            return false;

        byte[] data =
        [
            (byte)(threshold & 0xFF),         // threshold low byte
            (byte)((threshold >> 8) & 0xFF),  // threshold high byte
            (byte)(min & 0xFF),               // min low byte
            (byte)((min >> 8) & 0xFF),        // min high byte
            (byte)(max & 0xFF),               // max low byte
            (byte)((max >> 8) & 0xFF),        // max high byte
        ];
        I2cComms.WriteRegister((byte)Registers.CFAR_THR_L, data);
        SetSensorI2c(SensorCommand.SaveParams);

        return true;
    }

    internal bool SetIoPolarityI2c(byte value)
    {
        return true;
    }

    internal byte GetIoPolarityI2c()
    {
        return 0;
    }

    internal bool SetPwmI2c(byte pwm1, byte pwm2, byte timer)
    {
        // PWM isn't supported over I2C
        return false;
    }

    internal PwmData GetPwmI2c()
    {
        // PWM isn't supported over I2C
        return new PwmData();
    }

    internal ushort GetTMinRangeI2c()
    {
        Span<byte> buffer = stackalloc byte[2];
        I2cComms!.ReadRegister((byte)Registers.T_MIN_RANGE_L, buffer);
        return (ushort)(buffer[0] | (buffer[1] << 8));
    }

    internal ushort GetTMaxRangeI2c()
    {
        Span<byte> buffer = stackalloc byte[2];
        I2cComms!.ReadRegister((byte)Registers.T_MAX_RANGE_L, buffer);
        return (ushort)(buffer[0] | (buffer[1] << 8));
    }

    internal ushort GetThresholdRangeI2c()
    {
        Span<byte> buffer = stackalloc byte[2];
        I2cComms!.ReadRegister((byte)Registers.CFAR_THR_L, buffer);
        return (ushort)(buffer[0] | (buffer[1] << 8));
    }

    internal void SetFrettingDetectionI2c(SwitchState state)
    {
        Span<byte> buffer = stackalloc byte[1];
        buffer[0] = (byte)state;
        I2cComms!.WriteRegister((byte)Registers.MICRO_MOTION, buffer);
        SetSensorI2c(SensorCommand.SaveParams);
    }

    internal SwitchState GetFrettingDetectionI2c()
    {
        Span<byte> buffer = stackalloc byte[1];
        I2cComms!.ReadRegister((byte)Registers.MICRO_MOTION, buffer);
        return (SwitchState)buffer[0];
    }
}