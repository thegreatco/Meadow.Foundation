using Meadow;
using Meadow.Devices;
using Meadow.Foundation.RTCs;
using System;
using System.Threading.Tasks;

namespace Ab0805_Sample
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        //<!=SNIP=>

        private Ab0805 rtc;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            rtc = new Ab0805(Device.CreateI2cBus());

            return base.Initialize();
        }

        public override async Task Run()
        {
            // Test basic RTC functionality
            await TestBasicRTC();

            // Test countdown timers
            await TestCountdownTimers();
        }

        private async Task TestBasicRTC()
        {
            Resolver.Log.Info("=== Testing Basic RTC Functionality ===");

            var running = rtc.IsRunning;
            Resolver.Log.Info($"RTC {(running ? "is running" : "is not running")}");

            if (!running)
            {
                Resolver.Log.Info("Starting RTC...");
                rtc.IsRunning = true;
            }

            var currentTime = rtc.GetTime();
            Resolver.Log.Info($"RTC current time: {currentTime:MM/dd/yy HH:mm:ss}");

            // Set RTC to a known time for testing
            var testTime = new DateTime(2025, 6, 15, 14, 30, 0);
            Resolver.Log.Info($"Setting RTC to: {testTime:MM/dd/yy HH:mm:ss}");
            rtc.SetTime(testTime);

            currentTime = rtc.GetTime();
            Resolver.Log.Info($"RTC time after setting: {currentTime:MM/dd/yy HH:mm:ss}");

            await Task.Delay(2000);

            currentTime = rtc.GetTime();
            Resolver.Log.Info($"RTC time after 2 second delay: {currentTime:MM/dd/yy HH:mm:ss}");
        }

        private async Task TestCountdownTimers()
        {
            Resolver.Log.Info("\n=== Testing Countdown Timer Functionality ===");

            // Test 1: Basic 5-second timer
            await TestBasicTimer();

            await Task.Delay(1000);
        }

        private async Task TestBasicTimer()
        {
            Resolver.Log.Info("\n--- Test 1: Basic 5-second countdown timer ---");

            rtc.ResetTimer();

            // Start a 5-second timer
            Resolver.Log.Info("Starting 5-second countdown timer...");
            rtc.StartTimer(5, Ab0805.DelayTimeUnit.Seconds);

            var startTime = DateTime.Now;

            // Monitor the timer
            while (rtc.HasTimerEnded == false)
            {
                var timerValue = 0; // rtc.GetCountdownTimerValue();
                var elapsed = DateTime.Now - startTime;
                Resolver.Log.Info($"Timer value: {timerValue}, Elapsed: {elapsed.TotalSeconds:F1}s");

                await Task.Delay(1000); // Check every second
            }

            // Check if interrupt fired
            if (rtc.HasTimerEnded)
            {
                var elapsed = DateTime.Now - startTime;
                Resolver.Log.Info($"✓ Timer completed! Interrupt fired after {elapsed.TotalSeconds:F1}s");
                rtc.ResetTimer();
            }
            else
            {
                Resolver.Log.Info("Timer completed but no interrupt detected");
            }
        }

        /*
        private async Task TestRepeatingTimer()
        {
            Resolver.Log.Info("\n--- Test 2: Repeating 3-second timer (will run 3 cycles) ---");

            rtc.ClearCountdownTimerInterrupt();

            // Start a repeating 3-second timer
            Resolver.Log.Info("Starting repeating 3-second timer...");
            rtc.StartCountdownTimer(3, CountdownFrequency.OneHz, repeatMode: true, enableInterrupt: true);

            int cycleCount = 0;
            var startTime = DateTime.Now;

            // Run for 3 cycles (about 9 seconds)
            while (cycleCount < 3)
            {
                if (rtc.CountdownTimerInterruptFired)
                {
                    cycleCount++;
                    var elapsed = DateTime.Now - startTime;
                    Resolver.Log.Info($"✓ Timer cycle {cycleCount} completed at {elapsed.TotalSeconds:F1}s");
                    rtc.ClearCountdownTimerInterrupt();
                }

                var timerValue = rtc.GetCountdownTimerValue();
                Resolver.Log.Info($"Timer value: {timerValue}, Cycle: {cycleCount}/3");

                await Task.Delay(500); // Check every half second
            }

            // Stop the repeating timer
            rtc.StopCountdownTimer();
            Resolver.Log.Info("Stopped repeating timer");
        }

        private async Task TestHighFrequencyTimer()
        {
            Resolver.Log.Info("\n--- Test 3: High frequency timer (64Hz for 2 seconds) ---");

            rtc.ClearCountdownTimerInterrupt();

            // Start a high-frequency timer - 128 ticks at 64Hz = 2 seconds
            Resolver.Log.Info("Starting 128-tick timer at 64Hz (should take ~2 seconds)...");
            rtc.StartCountdownTimer(128, CountdownFrequency.Hz64, repeatMode: false, enableInterrupt: true);

            var startTime = DateTime.Now;
            var lastValue = 128;

            while (rtc.IsCountdownTimerRunning)
            {
                var timerValue = rtc.GetCountdownTimerValue();
                var elapsed = DateTime.Now - startTime;

                // Only log when value changes significantly to avoid spam
                if (lastValue - timerValue >= 10 || timerValue == 0)
                {
                    Resolver.Log.Info($"Timer value: {timerValue}, Elapsed: {elapsed.TotalSeconds:F2}s");
                    lastValue = timerValue;
                }

                await Task.Delay(100); // Check every 100ms
            }

            if (rtc.CountdownTimerInterruptFired)
            {
                var elapsed = DateTime.Now - startTime;
                Resolver.Log.Info($"✓ High-frequency timer completed in {elapsed.TotalSeconds:F2}s");
                rtc.ClearCountdownTimerInterrupt();
            }
        }

        */

        //<!=SNOP=>
    }
}