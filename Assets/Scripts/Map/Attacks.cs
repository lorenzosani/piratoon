using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pathfinding;
using UnityEngine;
using UnityEngine.UI;

public class Attacks : MonoBehaviour {
  static int SHIPS_MAX_NUMBER = 3;

  public GameObject[] shipsPrefabs;
  public GameObject timerPrefab;
  public GameObject lineRendererPrefab;
  public GameObject worldSpaceUi;
  string selectedTarget = null;

  ControllerScript controller;
  MapController mapController;
  MapUI ui;

  ShipPath[] paths = new ShipPath[SHIPS_MAX_NUMBER];
  bool[] destinationReached = new bool[SHIPS_MAX_NUMBER];
  AIDestinationSetter[] destinations = new AIDestinationSetter[SHIPS_MAX_NUMBER];
  GameObject[] shipsSpawned = new GameObject[SHIPS_MAX_NUMBER];
  GameObject[] timersSpawned = new GameObject[SHIPS_MAX_NUMBER];
  LineRenderer[] lineRenderers = new LineRenderer[SHIPS_MAX_NUMBER];

  void Update() {
    detectClick();
    detectDirection();
    updateTimers();
    checkIfDestinationReached();
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
    float SPEED_THRESHOLD = 0.0005f;
    // TODO: Improve to have 8 directions
    // TODO: for each direction show a different sprite
    for (int i = 0; i < SHIPS_MAX_NUMBER; i++) {
      if (shipsSpawned[i] == null)continue;
      if (paths[i].desiredVelocity.x >= SPEED_THRESHOLD) {
        shipsSpawned[i].transform.localScale = new Vector3(-1f, 1f, 1f);
      } else if (paths[i].desiredVelocity.x <= -SPEED_THRESHOLD) {
        shipsSpawned[i].transform.localScale = new Vector3(1f, 1f, 1f);
      }
    }
  }

  //*****************************************************************
  // UPDATE the timers that indicate the ships' time left until arrival
  //*****************************************************************
  void updateTimers() {
    for (int i = 0; i < SHIPS_MAX_NUMBER; i++) {
      if (timersSpawned[i] == null || paths[i].getPath() == null)continue;
      // Update the position
      Vector3 shipPosition = shipsSpawned[i].transform.position;
      timersSpawned[i].transform.position = new Vector3(shipPosition.x, shipPosition.y + 0.6f, 0.0f);
      // Update the content
      int timeLeft = (int)(paths[i].remainingDistance / paths[i].speed);
      string formattedTimeLeft = timeLeft > 60 ?
        Math.Floor((double)timeLeft / 60) + Language.Field["MINUTES_FIRST_LETTER"] + " " + timeLeft % 60 + Language.Field["SECONDS_FIRST_LETTER"] :
        timeLeft + Language.Field["SECONDS_FIRST_LETTER"];
      timersSpawned[i].transform.GetChild(0).GetComponent<Text>().text = formattedTimeLeft;
    }
  }

  //*****************************************************************
  // CHECK if any ship has reached its destination
  //*****************************************************************
  void checkIfDestinationReached() {
    for (int i = 0; i < SHIPS_MAX_NUMBER; i++) {
      if (paths[i] != null && paths[i].reachedEndOfPath && !destinationReached[i]) {
        // Do this when a ship destination is reached
        controller.getUser().getVillage().getShip(i).finishJourney();
        destinationReached[i] = true;
        // Hide the ship on the map and the timer
        Destroy(shipsSpawned[i]);
        Destroy(paths[i]);
        Destroy(destinations[i]);
        Destroy(timersSpawned[i]);
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
        startAttack();
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
      int spriteNumber = ship.getLevel() < 5 ? ship.getLevel() - 1 : 3;
      shipsSpawned[shipNumber] = (GameObject)Instantiate(
        shipsPrefabs[spriteNumber],
        ship.getCurrentPosition(),
        Quaternion.identity
      );
      paths[shipNumber] = shipsSpawned[shipNumber].GetComponent<ShipPath>();
      destinations[shipNumber] = shipsSpawned[shipNumber].GetComponent<AIDestinationSetter>();
      // Spawn a timer that shows how long until the destination
      Vector3 timerPosition = shipsSpawned[shipNumber].transform.position;
      timersSpawned[shipNumber] = (GameObject)Instantiate(
        timerPrefab,
        worldSpaceUi.transform,
        false
      );
      timersSpawned[shipNumber].transform.position = new Vector3(timerPosition.x, timerPosition.y + 0.6f, 0.0f);
      updateTimers();
    }
    // The ship starts the navigation towards the destination specified by ShipJourney
    startNavigation(shipNumber);
    showPath(shipNumber);
  }

  //*****************************************************************
  // DRAW the ship navigation path on the scene
  //*****************************************************************
  async void showPath(int shipNumber) {
    // Create a new LineRenderer, which is the object that takes care of drawing the path
    if (lineRenderers[shipNumber] == null) {
      GameObject lineRendererSpawned = (GameObject)Instantiate(lineRendererPrefab, Vector3.zero, Quaternion.identity);
      lineRenderers[shipNumber] = lineRendererSpawned.GetComponent<LineRenderer>();
    }
    // Wait until the path has been calculated
    while (paths[shipNumber].getPath() == null) {
      await Task.Delay(100);
    }
    // Then use the LineRenderer to show the path
    List<Vector3> path = paths[shipNumber].getPath();
    lineRenderers[shipNumber].positionCount = path.Count;
    for (int i = 0; i < path.Count; i++) {
      lineRenderers[shipNumber].SetPosition(i, path[i]);
    }
  }
}