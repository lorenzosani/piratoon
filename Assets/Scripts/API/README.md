# API

## API Script

Handles most communications between the frontend and the Microsoft PlayFab backend.

### Public methods

```csharp
void RegisterUser(string username, string email, string password, Action<string,string> callback)
```

- This registers a new user using the provided details.
- On completion this calls the callback function with the string "SUCCESS" in case of successful registration, or an error message otherwise.

```csharp
void UsernameLogin(string username, string password, Action<string> callback)
```

- This logins the user with their username and password.
- On completion this calls the callback function with the string "SUCCESS" in case of successful registration, or an error message otherwise.

```csharp
void EmailLogin(string email, string password, Action<string> callback = null)
```

- This logins the user with their email and password.
- On completion this calls the callback function with the string "SUCCESS" in case of successful registration, or an error message otherwise.

```csharp
void NewPlayerLogin(string userId = locallyStoredId)
```

- This logins the user that hasn't yet registered (!) using a locally stored id, which you can optionally pass in as a parameter.
- This is not safe and we should always encourage users to register.

```csharp
void SetUserData(string[] dataTypes)
```

- This updates the current user's data on the server to match the locally stored data.
- The parameter dataTypes defines which type of data is to be updated. To this day, possible values are 'User', 'Village', 'Buildings' and they correspond to the locally stored User, Village and array of Building objects.

```csharp
void GetUserData(List<string> dataTypes, Action<GetUserDataResult> callback, string userId = null)
```

- This retrieves the user's data from the server.
- The parameter dataTypes defines which type of data is to be updated. To this day, possible values are 'User', 'Village', 'Buildings' and they correspond to the locally stored User, Village and array of Building objects.
- The callback is called on completion with either a PlayFab GetUserDataResult object (success) or null value (error).
- The userId parameter can optionally be added if we want to retrieve data for a user different from the active one, which is otherwise the default option.

```csharp
void GetMapData(string mapId)
```

- This retrieves data about the map of which the id is provided

```csharp
void UpdateBounty(int value)
```

- This updates the current user's bounty value on the server. This is important as, for the moment, bounty is the main metric for a player's strength and success.

```csharp
void GetLeaderboard(Action<List<PlayerLeaderboardEntry>, string> callback)
```

- This retrieves the players' leaderboard from the server.
- On completion it calls the callback function twice, returning once the local ond once the absolute leaderboards. The callback function must take the retrieved PlayFab PlayerLeaderboardEntry object and a string that indicates the type of leaderboard ("local", "absolute")

```csharp
void SendPasswordRecoveryEmail(string emailAddress)
```

- This sends a password recovery email to the given email address. If the address is not valid it shows an error message

Getters and setters:

```csharp
string GetStoredPlayerId(), bool IsRegistered(), void StorePlayerId(string id), void StoreRegistered(bool registered), void StoreUsername(string username), string GetUsername()
```

## Authentication Script

Handles some of the logic behind user authentication, by providing a level of abstraction above PlayFab functionalities.

- This script provides a higher level interface on top of some of the methods in the API script above. The methods used here have been thought and tested only for SOME SPECIFIC CASES. They could be tried as shortcut in some circumstances, but they might not work in contexts different from the ones where they're curerntly used.

## Mapmaking Script

Generates new maps and assign players to a position on a map

### Public methods

```csharp
void Start()
```

- This starts the process of mapmaking for the current player. If successful, this assigns the player to a particular map and position on that map. In case of error this prints an error message to the console and fails silently.
- This does not handle race conditions (e.g. two players registering at the same moment) and it is thus not a scalable solution for mapmaking. It will need revising with a more robust solution.

```csharp
void Stop()
```

- This interrupts any ongoing mapmaking process.
