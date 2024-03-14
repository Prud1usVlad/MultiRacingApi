using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiRacingApi.Controllers.ViewModels;
using MultiRacingApi.Models;
using MultiRacingApi.Services;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using AspNetCoreRateLimit;


namespace MultiRacingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private UsersService _usersService;
        private JwtService _jwtService;

        public AuthController(UsersService usersService, JwtService jwtService)
        {
            _usersService = usersService;
            _jwtService = jwtService;
        }

        [AllowAnonymous]
        [HttpPost("/login")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            Console.WriteLine($"login {login.Username}");

            try
            {
                var user = await AuthenticateUser(login);

                if (user != null)
                {
                    return Ok(await _jwtService.CreateTokenAsync(user));
                }

                else return Unauthorized();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost("/register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel register)
        {
            try
            {
                var user = await AuthenticateUser(register);

                if (user != null)
                {
                    return BadRequest($"User with username {register.Username} already exists");
                }
                else
                {
                    var passData = HashPassword(register.Password);

                    user = new User()
                    {
                        Name = register.Username,
                        PasswordHash = passData.hash,
                        PasswordSalt = passData.salt,
                        Results = new(),
                    };

                    await _usersService.Add(user);

                    return Ok(await _jwtService.CreateTokenAsync(user));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("/change/password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            try
            {
                var user = await _usersService.GetById(model.UserId);

                if (user == null)
                {
                    return BadRequest($"No user with ID {model.UserId}");
                }
                else
                {
                    var passData = HashPassword(model.NewPassword);

                    user.PasswordHash = passData.hash;
                    user.PasswordSalt = passData.salt;

                    await _usersService.Update(user.Id, user);

                    return Ok("Password changed");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("/change/name")]
        public async Task<IActionResult> ChangeName([FromBody] ChangeNameModel model)
        {
            try
            {
                var user = await _usersService.GetById(model.UserId);
                var userByName = await _usersService.GetByName(model.NewName);

                if (user == null)
                {
                    return BadRequest($"No user with ID {model.UserId}");
                }
                else if (userByName != null)
                {
                    return BadRequest($"Name {model.NewName} has already been taken");
                }
                else
                {
                    user.Name = model.NewName;

                    await _usersService.Update(user.Id, user);

                    return Ok("Name changed");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task<User> AuthenticateUser(LoginModel login)
        {
            var user = await _usersService.GetByName(login.Username);

            if (user != null && CheckPassword(user, login))
            {
                return user;
            }
            else
            {
                return null;
            }
        }

        private (string hash, string salt) HashPassword(string password, string saltStr = "")
        {
            // generate a 128-bit salt using a cryptographically strong random sequence of nonzero values
            byte[] salt = new byte[128 / 8];
            
            if (saltStr == "")
            {
                using (var rngCsp = new RNGCryptoServiceProvider())
                {
                    rngCsp.GetNonZeroBytes(salt);
                }
            }
            else
            {
                salt = Convert.FromBase64String(saltStr);
            }

            // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
            string hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            return (hash, Convert.ToBase64String(salt));
        }

        private bool CheckPassword(User user, LoginModel login)
        {
            var newHash = HashPassword(login.Password, user.PasswordSalt).hash;

            return user.PasswordHash == newHash;
        }
    }
}
