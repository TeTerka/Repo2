using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

//this script holds a set of small classes that usually only contain data




/// <summary>
/// class attaching a name to a NPC model, which is used for selecting the model in menu
/// </summary>
[System.Serializable]
public class NpcModel
{ 
    /// <summary> the name of this model (to be displayed in menu)</summary>
    public string modelName;
    /// <summary> the 3D model</summary>
    public GameObject modelObject;
}



/// <summary>
/// class attaching a name to a NPC behaviour (NPC animator), which is used for selecting the behaviour in menu
/// </summary>
[System.Serializable]
public class NpcBeahviour
{
    /// <summary> the name of this behaviour (to be displayed in menu)</summary>
    public string bahaviourName;
    /// <summary> the behaviour is described by this animator controller</summary>
    public RuntimeAnimatorController behaviourAnimController;
}



/// <summary>
/// class for SpeechBehaviour containing info abou one line o text
/// </summary>
[System.Serializable]
public class Sentence
{
    /// <summary>the sentence</summary>
    public string text;
    /// <summary>if true, replace "Alex" in the sentence by the name of currently used virtual agent</summary>
    public bool replaceAlex;
    /// <summary>optional, used only if useSound==true in SpeechBehaviour</summary>
    public AudioClip audio;
}


/// <summary>
/// class holding information about a puzzle, used for serialization
/// </summary>
/// <remarks>
/// Unity is not able to serialize polymorphic classes, that is why this class contains the data for all the existing puzzle types
/// </remarks>
public class PuzzleData
{
    //for cubes
    /// <summary>path to the picture(CubePuzzle)</summary>
    public string pathToImage;
    [XmlArray("spawnPointMix")]
    [XmlArrayItem("int")]
    /// <summary>describes which cube goes to which spawn point (CubePuzzle)</summary>
    public List<int> spawnPointMix;

    //for pipes
    [XmlArray("StateField")]
    [XmlArrayItem("row")]
    /// <summary>layout of pipes in the grid (PipePuzzle)</summary>
    public List<Wrapper> chosenList;

    //for all
    /// <summary>width of grid (CubePuzzle and PipePuzzle)</summary>
    public int widthpx;
    /// <summary>heigth of grid (CubePuzzle and PipePuzzle)</summary>
    public int heigthpx;
    /// <summary>name of this puzzle (all puzzles)</summary>
    public string name;
}

/// <summary>
/// helper class used because Unity can not serialize 2D lists
/// </summary>
[System.Serializable]
public class Wrapper
{
    [XmlArray("states")]
    [XmlArrayItem("state")]
    public List<char> row;
}

/// <summary>
/// class holding information about one configuration, used for serialization
/// </summary>
public class Configuration
{
    /// <summary>configuration name</summary>
    public string name;
    /// <summary>true = virtual agent is present</summary>
    public bool withNPC;
    /// <summary>true = include tutorial phase</summary>
    public bool withTutorial;
    /// <summary>max time for solving one puzzle (in seconds)</summary>
    public int timeLimit;
    /// <summary>which virtual agent model is used</summary>
    public string modelName;
    /// <summary>which virtual agent behaviour is used</summary>
    public string behaviourName;
    /// <summary>which type of puzzle is used</summary>
    public string puzzleType;

    [XmlArray("Puzzles")]
    [XmlArrayItem("Puzzle")]
    /// <summary>list of puzzles that this configuration contains</summary>
    public List<PuzzleData> puzzles = new List<PuzzleData>();
}

/// <summary>
/// helper class to be able to serialize a list of configurations
/// </summary>
[XmlRoot("ListOfConfigurations")]
public class ListOfConfigurations
{
    [XmlArray("Confiurations")]
    [XmlArrayItem("Configuration")]
    public List<Configuration> configs = new List<Configuration>();
}

/// <summary>
/// class holding information about one experiment, used for serialization
/// </summary>
public class Experiment
{
    /// <summary>experiment name</summary>
    public string name;
    /// <summary>path to the file where results should be saved</summary>
    public string resultsFile;
    /// <summary>which puzzle type is used</summary>
    public string puzzleType;
    [XmlArray("Confiurations")]
    [XmlArrayItem("Configuration")]
    /// <summary>list of configurations this experiment contains</summary>
    public List<Configuration> configs = new List<Configuration>();
    [XmlArray("IDs")]
    [XmlArrayItem("id")]
    /// <summary>list of player ids which were already used in this experiment</summary>
    public List<string> ids = new List<string>();
}


/// <summary>
/// helper class to be able to serialize a list of experiments
/// </summary>
[XmlRoot("ListOfExperiments")]
public class ListOfExperiments
{
    [XmlArray("Experiments")]
    [XmlArrayItem("Experiment")]
    public List<Experiment> experiments = new List<Experiment>();
}

/// <summary>
/// interface for objects that are not grabable but still want some interactions with contorllers, IInteractible objects should have tag "InteractibleObject"
/// </summary>
public interface IInteractibleObject
{
    /// <summary>
    /// what should happen when the hair trigger on Vive controller is pressed while touching this object
    /// </summary>
    /// <param name="controller">controller that pressed the trigger</param>
    void OnTriggerPressed(ControllerScript controller);
}




//------------------------------------------------------------------------------------------------------------------------
//this part only describes the main page for documentation created by Doxygen
//------------------------------------------------------------------------------------------------------------------------



/*! \mainpage My Personal Index Page
 *
 * \section intro_sec Introduction
 *
 * This is the introduction.
 *
 * \section install_sec Installation
 *
 * \subsection step1 Step 1: Opening the box
 *
 * etc...
 */
