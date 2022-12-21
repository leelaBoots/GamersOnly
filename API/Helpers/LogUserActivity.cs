using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace API.Helpers
{
  public class LogUserActivity : IAsyncActionFilter
  {
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
      // this action will happen after request, can be done before or during as well
      var resultContext = await next();

      // might be unessary because we only allow this request after a user is authenticated anyway
      if (!resultContext.HttpContext.User.Identity.IsAuthenticated) return;

      var userId = resultContext.HttpContext.User.GetUserId();

      // this has one jobm to update one property LastActive
      var repo = resultContext.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
      var user = await repo.GetUserByIdAsync(userId);
      user.LastActive = DateTime.UtcNow;
      await repo.SaveAllAsync(); // update the database with this line

      
    }
  }
}