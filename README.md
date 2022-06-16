# WakaBot

Cross Platform Discord bot for [WakaTime](https://wakatime.com) programming metrics. Currently can only run in a single discord server at a time.

## Features

| Command          | Description                                  |
| ---------------- | -------------------------------------------- |
| `wakaprofile`    | Get detailed information about WakaBot user. |
| `wakarank`       | Get ranking of WakaBot registered users.     |
| `wakausers`      | Get list of registered WakaBot users.        |
| `wakaregister`   | Register new WakaBot user.                   |
| `wakaderegister` | Deregister WakaBot user                      |
| `wakaping`       | Test WakaBot is up and running.              |

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
  "token": "required",
  "guildId": "required",
  "dBPath": "optional",
  "dBFileName": "optional",
  "colourURL": "optional",
  "maxUsersPerPage": "optional"
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

### Execution

Run the bot using the following command in the folder the bot is published in.

```bash
dotnet WakaBot.dll
```

## Issues

Could break or be slow if too many users are registered.
