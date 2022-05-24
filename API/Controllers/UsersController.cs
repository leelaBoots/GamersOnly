using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        // make use of our UserRepository class
        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            _mapper = mapper;
            _userRepository = userRepository;
        }

        // api/users/
        [HttpGet]
        /* IEnumerable is a simple list instead of List type that has may methods not needed */
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var users = await _userRepository.GetMembersAsync(); // await gets results of a Task

            return Ok(users);
        }

        // api/user/bob
        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await _userRepository.GetMemberAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto) {
            // find the claim that matches the given name identifier
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // now use this name to get the user from the user repository
            var user = await _userRepository.GetUserByUsernameAsync(username);
            // AutoMapper: this saves us from manually mapping out dto to our user object. Map has tons of overloads for different types of objects
            _mapper.Map(memberUpdateDto, user);
            // this flags the user as updated, to prevent us getting an exception when coming back from updating our database
            _userRepository.Update(user);

            // we dont need to send any content back for a put request
            if (await _userRepository.SaveAllAsync()) return NoContent();

            // default if SaveAllAsync() fails
            return BadRequest("Failed to update user");
             
        }
    }
}