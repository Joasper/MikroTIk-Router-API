
using MikroClean.Application.Dtos;

namespace MikroClean.Application.Interfaces
{
    public interface IDeviceService
    {
        Task<IEnumerable<DeviceDto>> GetAllDevicesAsync();
        Task<DeviceDto> GetDeviceByIdAsync(int id);
        Task<DeviceDto> CreateDeviceAsync(CreateDeviceDto createDeviceDto);
        Task UpdateDeviceAsync(int id, UpdateDeviceDto updateDeviceDto);
        Task DeleteDeviceAsync(int id);
    }
}
