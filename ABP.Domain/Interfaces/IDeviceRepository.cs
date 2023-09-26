using ABP.Domain.Entities;

namespace ABP.Domain.Repository
{
    public interface IDeviceRepository
    {
        Task AddDeviceAsync(string deviceToken, string experimentName, string receivedValue);
        Task<List<Device>> GetAllDevicesAsync();
        Task<Device> GetDeviceAsync(string deviceToken);
    }
}