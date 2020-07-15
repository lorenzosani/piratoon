using UnityEngine;
using System.Collections;

public class Observable
{
  ObserverScript observer;

  public void attachObserver(ObserverScript o){
    observer = o;
  }

  public void notifyChange(){
    observer.update();
  }
}