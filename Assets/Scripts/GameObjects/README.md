# GameObjects

## Bounty Script

Handles the logic behind the user's 'bounty' (how strong the user is)

### Public methods

No public methods.

## Buildings Script

Handles the logic behind the spawning and creation of buildings

### Public methods

```csharp
void main(string buildingName)
```

- This builds the building of which the name is provided as a parameter

```csharp
void populateVillage(string buildingsJson)
```

- This populates the village with the buildings provided (as a string in json format)

```csharp
void populateFloatingObjects()
```

- This spawns the floating objects in the hideout's bay

```csharp
void populateShips()
```

- This spawns the ships owned by the player in the hideout scene

## Floating Objects Script

Handles the logic behind the spawning and floating objects (tiny collectables that appear in the sea around the hideout and provide the user with some extra resources)

### Public methods

```csharp
void spawn(FloatingObject obj)
```

- This shows a particular FloatingObject in the hideout scene

## Ships Script

Handles the logic behind the construction, upgrade, repair and (part of the) navigation for the ships in the game

- This is a work in progress and it's not yet fully implemented

## User Resources Script

Handles the logic behind the collection and display of user resources

### Public methods

```csharp
void showTooltip(string buildingName)
```

- Shows the clickable tooltip above a building to collect the resources it produced
