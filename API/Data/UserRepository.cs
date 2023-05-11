using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        // inject IMapper into the repository, so we can use it to Project onto the MemberDto
        public UserRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<MemberDto> GetMemberAsync(string username)
        {
            return await _context.Users
                .Where(x => x.UserName == username)
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            // AsNoTracking turns off tracking in EF because we only ever read this, makes it more effiecient
            // Old way, now we need to filter BEFORE we project to the MemberDto
            /*var query = _context.Users
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .AsQueryable();*/

            // we dont want to return the users profile in the results, and also filter by gender
            var query = _context.Users.AsQueryable();
            query = query.Where(u => u.UserName != userParams.CurrentUsername); 
            query = query.Where(u => u.Gender == userParams.Gender);

            // calculate the date range via the min and max age params
            var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1));
            var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));

            query = query.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);

            query = userParams.OrderBy switch {
              "created" => query.OrderByDescending(u => u.Created),
              _ => query.OrderByDescending(u => u.LastActive)
            };

            // we no longer execute the query here, we pass this to CreateAsync and it executes query there.
            return await PagedList<MemberDto>.CreateAsync(query.ProjectTo<MemberDto>(_mapper
              .ConfigurationProvider).AsNoTracking(),
              userParams.PageNumber, userParams.PageSize);
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            // returns a list of users that match the given username
            return await _context.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == username);
        }

        public async Task<string> GetUserGender(string username)
        {
          // this gives us a single property from the database, instead of returning entire user
            return await _context.Users.Where(x => x.UserName == username).Select(x => x.Gender).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            // must explicity include p.Photos in order to return a sub set
            return await _context.Users
                .Include(p => p.Photos)
                .ToListAsync();
        }

        public void Update(AppUser user)
        {
            // updates the state of the user to modified
            _context.Entry(user).State = EntityState.Modified;
        }
    }
}