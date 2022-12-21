
using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    // uses a route to access the controller

    public class LikesController : BaseApiController
    {
      public IUserRepository _userRepository { get; set; }
      public ILikesRepository _likesRepository { get; set; }
      public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
      {
        _likesRepository = likesRepository;
        _userRepository = userRepository;
        
      }

      // what we are really doing here is just updating our Join table for likes
      [HttpPost("{username}")]
      public async Task<ActionResult> AddLike(string username) {
        var sourceUserId = User.GetUserId();
        var likedUser = await _userRepository.GetUserByUsernameAsync(username);
        var SourceUser = await _likesRepository.GetUserWithLikes(sourceUserId);
        
        if (likedUser == null) return NotFound();

        if (SourceUser.UserName == username) return BadRequest("You cannot like yourself");

        var userLike = await _likesRepository.GetUserLike(sourceUserId, likedUser.Id);

        if (userLike != null) return BadRequest("You already like this user");

        userLike = new UserLike
        {
          SourceUserId = sourceUserId,
          TargetUserId = likedUser.Id
        };
        
        // this creates that entity in the userLikes table
        SourceUser.LikedUsers.Add(userLike);

        if (await _userRepository.SaveAllAsync()) return Ok();

        return BadRequest("Failed to like user");
      }

      [HttpGet]
      public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams) {
        likesParams.UserId = User.GetUserId();

        var users = await _likesRepository.GetUserLikes(likesParams);

        //Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));

        Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

        return Ok(users);
      }
    }
}