using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Attacks : MonoBehaviour {
  Ship attackingShip = null;
  ControllerScript controller;
  MapController mapController;
  MapUI ui;

  void Start() {
    controller = GameObject.Find("GameController").GetComponent<ControllerScript>();
    mapController = GetComponent<MapController>();
    ui = mapController.getUI();
  }

  public void startAttack() {
    Ship[] ships = controller.getUser().getVillage().getShips();
    if (ships.Count(s => s != null) > 1) {
      // If the user has more than one ship, let them pick one for the attack
      ui.showHideoutPopup(false);
      ui.showShipPicker(ships, true);
    } else {
      // Otherwise just set that ship as the attacking ship
      foreach (Ship s in ships) {
        if (s != null) {
          ui.showHideoutPopup(false);
          setAttackingShip(s.getSlot());
          break;
        }
      }
    }
  }

  //*****************************************************************
  // SET which user ship is going to attack and start the navigation
  //*****************************************************************
  public void setAttackingShip(int ship) {
    Ship[] ships = controller.getUser().getVillage().getShips();
    attackingShip = ships[ship];
    mapController.startNavigation();
  }
}