# Piratoon

Piratoon is an indie strategy mobile game. Users play as pirtes and their main goal is to become more and more powerful. They can do so by improving their pirate hideout, plundering other players' hideouts or conquering neighbouring cities.

## Develop the game

The game is being developed in Unity (v2019.3), using the C# language (more documentation in the Assets/Scripts folder).

The game makes use of a Microsoft PlayFab backend, which mainly handles authentication, data storage and multiplayer. Please find the PlayFab documentation [here](https://docs.microsoft.com/en-us/gaming/playfab/).

## Build the game

You can build the game for both Android and iOS (not tested yet) using Unity. Before performing a build, remember to check that:

1. 'Check Headquarters' on the Buildings script is enabled
2. 'Remember Login Details' on the ControllerScript is enabled

### Meta files

Please ingore the .meta files in this repository. They are needed by Unity to avoid importing every asset every time.
