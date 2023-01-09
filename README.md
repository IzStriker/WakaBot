# WakaBot

Cross Platform Discord bot for [WakaTime](https://wakatime.com) programming metrics. Currently can only run in a single discord server at a time.
[Click Here](https://discord.com/oauth2/authorize?client_id=955935991087128596&permissions=274878036992&scope=bot%20applications.commands) To add to your server.
## Compilation & Execution

### Requirements

You must have .NET 6 SDK to run this discord bot. Check with following command.

```
dotnet --info
```

### Compilation

```bash
# Download bot from discord
mkdir tmp && cd tmp
git clone git@github.com:IzStriker/WakaBot.git
# Build Project
cd WakaBot/WakaBot
# Try to choose location outside of pulled repository
dotnet publish -o path/to/output/directory/WakaBot
# Remove repository if you wish
cd ../.. && rm WakaBot/ -rf
```

### Setup

In your WakaBot directory you need to create an `appsettings.json` and fill it with the following information.

```json
{
  "token": "required: token for discord api.",
  "guildId": "required: id of discord channel (used when testing).",
  "dBPath": "optional: alter path to database",
  "dBFileName": "optional: alter db file name.",
  "colourURL": "optional: alter language colours url (must follow github colours format).",
  "maxUsersPerPage": "optional: how many users are listed on each page of rank.",
  "alwaysCacheUsers": "optional: cache user stats in memory to improve performance.",
  "runWebServer": "optional: run very basic http API, will respond with `active`"
}
```

Next you need to create the database for the bot by applying it's migrations.

```
dotnet WakaBot.dll --ef-migrate
```

### Discord Portal Setup

Under `Privileged Gateway` Intents enable `PRESENCE INTENT` and `SERVER MEMBERS INTENT`

### Discord Permissions

Requires the following permissions

- View Channels
- Send Messages
- Attach Files
- Read Message History

### Execution

Run the bot using the following command in the folder the bot is published in.

```bash
dotnet WakaBot.dll
```
