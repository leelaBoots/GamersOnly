using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;

/* Design Paradigm: We wiil create an interface any time we implment a new Service or Repository  */

namespace API.Interfaces
{
    public interface IMessageRepository
    {
        void AddMessage(Message message);
        void DeleteMessage(Message message);
        Task<Message> GetMessage(int id);
        Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams);
        Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName);

        // all this work below to update a simple property in an entity
        // adding groups to database so we can update message read in real time.
        // this is a simple solution but not scalable on systems with multiple SignalR servers. Optimal way would be to use REDIS and distribute
        // over multiple services, but that is out of the scope at this point.
        void AddGroup(Group group);
        void RemoveConnection(Connection connection);
        Task<Connection> GetConnection(string connectionId);
        Task<Group> GetMessageGroup(string groupName);
        Task<Group> GetGroupForConnection(string connectionId);
    }
}