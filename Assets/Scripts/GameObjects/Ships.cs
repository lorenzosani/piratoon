using System;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class Ships : MonoBehaviour {

  AIPath path;
  AIDestinationSetter destination;
  public GameObject ship;

  void Update() {
    path = ship.GetComponent<AIPath>();
    destination = ship.GetComponent<AIDestinationSetter>();
    detectDirection();
    detectClick();
  }

  void detectDirection() {
    // TODO: Improve to have 8 directions
    // TODO: for each direction show a different sprite
    if (path.desiredVelocity.x >= 0.01f) {
      ship.transform.localScale = new Vector3(-1f, 1f, 1f);
    } else if (path.desiredVelocity.x <= -0.01f) {
      ship.transform.localScale = new Vector3(1f, 1f, 1f);
    }
  }

  void detectClick() {
    if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended) {
      onCityClick(Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position));
    } else if (Input.GetMouseButtonUp(0)) {
      onCityClick(Camera.main.ScreenToWorldPoint(Input.mousePosition));
    }
  }

  public void onCityClick(Vector3 position) {
    Vector2 position2d = new Vector2(position.x, position.y);
    RaycastHit2D raycastHit = Physics2D.Raycast(position2d, Vector2.zero);
    if (raycastHit) {

      string cityName = raycastHit.collider.name;
      Debug.Log("Raycast hit " + cityName);
      if (cityName.Split('_')[0] == "city") { // Check if the click is on a city
        Debug.Log("Set target");
        destination.target = raycastHit.transform; // If yes, set target to the pathfinder
      }
    }
  }
}