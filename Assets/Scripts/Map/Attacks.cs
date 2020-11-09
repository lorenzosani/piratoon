using System;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using UnityEngine;

public class Attacks : MonoBehaviour {
  static int SHIPS_MAX_NUMBER = 3;

  public GameObject[] shipsPrefabs;
  string selectedTarget = null;

  ControllerScript controller;
  MapController mapController;
  MapUI ui;

  AIPath[] paths = new AIPath[SHIPS_MAX_NUMBER];
  bool[] destinationReached = new bool[SHIPS_MAX_NUMBER];
  AIDestinationSetter[] destinations = new AIDestinationSetter[SHIPS_MAX_NUMBER];
  GameObject[] shipsSpawned = new GameObject[SHIPS_MAX_NUMBER];

  void Update() {
    detectDirection();
    detectClick();
    for (int i = 0; i < SHIPS_MAX_NUMBER; i++) {
      if (paths[i] != null && paths[i].reachedEndOfPath && !destinationReached[i]) {
        controller.getUser().getVillage().getShip(i).finishJourney();
        destinationReached[i] = true;
      }
    }
  }

  void Start() {
    controller = GameObject.Find("GameController").GetComponent<ControllerScript>();
    mapController = GetComponent<MapController>();
    ui = mapController.getUI();
  }

  //*****************************************************************
  // PUBLIC: call this when you want to start an attack
  //*****************************************************************
  public void startAttack() {
    Ship[] shipsOwned = controller.getUser().getVillage().getShips();
    if (shipsOwned.Count(s => s != null) > 1) {
      // If the user has more than one ship, let them pick one for the attack
      ui.showHideoutPopup(false);
      ui.showShipPicker(shipsOwned, true);
    } else {
      // Otherwise just set that ship as the attacking ship
      foreach (Ship s in shipsOwned) {
        if (s != null) {
          ui.showHideoutPopup(false);
          spawnShip(s.getSlot());
          break;
        }
      }
    }
  }

  //*****************************************************************
  // DETECT general clicks or taps on the map
  //*****************************************************************
  void detectClick() {
    if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended) {
      onHideoutClick(Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position));
    } else if (Input.GetMouseButtonUp(0)) {
      onHideoutClick(Camera.main.ScreenToWorldPoint(Input.mousePosition));
    }
  }

  //*****************************************************************
  // CHANGE the ship orientation based on its navigation direction
  //*****************************************************************
  void detectDirection() {
    // TODO: Improve to have 8 directions
    // TODO: for each direction show a different sprite
    for (int i = 0; i < SHIPS_MAX_NUMBER; i++) {
      if (shipsSpawned[i] == null)continue;
      if (paths[i].desiredVelocity.x >= 0.01f) {
        shipsSpawned[i].transform.localScale = new Vector3(-1f, 1f, 1f);
      } else if (paths[i].desiredVelocity.x <= -0.01f) {
        shipsSpawned[i].transform.localScale = new Vector3(1f, 1f, 1f);
      }
    }
  }

  //*****************************************************************
  // HANDLE clicks or taps on hideouts
  //*****************************************************************
  void onHideoutClick(Vector3 position) {
    Vector2 position2d = new Vector2(position.x, position.y);
    RaycastHit2D raycastHit = Physics2D.Raycast(position2d, Vector2.zero);
    if (raycastHit) {
      string hideoutName = raycastHit.collider.name;
      // Check if the click is on a hideout
      if (hideoutName.Split('_')[0] == "hideout") {
        if (hideoutName.Split('_')[2] == API.playFabId) { // If the hideout is the player's one, open it
          mapController.close();
        } else { // Otherwise show information about it
          selectedTarget = hideoutName;
          ui.showHideoutPopup();
          API.GetUserData(
            new List<string> {
              "User",
              "Village"
            },
            result => mapController.showHideoutInfo(
              result.Data.ContainsKey("User") ? result.Data["User"].Value : null,
              result.Data.ContainsKey("Village") ? result.Data["Village"].Value : null),
            hideoutName.Split('_')[2]
          );
        }
      } else if (hideoutName.Split('_')[0] == "city") {
        onCityClick(position);
      }
    }
  }

  //*****************************************************************
  // HANDLE clicks or taps on hideouts
  //*****************************************************************
  void onCityClick(Vector3 position) {
    Vector2 position2d = new Vector2(position.x, position.y);
    RaycastHit2D raycastHit = Physics2D.Raycast(position2d, Vector2.zero);
    if (raycastHit) {
      string cityName = raycastHit.collider.name;
      // Check if the click is on a city
      if (cityName.Split('_')[0] == "city") {
        selectedTarget = cityName;
        GetComponent<Attacks>().startAttack();
      }
    }
  }

  //*****************************************************************
  // START the navigation of a ship by setting the target
  //*****************************************************************
  void startNavigation(int ship) {
    destinations[ship].target = GameObject.Find(selectedTarget).transform;
    destinationReached[ship] = false;
  }

  //*****************************************************************
  // GET the selected target towards which a ship will start navigation
  //*****************************************************************
  Transform getNavigationTarget() {
    return GameObject.Find(selectedTarget).transform;
  }

  //*****************************************************************
  // SET which ship is going to attack and start the navigation
  //*****************************************************************
  public void spawnShip(int shipNumber) {
    Ship ship = controller.getUser().getVillage().getShip(shipNumber);
    // Create ShipJourney object and add it to the ship (+store on playfab)
    ShipJourney journey = new ShipJourney(
      ship.getCurrentPosition(),
      getNavigationTarget().position,
      DateTime.Now
    );
    ship.startJourney(journey);
    // Spawn that ship onto the map only if it's not been spawn already
    if (shipsSpawned[shipNumber] == null) {
      shipsSpawned[shipNumber] = (GameObject)Instantiate(
        shipsPrefabs[ship.getLevel() - 1],
        ship.getCurrentPosition(),
        Quaternion.identity
      );
      for (int i = 0; i < SHIPS_MAX_NUMBER; i++) {
        if (shipsSpawned[i] != null) {
          paths[i] = shipsSpawned[i].GetComponent<AIPath>();
          destinations[i] = shipsSpawned[i].GetComponent<AIDestinationSetter>();
        }
      }
    }
    // The ship starts the navigation towards the arrival of the ShipJourney
    startNavigation(shipNumber);
  }
}