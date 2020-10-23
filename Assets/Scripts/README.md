# Scripts

## Controller Script

This holds all the local data centrally and feeds it to other scripts.

### Public methods that you should be aware of:

```csharp
User getUser()
```

- Returns the current User object, which contains all the information about the current player. To set the User object use setUser instead.

```csharp
Map getMap()
```

- Returns the current Map object, which contains all the information about map on which the current player is. To set the map use setMap instead.

```csharp
UI getUI()
```

- Returns the script that handles UI elements.

## Map Controller and Map Loader Scripts

These scripts handle user interaction with Unity components in the map. They should probably be merged into a single script and reviewed.

```csharp
void open()
```

- Opens up the map.

```csharp
void close()
```

- Closes the map and goes back to the hideout.

```csharp
void reloadMap()
```

- This is called if the user is not assigned a position on the map to fetch a new one.
