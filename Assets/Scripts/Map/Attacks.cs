using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pathfinding;
using UnityEngine;
using UnityEngine.UI;

public class Attacks : MonoBehaviour {
  static int SHIPS_MAX_NUMBER = 3;
  string selectedTarget = null;

  public GameObject timerPrefab;
  public GameObject lineRendererPrefab;
  public GameObject[] markerPrefabs;
  public GameObject worldSpaceUi;

  ControllerScript controller;
  MapController mapController;
  MapUI ui;

  ShipPath[] paths = new ShipPath[SHIPS_MAX_NUMBER];
  bool[] destinationReached = new bool[SHIPS_MAX_NUMBER];
  AIDestinationSetter[] destinations = new AIDestinationSetter[SHIPS_MAX_NUMBER];
  GameObject[] shipsSpawned = new GameObject[SHIPS_MAX_NUMBER];
  GameObject[] timersSpawned = new GameObject[SHIPS_MAX_NUMBER];
  LineRenderer[] lineRenderers = new LineRenderer[SHIPS_MAX_NUMBER];
  GameObject[] shipMarkers = new GameObject[SHIPS_MAX_NUMBER];

  void Update() {
    detectClick();
    detectDirection();
    updateTimers();
  }

  void Start() {
    controller = GameObject.Find("GameController").GetComponent<ControllerScript>();
    mapController = GetComponent<MapController>();
    ui = mapController.getUI();
    InvokeRepeating("checkIfDestinationReached", 0.5f, 0.5f);

    // Populate ships on the map
    Ship[] ships = controller.getUser().getVillage().getShips();
    for (int i = 0; i < ships.Length; i++) {
      if (ships[i] != null) {
        if (ships[i].getCurrentJourney() != null) {
          spawnShip(i);
        } else {
          // Mark village/city where they're parked
          Vector3 position = ships[i].getCurrentPosition();
          Vector2 position2d = new Vector2(position.x, position.y);
          RaycastHit2D raycastHit = Physics2D.Raycast(position2d, Vector2.zero);
          if (raycastHit) {
            string hideoutName = raycastHit.collider.name;
            if (hideoutName.Split('_')[0] == "hideout" || hideoutName.Split('_')[0] == "city") {
              addShipMarker(GameObject.Find(hideoutName).transform, i);
            }
          }
        }
      }
    }
  }

  //*****************************************************************
  // PUBLIC: call this when you want to start an attack
  //*****************************************************************
  public void startAttack() {
    Ship[] shipsOwned = controller.getUser().getVillage().getShips();
    Debug.Log("TEST - SHIPS OWNED: " + shipsOwned.Count(s => s != null));
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
          Destroy(shipMarkers[s.getSlot()]);
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
    float THRESHOLD = 0.001f;
    for (int i = 0; i < SHIPS_MAX_NUMBER; i++) {
      if (shipsSpawned[i] == null)continue;
      // Get the sprite renderer
      SpriteRenderer sr = shipsSpawned[i].transform.GetChild(0).GetComponent<SpriteRenderer>();
      Vector3 speed = paths[i].desiredVelocity;
      string direction = "/S";
      if (speed.x <= THRESHOLD && speed.x >= -THRESHOLD) {
        if (speed.y >= THRESHOLD) {
          direction = "/N";
        } else {
          direction = "/S";
        }
      } else if (speed.x >= THRESHOLD) {
        if (speed.y >= -THRESHOLD && speed.y <= THRESHOLD) {
          direction = "/E";
        } else if (speed.y >= THRESHOLD) {
          direction = "/NE";
        } else {
          direction = "/SE";
        }
      } else {
        if (speed.y >= -THRESHOLD && speed.y <= THRESHOLD) {
          direction = "/W";
        } else if (speed.y >= THRESHOLD) {
          direction = "/NW";
        } else {
          direction = "/SW";
        }
      }
      int shipLevel = controller.getUser().getVillage().getShip(i).getLevel();
      sr.sprite = (Sprite)Resources.Load("Images/Ships/Ship" + (shipLevel > 5 ? "4" : (shipLevel).ToString()) + direction, typeof(Sprite));
    }
  }

  //*****************************************************************
  // UPDATE the timers that indicate the ships' time left until arrival
  //*****************************************************************
  void updateTimers() {
    // TODO: Fix time calculation, doesn't work
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
        string destinationName = controller.getUser().getVillage().getShip(i).getCurrentJourney().getDestinationName();
        addShipMarker(GameObject.Find(destinationName).transform, i);
        Ship ship = controller.getUser().getVillage().getShip(i);
        ship.finishJourney(GameObject.Find(destinationName).transform.position);
        destinationReached[i] = true;
        // Generate attack outcome
        generateAttackOutcome(ship, destinationName);
        // Hide the ship on the map and the timer
        Destroy(shipsSpawned[i]);
        Destroy(paths[i]);
        Destroy(destinations[i]);
        Destroy(timersSpawned[i]);
        Destroy(lineRenderers[i]);
      }
    }
  }

  //*****************************************************************
  // TODO: Needs refinement, this just generates a general attack outcome
  //*****************************************************************
  async void generateAttackOutcome(Ship ship, string enemyName) {
    int userStrength = 150 * ship.getLevel();
    int enemyStrength = 200;
    int[] enemyResources = new int[3];
    string outcomeMessage = "";
    bool userDataReceived = false;
    // Get enemy and user strengths
    API.GetUserData(
      new List<string> {
        "User",
        "Village"
      },
      result => {
        if (result.Data.ContainsKey("User") && result.Data.ContainsKey("Village")) {
          User user = JsonConvert.DeserializeObject<User>(result.Data["User"].Value);
          Village village = JsonConvert.DeserializeObject<Village>(result.Data["Village"].Value);
          user.setVillage(village);
          enemyStrength = village.getStrength();
          enemyResources = user.getResources();
          userDataReceived = true;
        } else {
          Debug.Log("ERROR: Failed to retrieve enemy strength. Value set to default.");
          userDataReceived = true;
        }
      },
      enemyName.Split('_')[2]
    );
    while (!userDataReceived) {
      await Task.Delay(10);
    }
    if (enemyName.Split('_')[0] == "hideout") {
      if (userStrength >= enemyStrength) { // If victory, compute the resources won
        int[] resourcesWon = new int[3];
        // TODO: deal with situations when the storage is full
        for (int i = 0; i < 3; i++) {
          resourcesWon[i] = (int)enemyResources[i] / 5 * ship.getLevel();
          controller.getUser().increaseResource(i, resourcesWon[i]);
        }
        // TODO: Remove resources from enemy after an attack
        outcomeMessage = String.Format(
          Language.Field["ATTACK_VICTORY"], resourcesWon[0], resourcesWon[1], resourcesWon[2]);
      } else { // If loss, compute whether ship is lost
        controller.getUser().getVillage().setShip(null, ship.getSlot());
        outcomeMessage = Language.Field["ATTACK_DEFEAT"];
      }
    } else {
      // TODO: Implement attack outcome for cities
      Debug.Log("Attack outcomes for cities have not yet been implemented");
    }
    // Show outcome message
    Debug.Log(outcomeMessage);
    ui.showPopupMessage(outcomeMessage);
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
        int cityNo = Int32.Parse(cityName.Split('_')[1]);
        mapController.showCityInfo(controller.getMap().getCities()[cityNo]);
      }
    }
  }

  //*****************************************************************
  // GET the selected target position towards which a ship will navigate
  //*****************************************************************
  Transform getNavigationTarget(int i) {
    if (selectedTarget == null) {
      Ship ship = controller.getUser().getVillage().getShip(i);
      Debug.Log("TEST - DESTINATION NAME: " + ship.getCurrentJourney().getDestinationName());
      return GameObject.Find(ship.getCurrentJourney().getDestinationName()).transform;
    } else {
      Debug.Log("TEST - SELECTED TARGET: " + selectedTarget);
      return GameObject.Find(selectedTarget).transform;
    }
  }

  //*****************************************************************
  // START the navigation of a ship by setting the target
  //*****************************************************************
  void startNavigation(int ship) {
    Debug.Log("TEST - SHIP NO: " + ship);
    destinations[ship].target = getNavigationTarget(ship);
    destinationReached[ship] = false;
  }

  //*****************************************************************
  // SET which ship is going to attack and start the navigation
  //*****************************************************************
  public void spawnShip(int shipNumber) {
    Debug.Log("TEST - SHIP NO: " + shipNumber);
    Ship ship = controller.getUser().getVillage().getShip(shipNumber);
    Debug.Log("TEST - SHIP LEVEL: " + ship.getLevel());
    Vector3 currentShipPosition = ship.getCurrentPosition();
    // Create ShipJourney object and add it to the ship
    if (selectedTarget != null) {
      Debug.Log("TEST - SELECTED TARGET: " + selectedTarget);
      if (getNavigationTarget(shipNumber).position == currentShipPosition) {
        return;
      }
      ShipJourney journey = new ShipJourney(
        currentShipPosition,
        getNavigationTarget(shipNumber).position,
        DateTime.Now
      );
      journey.setDestinationName(selectedTarget);
      ship.startJourney(journey);
    }
    // Spawn that ship onto the map only if it's not been spawn already
    if (shipsSpawned[shipNumber] == null) {
      int prefabNumber = ship.getLevel() < 5 ? ship.getLevel() : 4;
      Debug.Log("TEST - PREFAB NUMBER: " + prefabNumber);
      shipsSpawned[shipNumber] = (GameObject)Instantiate(
        (GameObject)Resources.Load("Prefabs/Ship" + prefabNumber, typeof(GameObject)),
        currentShipPosition,
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
    Debug.Log("TEST - SHIP NO: " + shipNumber);
    // Create a new LineRenderer, which is the object that takes care of drawing the path
    if (lineRenderers[shipNumber] != null) {
      Destroy(lineRenderers[shipNumber].gameObject);
      paths[shipNumber].resetPath();
    }
    GameObject lineRendererSpawned = (GameObject)Instantiate(lineRendererPrefab, Vector3.zero, Quaternion.identity);
    lineRenderers[shipNumber] = lineRendererSpawned.GetComponent<LineRenderer>();
    // Wait until the path has been calculated
    List<Vector3> path = paths[shipNumber].getPath();
    while (path == null) {
      await Task.Delay(10);
      path = paths[shipNumber].getPath();
    }
    // Add information about the path to the ShipJourney object
    ShipJourney journey = controller.getUser().getVillage().getShip(shipNumber).getCurrentJourney();
    journey.setPath(path);
    journey.setDuration((int)(paths[shipNumber].remainingDistance / paths[shipNumber].speed));
    // Then use the LineRenderer to show the path
    lineRenderers[shipNumber].positionCount = path.Count;
    for (int i = 0; i < path.Count; i++) {
      lineRenderers[shipNumber].SetPosition(i, path[i]);
    }
  }

  //*****************************************************************
  // MARK on the map that a ship is parked at a given place
  //*****************************************************************
  void addShipMarker(Transform place, int shipNumber) {
    shipMarkers[shipNumber] = (GameObject)Instantiate(
      markerPrefabs[shipNumber],
      worldSpaceUi.transform,
      false
    );
    shipMarkers[shipNumber].transform.position = new Vector3(place.position.x, place.position.y + 0.6f, 0.0f);
  }
}