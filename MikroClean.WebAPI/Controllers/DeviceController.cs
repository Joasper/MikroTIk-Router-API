using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MikroClean.Application.Dtos;
using MikroClean.Application.Interfaces;

namespace MikroClean.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly IDeviceService deviceService;

        public DeviceController(IDeviceService deviceService)
        {
            this.deviceService = deviceService;
        }


        [HttpGet] 
        public async Task<ActionResult<IEnumerable<DeviceDto>>> GetAllDevices()
        {
            return Ok(await deviceService.GetAllDevicesAsync());
        }

        [HttpPost]
        public async Task<ActionResult<DeviceDto>> CreateDevice([FromBody] CreateDeviceDto createDeviceDto)
        {
            var createdDevice = await deviceService.CreateDeviceAsync(createDeviceDto);
            return CreatedAtAction(nameof(GetAllDevices), new { id = createdDevice.Id }, createdDevice);

        }
    }
}
