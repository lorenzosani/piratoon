using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour {

  ControllerScript controller;

  void Start() {
    controller = GameObject.Find("GameController").GetComponent<ControllerScript>();
  }

  public void getMoreResources() {
    int PRICE = 30;
    int[] RESOURCES = new int[3] { 500, 500, 50 };
    buyResourcesWithPearls(PRICE, RESOURCES);
  }

  public void getMoreWood() {
    int PRICE = 10;
    int[] RESOURCES = new int[3] { 500, 0, 0 };
    buyResourcesWithPearls(PRICE, RESOURCES);
  }

  public void getMoreStone() {
    int PRICE = 10;
    int[] RESOURCES = new int[3] { 0, 500, 0 };
    buyResourcesWithPearls(PRICE, RESOURCES);
  }

  public void getMoreGold() {
    int PRICE = 10;
    int[] RESOURCES = new int[3] { 0, 0, 50 };
    buyResourcesWithPearls(PRICE, RESOURCES);
  }

  void buyResourcesWithPearls(int PRICE, int[] RESOURCES) {
    // Check if user has enough pearls
    if (controller.getUser().getPearl() < PRICE) {
      controller.getUI().showPopupMessage(Language.Field["NOT_PEARL"]);
      return;
    }
    // Check if user has enough storage space
    int[] storageSpace = controller.getUser().getStorageSpaceLeft();
    if (storageSpace[0] < RESOURCES[0] || storageSpace[1] < RESOURCES[1] || storageSpace[2] < RESOURCES[2]) {
      controller.getUI().showPopupMessage(Language.Field["STORAGE_SPACE"]);
    }
    // If yes, remove add resources and remove pearls
    controller.getUser().increaseResource(0, RESOURCES[0]);
    controller.getUser().increaseResource(1, RESOURCES[1]);
    controller.getUser().increaseResource(2, RESOURCES[2]);
    controller.getUser().increasePearl(-PRICE);
    // Show a success message
    controller.getUI().showPopupMessage(Language.Field["MORE_RES"]);
  }
}