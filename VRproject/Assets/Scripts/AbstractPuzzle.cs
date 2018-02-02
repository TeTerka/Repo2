using System.Collections.Generic;
using UnityEngine;

//v podstate soubor veci, ktere musi puzzle mit, aby s nim umel NewManager korektne pracovat
//prikladem potomku teto tridy jsou PipePuzzle a CubePuzzle

public abstract class AbstractPuzzle : MonoBehaviour {

    public List<Transform> nonScalables;//seznam predmetu, ktere si maji zachovat pevne rozmery pri pocatecnim scalovanim mistnosti
                                        //napr. aby krychle zustaly krychlemi i kdyz se mistnost roztahne nerovnomerne

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
