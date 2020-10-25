# UI

## Buildings Menu Script

Handles the dynamic content and functionality of the 'Buildings' menu

### Public methods

```csharp
void populateBuildingsMenu()
```

- Populates the menu with the correct information about the buildings, their level, cost etc.
- Soon we should update this to change the image of the building based on its level

## Language Script

Handles internationalisation and sets the language of the game.

Example:

- Imagine you want to show the message "This is a new game" in the correct language, based on the device's default language
- First you need to decide a unique code for that string, for example "NEW_GAME"
- Then, to each .txt file under Assets/Resources/Languages, add a new line [code]=[string in that language]. For example in en.txt you will add "NEW_GAME=This is a new game"
- In your code, whenever you want to show that message, reference that string using its code in this way: Language.Field["NEW_GAME"]
- This will show the string in the correct language

## MapUI and UI Scripts

Handle the UI of respectively the Map and Hideout scenes

### Public methods

- To be added

## Shipyard Menu Script

Handles the dynamic content and functionality of the 'Buildings' menu

### Public methods

```csharp
void populateShipyardMenu()
```

- Populates the menu with the correct information about the ships, their level, cost etc.
- This is a work in progress as the functionality to repair a ship hasn't been added yet
