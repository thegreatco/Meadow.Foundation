using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.RTCs;

/// <summary>
/// Represents a Ab0805 real-time clock
/// </summary>
public partial class Ab0805 : II2cPeripheral, IRealTimeClock
{
    /// <summary>
    /// The default I2C address for the peripheral
    /// </summary>
    public byte DefaultI2cAddress => (byte)Addresses.Default;

    /// <summary>
    /// Gets or sets whether the RTC is running
    /// </summary>
    public bool IsRunning
    {
        get
        {
            byte control1 = i2CCommunications.ReadRegister((byte)Registers.CONTROL1);
            return (control1 & (1 << Control1Bits.STOP)) == 0;
        }
        set
        {
            byte control1 = i2CCommunications.ReadRegister((byte)Registers.CONTROL1);
            if (value == true)
            {
                control1 = (byte)(control1 & ~(1 << Control1Bits.STOP));
            }
            else
            {
                control1 |= (byte)(1 << Control1Bits.STOP);
            }
            control1 |= (1 << Control1Bits.WRTC);
            i2CCommunications.WriteRegister((byte)Registers.CONTROL1, control1);
        }
    }

    /// <summary>
    /// Has the timer ended
    /// </summary>
    public bool HasTimerEnded
    {
        get
        {
            byte status = i2CCommunications.ReadRegister((byte)Registers.STATUS);
            return (status & (1 << StatusBits.TIM)) != 0;
        }
    }

    private I2cCommunications i2CCommunications;

    /// <summary>
    /// Creates a new Ab0805 object
    /// </summary>
    /// <param name="i2cBus">The I2C bus</param>
    public Ab0805(II2cBus i2cBus)
    {
        i2CCommunications = new I2cCommunications(i2cBus, (byte)Addresses.Default, 20);
        Initialize();
    }

    private void Initialize()
    {
        i2CCommunications.WriteRegister((byte)Registers.CONTROL1, 0x91);
        i2CCommunications.WriteRegister((byte)Registers.CONFIG_KEY, 0xA1);
        i2CCommunications.WriteRegister((byte)Registers.OSC_CONTROL, 0x08);
    }

    /// <summary>
    /// Gets the current time from the RTC.
    /// </summary>
    /// <returns>A <see cref="DateTimeOffset"/> object representing the current time.</returns>
    public DateTimeOffset GetTime()
    {
        byte[] timeData = new byte[7];
        i2CCommunications.ReadRegister((byte)Registers.SECONDS, timeData);

        int sec = BcdToDecimalWithMask((byte)(timeData[0] & 0x7F), 0x70);
        int min = BcdToDecimalWithMask((byte)(timeData[1] & 0x7F), 0x70);
        int hour = BcdToDecimalWithMask((byte)(timeData[2] & 0x3F), 0x30);
        int day = BcdToDecimalWithMask((byte)(timeData[3] & 0x3F), 0x30);
        int month = BcdToDecimalWithMask((byte)(timeData[4] & 0x1F), 0x10);
        int year = 2000 + BcdToDecimal(timeData[5]);

        if (year < 2000 || year > 2099 || month < 1 || month > 12 || day < 1 || hour < 0 || hour > 23 || min < 0 || min > 59 || sec < 0 || sec > 59)
        {
            throw new Exception("Invalid time data from RTC");
        }
        if (day > DateTime.DaysInMonth(year, month))
        {
            throw new Exception("Invalid date data from RTC");
        }

        return new DateTimeOffset(year, month, day, hour, min, sec, TimeSpan.Zero);
    }

    /// <summary>
    /// Sets the time on the RTC.
    /// </summary>
    /// <param name="time">The <see cref="DateTimeOffset"/> to set.</param>
    public void SetTime(DateTimeOffset time)
    {
        byte control1 = i2CCommunications.ReadRegister((byte)Registers.CONTROL1);

        // Ensure 24-hour mode (Bit 6 = 0)
        if ((control1 & (1 << Control1Bits.HourFormat_12_24)) != 0) // If in 12hr mode
        {
            control1 = (byte)(control1 & ~(1 << Control1Bits.HourFormat_12_24)); // Set to 24hr mode
        }

        // Ensure WRTC is enabled (Bit 0 = 1)
        control1 |= (1 << Control1Bits.WRTC);
        i2CCommunications.WriteRegister((byte)Registers.CONTROL1, control1);

        bool wasRunning = (control1 & (1 << Control1Bits.STOP)) == 0;
        if (wasRunning)
        {
            // Stop the clock before setting time by modifying bit in current interrupts value
            byte stopCommand = (byte)(control1 | (1 << Control1Bits.STOP));
            i2CCommunications.WriteRegister((byte)Registers.CONTROL1, stopCommand);
            Thread.Sleep(10); // Give it a moment to stop if needed
        }

        if (time.Year < 2000 || time.Year > 2099)
        {
            throw new ArgumentOutOfRangeException(nameof(time), "Year must be between 2000 and 2099.");
        }
        i2CCommunications.WriteRegister((byte)Registers.YEAR, DecimalToBcd(time.Year - 2000));
        i2CCommunications.WriteRegister((byte)Registers.MONTH, DecimalToBcd(time.Month));
        i2CCommunications.WriteRegister((byte)Registers.DATE, DecimalToBcd(time.Day));

        int rtcDow = (time.DayOfWeek == DayOfWeek.Sunday) ? 7 : (int)time.DayOfWeek;
        byte currentDowReg = i2CCommunications.ReadRegister((byte)Registers.DAY_OF_WEEK);
        byte newDowReg = (byte)((currentDowReg & ~DayOfWeekBits.Mask) | (rtcDow & DayOfWeekBits.Mask));
        i2CCommunications.WriteRegister((byte)Registers.DAY_OF_WEEK, newDowReg);

        i2CCommunications.WriteRegister((byte)Registers.HOURS, DecimalToBcd(time.Hour));
        i2CCommunications.WriteRegister((byte)Registers.MINUTES, DecimalToBcd(time.Minute));
        i2CCommunications.WriteRegister((byte)Registers.SECONDS, DecimalToBcd(time.Second)); // CH bit (7) will be 0

        SetHundredths(0);

        if (wasRunning)
        {
            byte startCommand = (byte)((control1 & ~(1 << Control1Bits.STOP)) | (1 << Control1Bits.WRTC));
            i2CCommunications.WriteRegister((byte)Registers.CONTROL1, startCommand);
        }
    }

    public void StartTimer()
    {

    }

    void SetTimerInterrupt(bool enable)
    {
        byte interrupts = i2CCommunications.ReadRegister((byte)Registers.INT_MASK);

        if (enable)
        {
            interrupts |= (1 << InterruptMaskBits.TIE);
        }
        else
        {
            var value = (byte)InterruptMaskBits.TIE;
            interrupts &= (byte)~(1 << value);
        }
        i2CCommunications.WriteRegister((byte)Registers.INT_MASK, interrupts);
    }

    void SetAlarmInterrupt(bool enable)
    {
        byte interrupts = i2CCommunications.ReadRegister((byte)Registers.INT_MASK);

        if (enable)
        {
            interrupts |= (1 << InterruptMaskBits.AIE);
        }
        else
        {
            var value = (byte)InterruptMaskBits.AIE;
            interrupts &= (byte)~(1 << value);
        }
        i2CCommunications.WriteRegister((byte)Registers.INT_MASK, interrupts);
    }

    /// <summary>
    /// Reads the hundredths of a second from the RTC.
    /// Note: This is only valid if using an XT oscillator.
    /// </summary>
    /// <returns>Hundredths of a second (0-99).</returns>
    int GetHundredths()
    {
        byte bcdHundredths = i2CCommunications.ReadRegister((byte)Registers.HUNDREDTHS);
        return BcdToDecimal(bcdHundredths);
    }

    /// <summary>
    /// Sets the hundredths of a second on the RTC.
    /// Note: This is only valid if using an XT oscillator.
    /// </summary>
    /// <param name="hundredths">Hundredths of a second (0-99).</param>
    void SetHundredths(int hundredths)
    {
        if (hundredths < 0 || hundredths > 99)
        {
            throw new ArgumentOutOfRangeException(nameof(hundredths), "Hundredths must be between 0 and 99.");
        }
        i2CCommunications.WriteRegister((byte)Registers.HUNDREDTHS, DecimalToBcd(hundredths));
    }

    private byte DecimalToBcd(int value)
    {
        return (byte)(((value / 10) << 4) | (value % 10));
    }

    private int BcdToDecimal(byte bcdValue)
    {
        return ((bcdValue >> 4) * 10) + (bcdValue & 0x0F);
    }

    private int BcdToDecimalWithMask(byte bcdValue, byte tensMask)
    {
        return (bcdValue & 0x0F) + (((bcdValue & tensMask) >> 4) * 10);
    }
}