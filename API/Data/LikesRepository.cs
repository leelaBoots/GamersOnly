using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;

namespace API.Data
{
  public class LikesRepository : ILikesRepository
  {
    private readonly DataContext _context;
    
    public LikesRepository(DataContext context)
    {
      _context = context;
      
    }
    public async Task<UserLike> GetUserLike(int sourceUserId, int targetUserId)
    {
      return await _context.Likes.FindAsync(sourceUserId, targetUserId);
    }

    public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
    {
      // these are just queryables which means they have not yet been executed
      var users = _context.Users.OrderBy(u => u.UserName).AsQueryable();
      var likes = _context.Likes.AsQueryable();

      // check the predicate for liked or likes
      if (likesParams.Predicate == "liked") {
        // now we execute the query
        // this will filter out the users based on whats inside the likes list
        likes = likes.Where(like => like.SourceUserId == likesParams.UserId);
        users = likes.Select(like => like.TargetUser);
      }

      if (likesParams.Predicate == "likedBy") {
        // now we execute the query
        // this will filter out the users based on whats inside the likes list
        likes = likes.Where(like => like.TargetUserId == likesParams.UserId);
        users = likes.Select(like => like.SourceUser);
      }

      
      var likedUsers = users.Select(user => new LikeDto 
      {
        Username = user.UserName,
        KnownAs = user.KnownAs,
        Age = user.DateOfBirth.CalculateAge(),
        PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain).Url,
        City = user.City,
        Id = user.Id
      });

      return await PagedList<LikeDto>.CreateAsync(likedUsers, likesParams.PageNumber, likesParams.PageSize);

    }

    public async Task<AppUser> GetUserWithLikes(int userId)
    {
      return await _context.Users
        .Include(x => x.LikedUsers)
        .FirstOrDefaultAsync(x => x.Id == userId);
    }
  }
}