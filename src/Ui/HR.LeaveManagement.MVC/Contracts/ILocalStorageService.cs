using System.Collections.Generic;

namespace HR.LeaveManagement.MVC.Contracts
{
    // we use local storage to store jwt tokens etc while the user is using the service
    // - the interface hides away the localstorage-nuget-package
    public interface ILocalStorageService
    {
        void ClearStorage(List<string> keys);
        bool Exists(string key);
        T GetStorageValue<T>(string key);
        void SetStorageValue<T>(string key, T value);
    }
}