using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Environmental.Ysi;

/// <summary>
/// A class providing simulated EXO sonde behavior
/// </summary>
public class SimulatedExo : IExoSonde
{
    /// <inheritdoc/>
    public Task ForceSample()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<(ParameterCode ParameterCode, object Value)[]> GetCurrentData()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<Exo.ParameterStatus[]> GetParameterStatus()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<ParameterCode[]> GetParametersToRead()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<TimeSpan> GetSamplePeriod()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task SetParametersToRead(ParameterCode[] parameters)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task SetSamplePeriod(TimeSpan period)
    {
        throw new NotImplementedException();
    }
}
