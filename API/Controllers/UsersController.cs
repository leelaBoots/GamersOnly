using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        // make use of our UserRepository class
        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
        {
          _photoService = photoService;
          _mapper = mapper;
          _userRepository = userRepository;
        }

        // api/users/
        [HttpGet]
        /* IEnumerable is a simple list instead of List type that has may methods not needed.
        because we are in a API controller, .NET should be smart enough to see we are sending query string parameters,
        and it should be able to match them up in userParams */
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
            userParams.CurrentUsername = user.UserName;

            // if gender pref is not passed in params, then set it to opposite gender by default
            if (string.IsNullOrEmpty(userParams.Gender)) {
              userParams.Gender = user.Gender == "male" ? "female" : "male";
            }

            var users = await _userRepository.GetMembersAsync(userParams); // await gets results of a Task

            // we always have access to our Response inside the controllers 
            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);
        }

        // api/user/bob
        // we need to give a route a name "GetUser" so that we can use it in the CreatedAtRoute 201 response
        // Post requests to update a database should always return a 201 response when adding a new resource to a database
        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await _userRepository.GetMemberAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto) {
            // get the user from the user repository
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
            // AutoMapper: this saves us from manually mapping out dto to our user object. Map has tons of overloads for different types of objects
            _mapper.Map(memberUpdateDto, user);
            // this flags the user as updated, to prevent us getting an exception when coming back from updating our database
            _userRepository.Update(user);

            // we dont need to send any content back for a put request
            if (await _userRepository.SaveAllAsync()) return NoContent();

            // default if SaveAllAsync() fails
            return BadRequest("Failed to update user");
             
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file) {
          // getting user includes the photos
          var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

          var result = await _photoService.AddPhotoAsync(file);

          if (result.Error != null) { return BadRequest(result.Error.Message); }

          // create a new photo to add
          var photo = new Photo {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
          };

          if (user.Photos.Count == 0) {
            photo.IsMain = true;
          }

          user.Photos.Add(photo);

          if (await _userRepository.SaveAllAsync()) {
            // map our photo into a PhotoDto for return purposes
            // createdAtRoute overload takes a Route name string, the username as an object, and an object (the photo in this case)
            // we have to do this but the GetUser route requires a parameter in this case it a username
            // this is all so we can return a 201 Created request instead of 200 OK
            return CreatedAtRoute("GetUser", new {username = user.UserName},_mapper.Map<PhotoDto>(photo));
          }

          return BadRequest("Problem adding photo");

        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
          // when we get the username from the token, its been authenticated to this method so we can trust that the uses name is correct
          var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

          // not async, we already  have user in memory. no need to hit database. look for photo with matching ID
          var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

          if(photo.IsMain) return BadRequest("This is already your main photo");

          var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
          if (currentMain != null) { currentMain.IsMain = false; }
          photo.IsMain = true;

          // this returns a good status 204 with no message/content
          if (await _userRepository.SaveAllAsync()) { return NoContent(); }

          return BadRequest("Failed to set main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        // when we delete a resource, we dont need to send anything back to the client 
        public async Task<ActionResult> DeletePhoto(int photoId) {
          var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

          var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

          if (photo == null) return NotFound();

          if (photo.IsMain) return BadRequest("You cannot delete your main photo");

          // only photos with PublicId need to be deleted from cloudinary
          if (photo.PublicId != null) {
            var result = await _photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null) return BadRequest(result.Error.Message);
          }

          user.Photos.Remove(photo);

          if (await _userRepository.SaveAllAsync()) return Ok();

          return BadRequest("Failed to delete the photo");

        }
    }
}