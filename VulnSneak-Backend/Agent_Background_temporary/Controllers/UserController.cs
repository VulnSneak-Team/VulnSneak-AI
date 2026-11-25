using Agent_Background_temporary.Dtos;
using Agent_Background_temporary.Models;
using Agent_Background_temporary.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Agent_Background_temporary.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly string _basePath;
        private readonly UnitServices _unitServices;
        private int UserId
        {
            get
            {
                if (field == default)
                    int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value, out field);
                return field;
            }
        }

        public UserController(UnitServices unitServices, IConfiguration configuration)
        {
            _unitServices = unitServices;
            _basePath = configuration["FileStorage:BasePath"]!;
        }

        // GET api/v1/User
        [HttpGet]
        public async Task<IActionResult> GetUserAsync()
        {
            User? user = await _unitServices.userService.GetUserAsync(UserId);
            if (user is null)
                return NotFound(new { message = "User not found." });

            UserDto userDto = _unitServices.userService.MapToUserDto(user);

            return Ok(userDto);
        }

        // PUT api/v1/User
        [HttpPut]
        public async Task<IActionResult> UpdateUserAsync(UpdateUserDto updateUserDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            User? user = await _unitServices.userService.GetUserAsync(UserId, Tracking: true);
            if (user is null)
                return NotFound(new { message = "User not found." });

            _unitServices.userService.UpdateUser(user, updateUserDto);
            await _unitServices.SaveInDataBaseAsync();

            return Ok(new { message = "User updated successfully." });
        }

        // PUT api/v1/User/change-password
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePasswordAsync(PasswordDto passwordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            User? user = await _unitServices.userService.GetUserAsync(UserId, Tracking: true);
            if (user is null)
                return NotFound(new { message = "User not found." });

            if (!_unitServices.userService.UpdatePassword(user, passwordDto))
                return BadRequest(new { message = "Current password is incorrect." });

            await _unitServices.SaveInDataBaseAsync();

            return Ok(new { message = "Password changed successfully." });
        }

        // DELETE api/v1/User
        [HttpDelete]
        public async Task<IActionResult> DeleteUserAsync()
        {
            User? user = await _unitServices.userService.GetUserAsync(UserId);
            if (user is null)
                return NotFound(new { message = "User not found." });

            bool userDeleted = await _unitServices.userService.DeleteUserAsync(user, _basePath);
            if (!userDeleted)
                return StatusCode(500);
            await _unitServices.SaveInDataBaseAsync();
            return Ok(new { message = "User deleted successfully." });
        }
    }
}