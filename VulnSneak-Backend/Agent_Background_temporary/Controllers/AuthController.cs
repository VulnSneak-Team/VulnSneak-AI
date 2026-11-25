using Agent_Background_temporary.Dtos;
using Agent_Background_temporary.Models;
using Agent_Background_temporary.Services;
using Microsoft.AspNetCore.Mvc;
namespace Agent_Background_temporary.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UnitServices _unitServices;
        private readonly IConfiguration Configuration;
        public AuthController(UnitServices unitServices, IConfiguration _configuration)
        {
            _unitServices = unitServices;
            Configuration = _configuration;
        }

        [HttpPost("register")]  // api/v1/register
        public async Task<IActionResult> RegisterAsync(RegisterDto registerDto)
        {
            if (registerDto is null)
                return BadRequest("Registration fail, User Invalid");

            User? user = await _unitServices.userService.GetUserByEmailAsync(registerDto.Email);
            if (user is not null)
                return BadRequest(new { message = $"Email {registerDto.Email} Already Exists " });

            _unitServices.userService.AddUser(ref user!, registerDto);
            await _unitServices.SaveInDataBaseAsync();

            return Ok(new
            {
                token = _unitServices.userService.CreateToken(user!, Configuration),
                expiration = DateTime.Now.AddHours(1),
                Name = $"{user!.Fname} {user.Lname}",
                user.Email
            });
        }

        [HttpPost("login")]  // api/v/login
        public async Task<IActionResult> LoginAsync(LoginDto loginDto)
        {
            if (loginDto is null)
                return BadRequest(new { message = "Login Failed, Invalid Data" });

            User? user = await _unitServices.userService.GetUserByEmailAsync(loginDto.Email);
            if (user is null)
                return NotFound(new { message = "Login Failed, Not Found User" });

            var isHashed = _unitServices.userService.VerifyPassword(user, user.Password, loginDto.Password);
            if (!isHashed)
                return BadRequest(new { message = "Login Failed." });

            return Ok(new
            {
                token = _unitServices.userService.CreateToken(user, Configuration),
                expiration = DateTime.Now.AddHours(1),
                Name = $"{user.Fname} {user.Lname}",
                user.Email
            });
        }
    }
}