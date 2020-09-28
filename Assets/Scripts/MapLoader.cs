using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapLoader : MonoBehaviour {

  ControllerScript controller;

  void Start() {
    controller = GameObject.Find("GameController").GetComponent<ControllerScript>();
  }

  //*****************************************************************
  // SHOW the map
  //*****************************************************************
  public async void open() {
    if (!API.IsRegistered()) {
      controller.getUI().showPopupMessage(Language.Field["MAP_REGISTER"]);
      return;
    }
    if (controller.getMap() == null) {
      controller.getUI().showMapError();
    }
    Camera.main.GetComponent<PanAndZoom>().Zoom(Camera.main.orthographicSize, 15);
    await Task.Delay(200);
    controller.getUI().showLoadingScreen();
    SceneManager.LoadScene("Map", LoadSceneMode.Single);
  }

  //*****************************************************************
  // FETCH a new position and map if the user is not assigned to one
  //*****************************************************************
  public async void reloadMap() {
    controller.getUI().showMapTryAgain(false);
    Mapmaking.Start();

    int secondsPassed = 0;
    while (controller.getMap() == null && secondsPassed <= 30) {
      await Task.Delay(3000);
      secondsPassed += 3;
    }
    if (secondsPassed >= 30) {
      controller.getUI().showMapTryAgain();
    } else {
      controller.getUI().showMapTryAgain();
      controller.getUI().showMapError(false);
      open();
    }
  }
}