<p align="center">
<img src="Images/Logo/black-wakatime.svg" alt="WakaTime Logo" style="width: 100px; height: 100px;" />
</p>

# WakaBot

Cross Platform Discord bot for [WakaTime](https://wakatime.com) programming metrics.

### Getting Started

[Click Here](https://discord.com/oauth2/authorize?client_id=955935991087128596&permissions=274878036992&scope=bot%20applications.commands) to add to your server. Sign up to [WakaTime](https://wakatime.com) if you haven't already and use the `/user register` command to register your discord account with your WakaTime account. You can then use the commands above to view programming metrics of users in your discord server.

Signing up using OAuth is optional but allows the Wakabot to access more information about your WakaTime account such as projects and get specific time ranges. To sign up using OAuth, use the `/user register` command, set useOAuth to true and command and follow the instructions.

### Commands

#### /Rank [timerange: All Time | Last 7 Days | Last 30 Days | Last 6 Months] default All Time

Shows the rank of the top users in terms of programming time in your discord server with accompanying pie chart. You can optionally specify a time range option which will show the rank of the top users in that time range. To use the time range option, have signed up using OAuth.

<img src="Images/Rank.png" alt="Rank Command" style="height: 300px;" />

#### /toplangs

Show the top programming languages used by members of your discord server with accompanying stacked bar chart.

<img src="Images/TopLangs.png" alt="TopLangs Command" style="height: 300px;" />

#### /profile user: @user

Show the programming metrics and other interesting information about a user in your discord server with an accompanying pie chart showing the users top languages.

<img src="Images/Profile.png" alt="Profile Command" style="height: 300px;" />

#### /languagestats language: language

Show a pie chart of the top users in your discord server for the particular programming language.

<img src="Images/LanguageStats.png" alt="LanguageStats Command" style="height: 300px;" />

#### /project user: @user [time-range: All Time | Last 7 Days | Last 30 Days | Last 6 Months] default All Time

Shows the top projects of a user in your discord server with an accompanying pie chart. You can optionally specify a time range option which will show the top projects of a user in that time range. This command can only be used if you have signed up using OAuth.

<img src="Images/Project.png" alt="Project Command" style="height: 300px;" />

#### /help

Show the help menu.

### Discord Permissions

Requires the following permissions

- View Channels

- Send Messages

- Attach Files

- Read Message History

### Docker

##### Using it in Docker 

Create a Dockerfile that copies in your appsettings:

```Dockerfile
FROM izstriker/wakabot
COPY appsettings.json .
```

```sh
docker build -t waka .
docker run waka
docker run -p 5000:5000 --rm -it waka
```

You will need to migrate your database with `dotnet ef database update --context WakaBot.Core.Data.PostgreSqlContext`

### Contributing and Feedback

Contributions are welcome. Feel free to open an issue or submit a pull request. If you have any feedback or suggestions, feel free to open an issue.
