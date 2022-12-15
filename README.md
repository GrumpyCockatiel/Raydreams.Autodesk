# Autodesk BIM360 Client

WIP Project to a BIM 360 Command Line Tool (exe)

## Getting started

You need to add a `appsettings.json` file to your project that looks like the below

```
{
    "AppConfig": {
        "Environment": "DEV"
    },
    "ForgeAppClient": {
        "ClientID": "<Forge App Client ID>",
        "Secret": "<Forage App Secret>",
        "CallbackURL": "http://localhost:50001/callback",
        "PrimaryHubID": "<A Default Hub ID>"
    },
    "Logging": {
        "Console": {
            "LogLevel": {
                "Default": "Information"
            }
        },
        "LogLevel": {
            "Default": "Warning"
        }
    }
}
