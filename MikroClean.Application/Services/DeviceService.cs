using MikroClean.Application.Dtos;
using MikroClean.Application.Interfaces;
using MikroClean.Domain.Interfaces;
using MikroClean.Domain.Interfaces.UOW;
using MikroClean.Domain.Entities;

namespace MikroClean.Application.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly IDeviceRepository deviceRepository;
        private readonly IUnitOfWork unitOfWork;

        public DeviceService(IDeviceRepository deviceRepository, IUnitOfWork unitOfWork)
        {
            this.deviceRepository = deviceRepository;
            this.unitOfWork = unitOfWork;
        }
        public async Task<DeviceDto> CreateDeviceAsync(CreateDeviceDto createDeviceDto)
        {
            try
            {
                var device = new Device
                {
                    Name = createDeviceDto.Name,
                    Model = createDeviceDto.Model
                };
                deviceRepository.Add(device);
                await unitOfWork.SaveChangesAsync();
                return (new DeviceDto
                {
                    Id = device.Id,
                    Name = device.Name,
                    Model = device.Model
                });

            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred while creating the device: {e.Message}", e);
            }
        }

        public Task DeleteDeviceAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<DeviceDto>> GetAllDevicesAsync()
        {
            try
            {
                var devices = await deviceRepository.GetAllAsync();
                return devices.Select(d => new DeviceDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Model = d.Model
                });

            }
            catch (Exception e)
            {

                throw new Exception($"An error occurred while retrieving devices: {e.Message}", e);
            }

        }

        public Task<DeviceDto> GetDeviceByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateDeviceAsync(int id, UpdateDeviceDto updateDeviceDto)
        {
            throw new NotImplementedException();
        }
    }
}
