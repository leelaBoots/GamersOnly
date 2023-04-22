// this will represent the join table between our users and roles
using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
    public class AppUserRole : IdentityUserRole<int>
    {
      public AppUser User {get; set;}

      public AppRole Role {get; set;}

    }
}