using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pathfinding;
using UnityEngine;
using UnityEngine.UI;

public class Attacks : MonoBehaviour {
  static int SHIPS_MAX_NUMBER = 3;
  string selectedTarget = null;

  public GameObject[] shipsPrefabs;
  public GameObject timerPrefab;
  public GameObject lineRendererPrefab;
  public GameObject markerPrefab;
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
      sr.sprite = (Sprite)Resources.Load("Images/Ships/Ship" + (i + 1).ToString() + direction, typeof(Sprite));
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
        controller.getUser().getVillage().getShip(i).finishJourney();
        destinationReached[i] = true;
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
  // GET the selected target position towards which a ship will navigate
  //*****************************************************************
  Transform getNavigationTarget(int i) {
    if (selectedTarget == null) {
      Ship ship = controller.getUser().getVillage().getShip(i);
      return GameObject.Find(ship.getCurrentJourney().getDestinationName()).transform;
    } else {
      return GameObject.Find(selectedTarget).transform;
    }
  }

  //*****************************************************************
  // START the navigation of a ship by setting the target
  //*****************************************************************
  void startNavigation(int ship) {
    destinations[ship].target = getNavigationTarget(ship);
    destinationReached[ship] = false;
  }

  //*****************************************************************
  // SET which ship is going to attack and start the navigation
  //*****************************************************************
  public void spawnShip(int shipNumber) {
    Ship ship = controller.getUser().getVillage().getShip(shipNumber);
    Vector3 currentShipPosition = ship.getCurrentPosition();
    // Create ShipJourney object and add it to the ship
    if (selectedTarget != null) {
      if (getNavigationTarget(shipNumber).position == currentShipPosition) {
        return;
      }
      ShipJourney journey = new ShipJourney(
        currentShipPosition,
        getNavigationTarget(shipNumber).position,
        DateTime.Now
      );
      journey.setDestinationName(selectedTarget);
      ship.finishJourney();
      ship.startJourney(journey);
    }
    // Spawn that ship onto the map only if it's not been spawn already
    if (shipsSpawned[shipNumber] == null) {
      int spriteNumber = ship.getLevel() < 5 ? ship.getLevel() - 1 : 3;
      shipsSpawned[shipNumber] = (GameObject)Instantiate(
        shipsPrefabs[spriteNumber],
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
    // Create a new LineRenderer, which is the object that takes care of drawing the path
    if (lineRenderers[shipNumber] != null) {
      Destroy(lineRenderers[shipNumber].gameObject);
      paths[shipNumber].resetPath();
    }
    GameObject lineRendererSpawned = (GameObject)Instantiate(lineRendererPrefab, Vector3.zero, Quaternion.identity);
    lineRenderers[shipNumber] = lineRendererSpawned.GetComponent<LineRenderer>();
    // Wait until the path has been calculated
    while (paths[shipNumber].getPath() == null) {
      await Task.Delay(10);
    }
    // Add information about the path to the ShipJourney object
    ShipJourney journey = controller.getUser().getVillage().getShip(shipNumber).getCurrentJourney();
    journey.setPath(paths[shipNumber].getPath());
    journey.setDuration((int)(paths[shipNumber].remainingDistance / paths[shipNumber].speed));
    // Then use the LineRenderer to show the path
    List<Vector3> path = paths[shipNumber].getPath();
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
      markerPrefab,
      worldSpaceUi.transform,
      false
    );
    shipMarkers[shipNumber].transform.position = new Vector3(place.position.x, place.position.y + 0.6f, 0.0f);
  }
}