
Description

Welcome to my project! This is a Dating App made in ASP.NET Core framework with an Angular front end. 

Features:

1. Authenication and Authorization with ASP.NET Identity including user roles for admin and moderators
2. Photo upload and gallery feature, using external photo hosting via Cloudinary
3. Postgres Database
4. Search for user matchs against user database with sorting and pagination of results, query cache
5. Messaging system between users updated in realtime using SignalR
6. Currently active users displayed in realtime
7. User like feature


I welcome any and all contributions! Here are some ways you can get started:

    Report bugs: If you encounter any bugs, please let us know. Open up an issue and let us know the problem.
    Contribute code: If you are a developer and want to contribute, follow the instructions below to get started!
    Suggestions: If you don't want to code but have some awesome ideas, open up an issue explaining some updates or imporvements you would like to see!
    Documentation: If you see the need for some additional documentation, feel free to add some!

Instructions

    Fork this repository
    Clone the forked repository
    Add your contributions (code or documentation)
    Commit and push
    Wait for pull request to be merged

You will need to setup your own postgres or mysql database with id and password for your development environment
Example appsettings.Development.json file:

{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "ConnectionStrings" : {
    "DefaultConnection": "Data source=datingapp.db",
    "SqliteConnection": "Data source=datingapp.db",
    "PostgresConnection": "Server=localhost; Port=5432; User Id=[ID]; Password=[PASSWORD]; Database=datingapp"
    
  },
  "TokenKey": "super secret unguessable key"
}

