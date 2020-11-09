using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapController : MonoBehaviour {
  ControllerScript controller;

  public MapUI ui;
  public GameObject worldSpaceUi;
  public GameObject hideoutsParent;

  void Start() {
    ui.showLoadingScreen();
    controller = GameObject.Find("GameController").GetComponent<ControllerScript>();

    populateMap();
    moveCameraOverHideout(controller.getUser().getVillage().getPosition());
    ui.showLoadingScreen(false);
    Camera.main.GetComponent<PanAndZoom>().Zoom(2, 5);
  }

  //*****************************************************************
  // GET the script that manages the map UI
  //*****************************************************************
  public MapUI getUI() {
    return ui;
  }

  //*****************************************************************
  // POPULATE the map with the correct players' hideouts
  //*****************************************************************
  void populateMap() {
    MapUser[] players = controller.getMap().getPlayers();
    for (int i = 0; i < players.Length; i++) {
      if (players[i] != null && players[i].getId() != null && players[i].getId() != "") {
        // Instantiate the hideout icon at the right position
        GameObject hideout = (GameObject)Instantiate(
          (GameObject)Resources.Load("Prefabs/Hideout", typeof(GameObject)), MapPositions.get(i),
          Quaternion.identity
        );
        hideout.transform.parent = hideoutsParent.transform;
        hideout.layer = 8;
        // The hideout object has name: 'hideout_[position]_[userId]' DO NOT CHANGE THIS
        hideout.name = String.Format("hideout_{0}_{1}", i, players[i].getId());
        // Put the username above the hideout in the world space UI
        GameObject username = (GameObject)Instantiate(
          (GameObject)Resources.Load("Prefabs/MapUsername", typeof(GameObject)),
          worldSpaceUi.transform,
          false
        );
        username.GetComponent<Text>().text = players[i].getUsername();
        username.transform.position = new Vector3(MapPositions.get(i).x, MapPositions.get(i).y + 0.8f, 0.0f);
      }
    }
    AstarPath.active.Scan();
  }

  //*****************************************************************
  // SHOW information about a hideout
  //*****************************************************************
  public void showHideoutInfo(string userData, string villageData) {
    User user = JsonConvert.DeserializeObject<User>(userData);
    Village village = JsonConvert.DeserializeObject<Village>(villageData);
    ui.populateHideoutPopup(
      user.getUsername(),
      user.getLevel(),
      user.getResources(),
      village.getStrength()
    );
  }

  //*****************************************************************
  // PUTS the camera just above the user's hideout
  //*****************************************************************
  void moveCameraOverHideout(int hideoutPosition) {
    Camera.main.transform.position = new Vector3(
      MapPositions.get(hideoutPosition).x, MapPositions.get(hideoutPosition).y, -10.0f
    );
  }

  //*****************************************************************
  // CLOSE the map and show the hideout
  //*****************************************************************
  public async void close() {
    Camera.main.GetComponent<PanAndZoom>().Zoom(Camera.main.orthographicSize, 3);
    await Task.Delay(200);
    ui.showLoadingScreen();
    Destroy(GameObject.Find("GameController"));
    SceneManager.LoadScene("Hideout", LoadSceneMode.Single);
  }
}