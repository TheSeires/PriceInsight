using API.Models.Application;

namespace API.Services.Interfaces;

public interface IServiceStateTracker
{
    void SetServiceState<T>(ServiceState state);
    void SetServiceState<T>(string subKey, ServiceState state);
    ServiceState GetServiceState<T>();
    ServiceState GetServiceState<T>(string subKey);
}
