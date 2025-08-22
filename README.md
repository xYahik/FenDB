# FenDB (FenDiscordBot)

FenDB is a Discord bot designed to help manage servers, provide fun interactive commands for users, and automatically respond to frequently asked questions by detecting relevant text.

## Features

- Server management tools (in development)
  - Warn Systems (implemented)
  - Purge Command (implemented)
  - Logs (implemented, requires further customization)
- Fun and engaging commands for users (in development)
  - WebScraper for random image from websites (Planned)
  - Leveling/Activity System (Planned)
- FAQ system that automatically answers common questions (implemented, but requires further customization)

## Current Status

FenDB is still in the early stages of development.  
So far, a basic command registration and handling system has been implemented, along with a logging system and an intelligent FAQ feature that allows the bot to decide automatically whether to respond for messages.

The project still requires work on automation, optimizations, and additional features.

## Technologies

- C# (.NET)
- Discord API
- Python
- PostgreSQL

## Requirements

- PostgreSQL >= 14
  - In PostgreSQL, create a new user `fendb` and database `fendb`, and then grant that user access to that database.
- Python >= 3.10

## Getting Started

To get FenDB running, follow these simple steps:

1.**Ensure you have the required software installed**

2.**Configure the bot** (GuildId is configurable for now, until multi-server support is implemented):  
 Edit `Config/Config.cs` to set your bot token, App ID, and server (guild) ID:

```csharp
public static class CONFIG
{
    public static string Token => "YOUR_BOT_TOKEN";
    public static string AppId => "YOUR_APP_ID";
    public static string GuildId => "YOUR_SERVER_ID"; // temporary, until multi-server support is added
}
```

3.**Run the program:**

```bash
  dotnet run
```

- The program will automatically set up a local Python service with its own virtual environment.

- The necessary SQL tables will be created automatically.

## TODO

- Improve usage of the reflection system. Currently some part of the code still uses static parameter names.
- Managing servers, which will allow bot be used on multiple servers
- Improving bot response with slash command
- WebScraper for random image from websites
- Leveling/Activity System

## Feedback & Ideas

I'm open to suggestions and ideas on how to improve FenDB.  
If you have any thoughts or feature requests, feel free to reach out or open an issue.

## License

This project is licensed under the **Apache License 2.0**.  
You are free to use, modify, and distribute the code.  
If you modify the code, please keep a note about the original author (me) to give proper credit.

For more details, see the [LICENSE](LICENSE) file.
