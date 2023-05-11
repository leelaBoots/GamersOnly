using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class MessageHub : Hub
    {
      private readonly IMapper _mapper;
      private readonly IUnitOfWork _uow;

      // we can inject other Hubs here, so that we can access that hub from here even if they are not connected to messageHub at the time
      private readonly IHubContext<PresenceHub> _presenceHub;
      public MessageHub(IUnitOfWork uow, IMapper mapper, IHubContext<PresenceHub> presenceHub) {
        _uow = uow;
        _presenceHub = presenceHub;
        _mapper = mapper;
      }

      public override async Task OnConnectedAsync() {
        // when we connect to SignalR hub, we send up an http request, that gives us opportunity to send something in a query string
        var httpContext = Context.GetHttpContext();
        var otherUser = httpContext.Request.Query["user"];
        var groupName = GetGroupName(Context.User.GetUsername(), otherUser);

        // this adds to the group in SignalR
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        // this adds to the Group entity in the database, we are maintaining this in 2 places
        var group = await AddToGroup(groupName);

        await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

        var messages = await _uow.MessageRepository.GetMessageThread(Context.User.GetUsername(), otherUser);

        // we do this because we moved the responsibility of SaveAllSync from the repositories to the UnitOfWork
        if (_uow.HasChanges()) await _uow.Complete();

        // this will get the messages from signalR instead of making an API call
        await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
      }

      public override async Task OnDisconnectedAsync(Exception exception) {
        var group = await RemoveFromMessageGroup();
        // this optimization will basically send this to only one other member since groups only have 2 members
        await Clients.Group(group.Name).SendAsync("UpdatedGroup");
        await base.OnDisconnectedAsync(exception);
      }

      public async Task SendMessage(CreateMessageDto createMessageDto) {
        // we dont have access to the User directly but we can get it from Context
        var username = Context.User.GetUsername();

        if (username == createMessageDto.RecipientUsername.ToLower())
          // typically we dont throw exceptions from our API by default, but since
          // we can't return bad request, just throw an exception
          throw new HubException("You cannot send messages to yourself");

        var sender = await _uow.UserRepository.GetUserByUsernameAsync(username);
        var recipient = await _uow.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

        if (recipient == null) throw new HubException("User not found");

        var message = new Message {
          Sender = sender,
          Recipient = recipient,
          SenderUsername = sender.UserName,
          RecipientUsername = recipient.UserName,
          Content = createMessageDto.Content
        };

        var groupName = GetGroupName(sender.UserName, recipient.UserName);

        var group = await _uow.MessageRepository.GetMessageGroup(groupName);

        if (group.Connections.Any(x => x.Username == recipient.UserName)) {
          // alot of work to get this to update from null to message read in real time
          message.DateRead = DateTime.UtcNow;
        } else {
          // if we made it here, then it means the recipient is not currently connected to the messageHub, so lets send a notification via presence hub
          var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
          if (connections != null) {
            await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", 
              new {username = sender.UserName, knownAs = sender.KnownAs});
          }
        }

        _uow.MessageRepository.AddMessage(message);

        if (await _uow.Complete()) {
          await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));  
          
        }
      }

      private string GetGroupName(string caller, string other) {
        // this sorts names alphabetically and returns the group name
        var stringCompare = string.CompareOrdinal(caller, other) < 0;

        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
      }

      private async Task<Group> AddToGroup(string groupName) {
        var group = await _uow.MessageRepository.GetMessageGroup(groupName);
        var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

        if (group == null) {
          group = new Group(groupName);
          // repository is database
          _uow.MessageRepository.AddGroup(group);
        }

        group.Connections.Add(connection);

        if (await _uow.Complete())  return group;

        throw new HubException("Failed to add to group");
      }

      // we are just removing from the database, then SignalR will remove them automatically when OnDisconnectedAsync is called
      private async Task<Group> RemoveFromMessageGroup() {
        var group = await _uow.MessageRepository.GetGroupForConnection(Context.ConnectionId);
        var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
        _uow.MessageRepository.RemoveConnection(connection);
        if (await _uow.Complete()) return group;

        throw new HubException("Failed to remove from group");
      }
    }
}