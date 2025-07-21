namespace Meadow.Foundation.Sensors.Motion;

public partial class C4001
{
    public struct SensorStatus
    {
        public byte WorkStatus;  // 0=stop, 1=start
        public byte WorkMode;    // 0=presence, 1=speed
        public byte InitStatus;  // 0=not init, 1=init success
    }

    internal struct PrivateData
    {
        public byte Number;
        public float Speed;
        public float Range;
        public uint Energy;
    }

    internal struct ResponseData
    {
        public bool Status;
        public float Response1;
        public float Response2;
        public float Response3;
    }

    internal struct PwmData
    {
        public byte Pwm1;
        public byte Pwm2;
        public byte Timer;
    }

    internal struct AllData
    {
        public byte Exist;
        public SensorStatus Status;
        public PrivateData Target;
    }
}