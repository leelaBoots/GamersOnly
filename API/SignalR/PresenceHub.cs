using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    // this will not use HTTP request, so it wont have access to HTTP headers.
    // we will use websockets for this communication
    public class PresenceHub : Hub
    {
    private readonly PresenceTracker _tracker;
      public PresenceHub(PresenceTracker tracker) {
        _tracker = tracker;
        
      }

      public override async Task OnConnectedAsync() {
        var isNewConnection = await _tracker.UserConnected(Context.User.GetUsername(), Context.ConnectionId);

        if (isNewConnection) {
          await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());
        }

        var currentUsers = await _tracker.GetOnlineUsers();
        await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
      }


      public override async Task OnDisconnectedAsync(Exception exception) {
        var isOffline = await _tracker.UserDisconnected(Context.User.GetUsername(), Context.ConnectionId);

        if (isOffline) {
          await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());
        }

        // not effiecient to send to all clients here. we will do this another way
        //var currentUsers = await _tracker.GetOnlineUsers();
        //await Clients.All.SendAsync("GetOnlineUsers", currentUsers);

        await base.OnDisconnectedAsync(exception);
      }
        
    }
}