using Agent_Background_temporary.Dtos;
using Agent_Background_temporary.Models;
using Agent_Background_temporary.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Agent_Background_temporary.Services
{
    public class UserService
    {
        UnitRepositroy _unitRepo;
        private string? _basePath;

        public UserService(UnitRepositroy unitRepo, IConfiguration configuration)
        {
            _unitRepo = unitRepo;
            _basePath = configuration["FileStorage:BasePath"];
        }

        public async Task<User?> GetUserAsync(int userId, bool Tracking = false)
        {
            return await _unitRepo.UserRepo.GetUserAsync(userId, Tracking);
        }
        public async Task<User?> GetUserByEmailAsync(string Email)
        {
            return await _unitRepo.UserRepo.GetUserByEmailAsync(Email);
        }

        public void AddUser(ref User user, RegisterDto registerDto)
        {
            user = new User()
            {
                Fname = registerDto.Fname,
                Lname = registerDto.Lname,
                Email = registerDto.Email,
                Gender = registerDto.Gender,
                Age = registerDto.Age,
                Address = registerDto.Address,
                Phone = registerDto.Phone,
                RoleId = 2
            };
            user.Password = AccountService.HashPassword(user, registerDto.Password);
            _unitRepo.UserRepo.AddUser(user);
        }

        public void UpdateUser(User user, UpdateUserDto updateUserDto)
        {
            string userFolderName = $"{user.Id}_{UnitServices.SanitizeForPath(user.Fname)}_{UnitServices.SanitizeForPath(user.Lname)}";
            string userFolderPath = Path.Combine(_basePath!, userFolderName);
            if ((!string.Equals(user.Fname, updateUserDto.Fname, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(user.Lname, updateUserDto.Lname, StringComparison.OrdinalIgnoreCase))
                && Directory.Exists(userFolderPath))
            {
                string newUserFolderName = $"{user.Id}_{UnitServices.SanitizeForPath(updateUserDto.Fname)}_{UnitServices.SanitizeForPath(updateUserDto.Lname)}";
                string newUserFolderPath = Path.Combine(_basePath!, newUserFolderName);
                Directory.Move(userFolderPath, newUserFolderPath);
            }
            user.Fname = updateUserDto.Fname ?? user.Fname;
            user.Lname = updateUserDto.Lname ?? user.Lname;
            user.Age = updateUserDto.Age ?? user.Age;
            user.Gender = updateUserDto.Gender ?? user.Gender;
            user.Phone = updateUserDto.Phone ?? user.Phone;
            user.Address = updateUserDto.Address ?? user.Address;
            _unitRepo.UserRepo.EditUser(user);
        }

        public bool UpdatePassword(User user, PasswordDto passwordDto)
        {
            if (!VerifyPassword(user, user.Password, passwordDto.CurrentPassword) || passwordDto.NewPassword != passwordDto.ConfirmPassword)
                return false;
            user.Password = HashPassword(user, passwordDto.NewPassword);
            return true;
        }

        public async Task<bool> DeleteUserAsync(User user, string _basePath)
        {
            _unitRepo.UserRepo.DeleteUser(user);

            string userFolder = $"{user.Id}_{UnitServices.SanitizeForPath(user.Fname)}_{UnitServices.SanitizeForPath(user.Lname)}";
            string userFolderPath = Path.Combine(_basePath, userFolder);

            if (!string.IsNullOrWhiteSpace(userFolderPath) && Directory.Exists(userFolderPath))
            {
                try
                {
                    Directory.Delete(userFolderPath, recursive: true);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                    // DB record is already deleted — log the orphaned folder for manual cleanup
                    // _logger.LogWarning(ex, "Failed to delete chat folder at {Path} for chatId {ChatId}", folderPath, chatId);
                }
            }
            return true;
        }
        public UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Fname = user.Fname,
                Lname = user.Lname,
                Email = user.Email,
                Age = user.Age,
                Gender = user.Gender,
                Phone = user.Phone,
                Address = user.Address
            };
        }

        public string CreateToken(User user, IConfiguration configuration)
        {
            List<Claim> UserClaims = new List<Claim>();
            UserClaims.Add(new Claim(ClaimTypes.Name, $"{user.Fname} {user.Lname}"));
            UserClaims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            UserClaims.Add(new Claim(ClaimTypes.Email, user.Email));
            UserClaims.Add(new Claim(ClaimTypes.Role, _unitRepo.roleRepo.GetRoleName(user)!));
            UserClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

            var SignInKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)
                    );

            var signingCredentials =
                new SigningCredentials(
                    SignInKey, SecurityAlgorithms.HmacSha256
                    );

            JwtSecurityToken userToken = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                expires: DateTime.Now.AddHours(1),
                claims: UserClaims,
                signingCredentials: signingCredentials
                );

            return new JwtSecurityTokenHandler().WriteToken(userToken);
        }

        public string HashPassword(User user, string plainPassword)
        {
            PasswordHasher<User> hasher = new PasswordHasher<User>();
            return hasher.HashPassword(user, plainPassword);
        }

        public bool VerifyPassword(User user, string hashedPassword, string providedPassword)
        {
            PasswordHasher<User> hasher = new PasswordHasher<User>();
            return hasher.VerifyHashedPassword(user, hashedPassword, providedPassword)
                    is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;

        }


    }
}
