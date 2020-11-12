using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

//*****************************************************************
// SHIPPATH: This represents the path that a ship undertakes to go 
// from point A to B. Look at the docs for A* AIPath for more info.
//*****************************************************************

public class ShipPath : AIPath {
  List<Vector3> shipPath = null;

  protected override void OnPathComplete(Path p) {
    shipPath = p.vectorPath;
  }

  public List<Vector3> getPath() {
    return shipPath;
  }
}