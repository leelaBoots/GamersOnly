using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface IUserRepository
    {
        // allows user to update their profile
        void Update(AppUser user);

        Task<IEnumerable<AppUser>> GetUsersAsync();

        Task<AppUser> GetUserByIdAsync(int id);

        Task<AppUser> GetUserByUsernameAsync(string username);

        // instead of returning IEnumerable, we will now return a PagedList that we created
        Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams);
        
        Task<MemberDto> GetMemberAsync(string username);
        
        Task<string> GetUserGender(string username);

    }
}