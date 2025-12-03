# Cross-Platform Setup

This driver is compatible with Windows, macOS, and Linux. However, for macOS and Linux, you must manually install the FTDI D2XX drivers.

## macOS

*NOTE* - This may not be necessary, as MacOS includes a Virtual COM Port (MCP) driver since macOS 10.9 (Mavericks). If you are using an older version of macOS, you may need to install the FTDI D2XX drivers.

1. Download the D2XX driver for macOS from the [FTDI Drivers page](https://ftdichip.com/drivers/d2xx-drivers/).
2. Follow the installation instructions provided in the driver package.
3. Ensure `libftd2xx.dylib` is in your library path (e.g., `/usr/local/lib` or `/usr/lib`).

## Linux
1. Download the D2XX driver for Linux from the [FTDI Drivers page](https://ftdichip.com/drivers/d2xx-drivers/).
2. Extract the package and copy the library files to `/usr/local/lib` or `/usr/lib`.
3. Create a symlink if necessary (e.g., `ln -s libftd2xx.so.1.4.8 libftd2xx.so`).
4. You may need to add the user to the `dialout` or `plugdev` group to access the device without sudo.

# Usage
# Meadow.Foundation.ICs.IOExpanders.Ftxxxx

**Ft2xxx family of USB IOExpanders for GPIO, I2C, SPI on Windows**

The **Ft2xxx** library is included in the **Meadow.Foundation.ICs.IOExpanders.Ftxxxx** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.ICs.IOExpanders.Ftxxxx`
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
## About Meadow

Meadow is a complete, IoT platform with defense-grade security that runs full .NET applications on embeddable microcontrollers and Linux single-board computers including Raspberry Pi and NVIDIA Jetson.

### Build

Use the full .NET platform and tooling such as Visual Studio and plug-and-play hardware drivers to painlessly build IoT solutions.

### Connect

Utilize native support for WiFi, Ethernet, and Cellular connectivity to send sensor data to the Cloud and remotely control your peripherals.

### Deploy

Instantly deploy and manage your fleet in the cloud for OtA, health-monitoring, logs, command + control, and enterprise backend integrations.


