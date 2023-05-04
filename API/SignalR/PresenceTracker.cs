using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.SignalR
{
    public class PresenceTracker {
      private static readonly Dictionary<string, List<string>> OnlineUsers = new Dictionary<string, List<string>>();
      
      public Task<bool> UserConnected(string username, string connectionId) {
        bool isNewConnection = false;
        // use lock because this method is not thread safe, 2 users could connect at the same time
        lock(OnlineUsers) {
          if (OnlineUsers.ContainsKey(username)) {
            OnlineUsers[username].Add(connectionId);

          } else {
            OnlineUsers.Add(username, new List<string>{connectionId});
            isNewConnection = true;
          }
        }

        return Task.FromResult(isNewConnection);
      }

      public Task<bool> UserDisconnected(string username, string connectionId) {
        bool isOffline = false;
        lock(OnlineUsers) {
          if (!OnlineUsers.ContainsKey(username)) return Task.FromResult(isOffline);

          OnlineUsers[username].Remove(connectionId);

          if (OnlineUsers[username].Count == 0) {
            // if key of username is 0, we know the user is offiline
            OnlineUsers.Remove(username);
            isOffline = true;
          }

        }

        return Task.FromResult(isOffline);
      }

      public Task<string[]> GetOnlineUsers() {
        string[] onlineUsers;

        lock(OnlineUsers) {
          onlineUsers = OnlineUsers.OrderBy(k => k.Key).Select(k => k.Key).ToArray();
        }

        return Task.FromResult(onlineUsers);
      }

      // this is not a scalable option, would probably use Redis in a production
      public static Task<List<string>> GetConnectionsForUser(string username) {
        List<string> connectionIds;

        // lock just incase 2 users try to access this simultaneously
        lock (OnlineUsers) {
          connectionIds = OnlineUsers.GetValueOrDefault(username);
        }

        return Task.FromResult(connectionIds);
      }

    }
}