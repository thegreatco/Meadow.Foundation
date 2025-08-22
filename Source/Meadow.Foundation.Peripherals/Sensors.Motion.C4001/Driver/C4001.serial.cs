using Meadow.Hardware;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Meadow.Foundation.Sensors.Motion;

public partial class C4001
{
    //The baud rate is 9600, 8 bits, no parity, with one stop bit
    readonly ISerialMessagePort? serialMessagePort;

    static readonly byte[] suffixDelimiter = { 13 }; //ASCII return
    static readonly int portSpeed = 9600;

    /// <summary>
    /// Creates a new C4001 object communicating over serial
    /// </summary>
    /// <param name="serialMessage">The serial message port</param>
    internal C4001(ISerialMessagePort serialMessage)
    {
        serialMessagePort = serialMessage;
        serialMessagePort.MessageReceived += SerialMessagePort_MessageReceived;

        communication = CommunicationType.Serial;
    }

    internal SensorStatus GetStatusSerial()
    {
        SensorStatus status = new SensorStatus();

        //TODO

        return status;
    }

    internal bool IsMotionDetectedSerial()
    {
        //TODO
        return false;
    }

    internal void SetSensorSerial(SensorCommand command)
    {
        //TODO
    }

    internal void SetSensorModeSerial(SensorMode mode)
    {
        //TODO
    }

    internal bool SetTrigSensitivitySerial(byte sensitivity)
    {
        //TODO
        return false;
    }

    internal byte GetTrigSensitivitySerial()
    {
        //TODO
        return 0;
    }

    internal bool SetKeepSensitivitySerial(byte sensitivity)
    {
        //TODO
        return false;
    }


    internal byte GetKeepSensitivitySerial()
    {
        //TODO
        return 0;
    }

    internal bool SetDelaySerial(byte delay)
    {
        //TODO
        return false;
    }

    internal byte GetTrigDelaySerial()
    {
        //TODO
        return 0;
    }

    internal ushort GetKeepTimeoutSerial()
    {
        //TODO
        return 0;
    }

    internal bool SetDetectionRangeSerial(ushort range)
    {
        //TODO
        return false;
    }

    internal ushort GetTrigRangeSerial()
    {
        //TODO
        return 0;
    }

    internal ushort GetMaxRangeSerial()
    {
        //TODO
        return 0;
    }

    internal ushort GetMinRangeSerial()
    {
        //TODO
        return 0;
    }

    internal byte GetTargetNumberSerial()
    {
        //TODO
        return 0;
    }

    internal bool SetDetectThresholdSerial(ushort min, ushort max, ushort threshold)
    {
        //TODO
        return false;
    }

    internal bool SetIoPolaritySerial(byte polarity)
    {
        //TODO
        return false;
    }

    internal bool SetPwmI2cSerial(byte pwm1, byte pwm2, byte timer)
    {
        //TODO
        return false;
    }


    private void SerialMessagePort_MessageReceived(object sender, SerialMessageData e)
    {
    }

    internal ResponseData ParseResponse(byte[] data, int length, int count)
    {
        var response = new ResponseData();
        int startIdx = -1;

        // Find the index where "Res" starts
        for (int i = 0; i < length - 2; i++)
        {
            if (data[i] == 'R' && data[i + 1] == 'e' && data[i + 2] == 's')
            {
                startIdx = i;
                break;
            }
        }

        if (startIdx <= 0)
        {
            response.Status = false;
            return response;
        }

        response.Status = true;
        var spaceIndices = new List<int>();

        for (int i = startIdx; i < length; i++)
        {
            if (data[i] == ' ')
            {
                spaceIndices.Add(i + 1); // position after space
            }
        }

        if (spaceIndices.Count > 0)
        {
            if (spaceIndices.Count >= 1)
                response.Response1 = ParseFloat(data, spaceIndices[0], length);

            if (spaceIndices.Count >= 2)
                response.Response2 = ParseFloat(data, spaceIndices[1], length);

            if (count == 3 && spaceIndices.Count >= 3)
                response.Response3 = ParseFloat(data, spaceIndices[2], length);
        }
        else
        {
            response.Response1 = 0.0f;
            response.Response2 = 0.0f;
            response.Response3 = 0.0f;
        }

        return response;
    }

    private static float ParseFloat(byte[] data, int start, int maxLength)
    {
        int end = start;
        while (end < maxLength && data[end] != ' ' && data[end] != '\n' && data[end] != '\r')
            end++;

        string numberStr = Encoding.ASCII.GetString(data, start, end - start);
        return float.TryParse(numberStr, NumberStyles.Any, CultureInfo.InvariantCulture, out float result)
            ? result
            : 0.0f;
    }
}