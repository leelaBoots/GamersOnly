namespace API.Entities
{
    public class Connection
    {
      // We need this empty constructor with no params for Entity Framework when it creates the schema for out database
      public Connection()
      {
      }

    public Connection(string connectionId, string username)
      {
        ConnectionId = connectionId;
        Username = username;
      }

      public string ConnectionId { get; set; }

      public string Username { get; set; }
        
    }
}