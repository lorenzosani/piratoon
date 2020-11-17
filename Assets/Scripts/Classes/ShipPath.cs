using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using Pathfinding.Util;
using UnityEngine;

//*****************************************************************
// SHIPPATH: This represents the path that a ship undertakes to go 
// from point A to B. Look at the docs for A* AIPath for more info.
//*****************************************************************

public class ShipPath : AIPath {
  List<Vector3> shipPath = null;

  protected override void OnPathComplete(Path newPath) {
    shipPath = newPath.vectorPath;
    ABPath p = newPath as ABPath;

    if (p == null)throw new System.Exception("This function only handles ABPaths, do not use special path types");

    waitingForPathCalculation = false;

    // Increase the reference count on the new path.
    // This is used for object pooling to reduce allocations.
    p.Claim(this);

    // Path couldn't be calculated of some reason.
    // More info in p.errorLog (debug string)
    if (p.error) {
      p.Release(this);
      return;
    }

    // Release the previous path.
    if (path != null)path.Release(this);

    // Replace the old path
    path = p;

    // Make sure the path contains at least 2 points
    if (path.vectorPath.Count == 1)path.vectorPath.Add(path.vectorPath[0]);
    interpolator.SetPath(path.vectorPath);

    var graph = AstarData.GetGraph(path.path[0])as ITransformedGraph;
    movementPlane = graph != null ? graph.transform : (rotationIn2D ? new GraphTransform(Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(-90, 270, 90), Vector3.one)) : GraphTransform.identityTransform);

    // Reset some variables
    reachedEndOfPath = false;

    // Simulate movement from the point where the path was requested
    // to where we are right now. This reduces the risk that the agent
    // gets confused because the first point in the path is far away
    // from the current position (possibly behind it which could cause
    // the agent to turn around, and that looks pretty bad).
    interpolator.MoveToLocallyClosestPoint((GetFeetPosition() + p.originalStartPoint) * 0.5f);
    interpolator.MoveToLocallyClosestPoint(GetFeetPosition());

    // Update which point we are moving towards.
    // Note that we need to do this here because otherwise the remainingDistance field might be incorrect for 1 frame.
    // (due to interpolator.remainingDistance being incorrect).
    interpolator.MoveToCircleIntersection2D(position, pickNextWaypointDist, movementPlane);

    var distanceToEnd = remainingDistance;
    if (distanceToEnd <= endReachedDistance) {
      reachedEndOfPath = true;
      OnTargetReached();
    }
  }

  public List<Vector3> getPath() {
    return shipPath;
  }

  public void resetPath() {
    shipPath = null;
  }
}