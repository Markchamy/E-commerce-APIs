using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("admin/api/2024-01")]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionServices _permissionRepository;
        public PermissionController(IPermissionServices permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }

        [HttpGet("permissions")]
        public async Task<IActionResult> GetAllPermissions()
        {
            try
            {
                var permissions = await _permissionRepository.GetAllPermissionsAsync();
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An internal error occurred while fetching the permissions: " + ex.Message);
            }
        }
    }
}
