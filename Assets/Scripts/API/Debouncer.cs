using System;
using System.Threading;

public class Debouncer {
  Action OnComplete;
  int WaitingMilliSeconds;
  System.Threading.Timer waitingTimer;

  public Debouncer(int waitingMilliSeconds, Action onComplete) {
    WaitingMilliSeconds = waitingMilliSeconds;
    OnComplete = onComplete;
    waitingTimer = new Timer(p => {
      OnComplete();
    });
  }
  public void onChange() {
    waitingTimer.Change(WaitingMilliSeconds, System.Threading.Timeout.Infinite);
  }
}