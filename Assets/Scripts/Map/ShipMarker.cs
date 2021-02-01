using System;
using System.Collections.Generic;
using UnityEngine;

public class ShipMarker : MonoBehaviour {

  public void showShipMarkerPopup() {
    MapController controller = GameObject.Find("MapController").GetComponent<MapController>();
    controller.getUI().showPopupMessage(Language.Field["ANCHORED"]);
  }
}