using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Every puzzle must inherit from this class
/// </summary>
public abstract class AbstractPuzzle : MonoBehaviour {

    /// <summary>
    /// name of this puzzle type
    /// </summary>
    public string typeName;//++++++++++++

    /// <summary>
    /// list of items which should preserve original size during the scaling of the room
    /// </summary>
    public List<Transform> nonScalables;

    //----------------------------------------------
    //things needed for running the puzzle
    //----------------------------------------------

    /// <summary>
    /// actions for the start of a configuration containing this type of puzzles
    /// </summary>
    /// <param name="c">the starting configuration</param>
    public abstract void StartConfig(Configuration c);
    /// <summary>
    /// actions for the end of a configuration
    /// </summary>
    public abstract void FinishConfig();
    /// <summary>
    /// actions for start of the start phase
    /// </summary>
    public abstract void StartStart();
    /// <summary>
    /// actions for end of the start phase
    /// </summary>
    public abstract void EndStart();
    /// <summary>
    /// actions for start of the tutorial phase
    /// </summary>
    public abstract void StartTut();
    /// <summary>
    /// actions for end of the tutorial phase
    /// </summary>
    public abstract void EndTut();
    /// <summary>
    /// actions for start of any regular phase
    /// </summary>
    public abstract void StartPhase();
    /// <summary>
    /// actions for end of any regular phase
    /// </summary>
    public abstract void EndPhase();
    /// <summary>
    /// actions for the moment when time runs out
    /// </summary>
    public abstract void OnTimerStop();

    //----------------------------------------------
    //things needed for creating the puzzle in menu
    //----------------------------------------------

    /// <summary>
    /// reference to customized copy of the asset "PuzzleInfoPanel"
    /// </summary>
    public GameObject infoPanelPrefab;

    /// <summary>
    /// fills the <paramref name="panel"/> with information about <paramref name="puzzle"/>
    /// </summary>
    /// <param name="panel">an instance of infoPanelPrefab</param>
    /// <param name="puzzle"></param>
    /// <returns>true = <paramref name="panel"/> was filled without any errors (no missing files etc.)</returns>
    public abstract bool FillTheInfoPanel(GameObject panel, Puzzle puzzle);

    /// <summary>
    /// reference to customized copy of the asset "PuzzlePanel"
    /// </summary>
    public GameObject interactibleInfoPanelPrefab;

    /// <summary>
    /// custom preparations called after instantioation of <paramref name="panel"/>
    /// </summary>
    /// <param name="panel">an instance of interactibleInfoPanelPrafab</param>
    /// <param name="i">position of <paramref name="panel"/> in the list in "Create Configuration" menu</param>
    public abstract void PrepareInteractibleInfoPanel(GameObject panel, int i);

    /// <summary>
    /// checks if the <paramref name="panel"/> was correctly filled
    /// </summary>
    /// <param name="panel">an instance of interactibleInfoPanelPrafab</param>
    /// <param name="i">position of <paramref name="panel"/> in the list in "Create Configuration" menu</param>
    /// <returns>true = <paramref name="panel"/> was filled correctly</returns>
    public abstract bool CheckFillingCorrect(GameObject panel, int i);

    /// <summary>
    /// creates new instance of the class Puzzle filled with informations gathered from <paramref name="panel"/>
    /// </summary>
    /// <param name="panel">an instance of interactibleInfoPanelPrafab</param>
    /// <param name="i">position of <paramref name="panel"/> in the list in "Create Configuration" menu</param>
    /// <returns>the new instance of Puzzle</returns>
    public abstract Puzzle CreatePuzzle(GameObject panel, int i);

    /// <summary>
    /// get the name the experimentor set in the interactibleInfoPanel
    /// </summary>
    /// <param name="panel"></param>
    /// <returns>name of puzzle described by <paramref name="panel"/> </returns>
    public abstract string GetPuzzleName(GameObject panel);



    //----------------------------------------------
    //things needed for replaying the experiment
    //----------------------------------------------
    public abstract void Simulate(string[] atoms);
}
