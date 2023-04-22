using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto) // this class does not know where username and password are coming from
        // it could be in the body, or the url. the [ApiController] attribute will figure this out
        {
            if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");

            // this maps from registerDto to AppUser
            var user = _mapper.Map<AppUser>(registerDto);

            // no longer creating our own hash. handled by AspNetCore.Identity
            //using var hmac = new HMACSHA512(); // this class has a disposable method, because we want to dispose of it when we are finished using it
            //user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
            //user.PasswordSalt = hmac.Key;

            user.UserName = registerDto.Username.ToLower();

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if(!result.Succeeded) return BadRequest(result.Errors);

            var roleResult = await _userManager.AddToRoleAsync(user, "Member");

            if (!roleResult.Succeeded) return BadRequest(result.Errors);

            return new UserDto {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender
                // we dont need photos because user does not have photos yet
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            // first thing we need to do is get the user from the database
            var user = await _userManager.Users
              .Include(p => p.Photos)
              .SingleOrDefaultAsync(x => x.UserName == loginDto.Username);

            if (user == null) return Unauthorized("Invalid Username");

            // so here we are using the AspNetCore.Identity to verify the password in the store
            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!result) return Unauthorized("Invalid Password");

            // no longer creating our own hash. handled by AspNetCore.Identity
            /*using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password");
            }*/

            return new UserDto {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
                // make main photo optional? because they may not have set one yet
            };
        }

        private async Task<bool> UserExists(string username)
        {
            return await _userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}