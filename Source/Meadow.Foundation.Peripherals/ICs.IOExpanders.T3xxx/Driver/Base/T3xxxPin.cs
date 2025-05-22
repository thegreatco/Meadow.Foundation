using Meadow.Hardware;
using System.Collections.Generic;

namespace Meadow.Foundation.IOExpanders;

public class T3xxxPin : IPin
{
    public IPinController? Controller { get; }

    public IList<IChannelInfo>? SupportedChannels { get; }

    public string Name { get; }

    public object Key { get; }

    internal T3xxxPin(IPinController device, string name, int key, params IChannelInfo[] channelInfo)
    {
        Controller = device;
        Name = name;
        Key = key;
        SupportedChannels = channelInfo;
    }

    public bool Equals(IPin other)
    {
        if (Name != other.Name) return false;
        if (Key != other.Key) return false;
        return true;
    }
}
