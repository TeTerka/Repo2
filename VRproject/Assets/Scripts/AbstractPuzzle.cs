using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractPuzzle : MonoBehaviour {

    public List<Transform> nonScalables;

    public abstract void StartConfig(Configuration c);
    public abstract void FinishConfig();

    public abstract void StartStart();
    public abstract void EndStart();
    public abstract void StartTut();
    public abstract void EndTut();
    public abstract void StartPhase();
    public abstract void EndPhase();

    public abstract void CustomUpdate();
    public abstract void OnTimerStop();
}
