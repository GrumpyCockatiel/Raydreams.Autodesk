# Autodesk BIM360 Client

WIP Project to a BIM 360 Command Line Tool (exe)
Not ready for public consumption.

## Getting started

You need to add a `appsettings.json` file to your project that looks like the below

The App Client Secret is **Super Senstive**! DO NOT share it with ANYONE! DO NOT hard code it into any user client (desktop, mobile, web client side JS)

Using a 2-Leg token - anyone with your client ID and client secret can pretty much mess-up your account.

```
{
    "AppConfig": {
        "Environment": "DEV"
    },
    "ForgeAppClient": {
        "ClientID": "<Forge App Client ID>",
        "Secret": "<Forge App Client Secret>",
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

## Forge App Client

You will need to create a BIM360 Account with Autodesk and create your own Forge App Client and then give your client rights to some BIM360 Account.

Sadly, Autodesk only gives you a 3 month trial before you have to pay which is not realistic for independent developers, so you will probably need to go through your employer.
