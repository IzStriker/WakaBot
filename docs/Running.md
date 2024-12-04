# Setup

## Dependencies
For arch you will need to install the following packages:
- ttf-ms-fonts

Create an `appsettings.json` file inside `WakaBot.Web` directory with the following content (replacing the relevant values):

```json {"id":"01J6WR4F9CN7WSSW09B59A3YMB"}
{
    "databaseProvider": "[sqlite|mysql|postgresql]",
    "ConnectionStrings": {
        "PostgreSql": "[if using postgresql]",
        "MySql": "[if using mysql]",
        "Sqlite": "[if using sqlite]"
    },
    "token": "your discord bot token",
    "OAuth": {
        "Name": "WakaTime",
        "ClientId": "[your wakatime client id]",
        "ClientSecret": "[your wakatime client secret]",
        "RedirectUrl": "[your redirect url]",
        "AuthorizeUrl": "https://wakatime.com/oauth/authorize",
        "TokenUrl": "https://wakatime.com/oauth/token"
    }
}
```

## Setting up database

Run a database with on of the supported providers (sqlite, mysql, postgresql) and run the following command to update the database schema:

```bash {"id":"01J6WR4F9CN7WSSW09B8J4TY37"}
cd WakaBot.Core
# Choose the context you want to use depending on your chosen database provider
dotnet ef database update --context WakaBot.Core.Data.PostgreSqlContext
# or
dotnet ef database update --context WakaBot.Core.Data.MySqlContext
# or
dotnet ef database update --context WakaBot.Core.Data.SqliteContext
```

## Running the bot

```bash
dotnet run --project WakaBot.Web
```