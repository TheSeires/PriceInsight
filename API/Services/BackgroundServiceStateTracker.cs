using System.Collections.Concurrent;
using API.Models.Application;
using API.Services.Interfaces;

namespace API.Services;

public class ServiceStateTracker : IServiceStateTracker
{
    private readonly ConcurrentDictionary<string, ServiceState> _serviceStates = [];

    private string GetKeyWithSubKey<T>(string subKey) => $"{nameof(T)}_{subKey}";

    public void SetServiceState<T>(ServiceState state)
    {
        _serviceStates[nameof(T)] = state;
    }

    public void SetServiceState<T>(string subKey, ServiceState state)
    {
        _serviceStates[GetKeyWithSubKey<T>(subKey)] = state;
    }

    public ServiceState GetServiceState<T>()
    {
        _serviceStates.TryGetValue(nameof(T), out var state);
        return state;
    }

    public ServiceState GetServiceState<T>(string subKey)
    {
        _serviceStates.TryGetValue(GetKeyWithSubKey<T>(subKey), out var state);
        return state;
    }
}
