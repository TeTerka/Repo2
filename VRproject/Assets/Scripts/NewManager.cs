using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;


/// <summary>
/// the main class for managing the running experiment
/// </summary>
public class NewManager : MonoBehaviour {
    
    //sigleton stuff
    public static NewManager instance;

    [Header("puzzle types")]
    public List<AbstractPuzzle> puzzleTypes = new List<AbstractPuzzle>();


    //information about current state
    private Experiment activeExperiment = null;
    public AbstractPuzzle CurrentPuzzle { get; private set; }
    public Configuration ActiveConfig { get; private set; }
    public int ActivePuzzleIndex { get; private set; }
    public bool InStart { get; private set; }
    public bool InTut { get; private set; }
    public bool PhaseFinished { get; private set; }
    public bool InTestMode { get; private set; }
    public bool InReplayMode { get; private set; }
    public bool Switching { get; private set; }

    [Header("model picture")]
    public Renderer modelPictureFrame;
    public Transform imageHolder;

    [Header("room scale stuff")]
    public Transform level;
    public Transform cameraRigPoint;
    public Transform editorFloorScale;
    private Vector3 originalScale = new Vector3();

    [Header("referencies to UI")]
    public UImanagerScript UImangr;
    public Text timerText;
    private float timeLeft; //in seconds
    private bool countingDown = false;
    public Text scoreText;
    private int skore;

    //next button
    public Button nextButton;
    private Color highlightColor = Color.green;
    private Color normalColor = Color.white;

    //other UI
    public Text idInuput;
    public Text messageOutput;
    public InputField playeridInputField;
    public GameObject popupPanel;
    public GameObject phaseLoadingPanel;
    public GameObject testModeHighlight;
    public Text expNameText;
    public Button pauseButton;

    //scrollview UI
    public GameObject phaseScrollContent;
    public GameObject configScrollContent;
    public GameObject namePlatePrefab;
    private List<GameObject> phaseLabels = new List<GameObject>();
    private List<Button> configButtons = new List<Button>();
    public Button configNameButtonPrafab;

    [Header("for changing NPC models and behaviours")]
    public GameObject theNpc;
    public Transform npcPoint;
    public List<NpcModel> npcModels = new List<NpcModel>();
    public List<NpcBeahviour> npcBehaviours = new List<NpcBeahviour>();

    [Header("for talking during animation")]
    public string npcName = "";
    public GameObject subtitlesCanvas;
    public int whoWantsSubtitles = -1;


    //--------------------------------------------------------------------------------------------------------------------------
    // INITIALIZATION
    //--------------------------------------------------------------------------------------------------------------------------

    private void Awake()
    {
        //singleton stuff
        if (instance != null)
        {
            Debug.Log("Multiple Managers in one scene!");
        }
        instance = this;

        //prepare room
        ScaleRoomToFitPlayArea();

        //inicialize
        modelPictureFrame.material.mainTexture = null;
        modelPictureFrame.material.color = Color.white;
        InTestMode = false;
        InReplayMode = false;
    }

    /// <summary>
    /// scale the virtual room so that it is the same size as the real room
    /// </summary>
    private void ScaleRoomToFitPlayArea()
    {
        //get info from play area
        Valve.VR.HmdQuad_t rect = new Valve.VR.HmdQuad_t();
        while (!SteamVR_PlayArea.GetBounds(SteamVR_PlayArea.Size.Calibrated, ref rect))
        {
            System.Threading.Thread.Sleep(1000);
        }

        //count new scale
        float floorScaleFactorX = editorFloorScale.localScale.x;
        float floorScaleFactorZ = editorFloorScale.localScale.z/2;//only half of the room is for the player       
        Vector3 newScale = new Vector3(Mathf.Abs(rect.vCorners0.v0 - rect.vCorners2.v0)/ floorScaleFactorX, 1,Mathf.Abs(rect.vCorners0.v2 - rect.vCorners2.v2)/ floorScaleFactorZ);
        //scale the room
        level.localScale = newScale;
        //move camera rig to original position
        this.transform.position = cameraRigPoint.position;
        //save this scale of model picture so that puzzles can change it but they also can go back to this size
        originalScale = imageHolder.localScale;

        //adjust scale to objects which should keep original size
        foreach (AbstractPuzzle puzzleType in puzzleTypes)
        {
            foreach (Transform x in puzzleType.nonScalables)
            {
                x.localScale = new Vector3(1 / x.lossyScale.x, 1, 1 / x.lossyScale.z);
            }
        }
    }



    //--------------------------------------------------------------------------------------------------------------------------
    // STARTING & FINISHING EXPERIMENTS & CONFIGURATIONS
    //--------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// start experiement <paramref name="e"/> in selected mode
    /// </summary>
    /// <param name="e">starting experiment</param>
    /// <param name="justTesting">true = test mode (no data seving)</param>
    /// <param name="justReplay">true = replay mode (replaying from a logfile)</param>
    public void StartExperiment(Experiment e,bool justTesting,bool justReplay)
    {
        InTestMode = justTesting;
        testModeHighlight.SetActive(InTestMode);
        InReplayMode = justReplay;
        activeExperiment = e;
        expNameText.text = e.name;

        //decide which puzzle type is being used
        CurrentPuzzle = null;
        foreach (AbstractPuzzle puzzleType in puzzleTypes)
        {
            if (puzzleType.typeName == e.puzzleType)
                CurrentPuzzle = puzzleType;
        }

        if(CurrentPuzzle = null)
        {
            ErrorCatcher.instance.Show("Error, puzzle type "+e.puzzleType+" is not available.");
        }

        //fill out the configs scrollview
        for (int i = 0; i < e.configs.Count; i++)
        {
            int iForDelegate = i;
            var p = Instantiate(configNameButtonPrafab, configScrollContent.transform);
            p.GetComponentInChildren<Text>().text = e.configs[i].name;
            configButtons.Add(p);
            p.onClick.AddListener(delegate { if (InStart) { FinishStartPhase(); FinishCurrentConfig(); StartConfig(e.configs[iForDelegate]); } });
        }

        if(InReplayMode)
        {
            SetControlsInteractible(false);
        }
        else
        {
            SetControlsInteractible(true);
        }

        //start the first configuration (every experiment certainly has at least one)
        StartConfig(e.configs[0]);
    }

    /// <summary>
    /// enables or disbles controls that should not be used during replay mode
    /// </summary>
    /// <param name="b">true = enable, false = disable</param>
    private void SetControlsInteractible(bool b)
    {
        nextButton.interactable = b;
        foreach (var item in configButtons)
        {
            item.interactable = b;
        }
        playeridInputField.interactable = b;
        pauseButton.interactable = !b;
    }

    /// <summary>
    /// ends current experiment
    /// </summary>
    /// <remarks>should be called only after current configuration was correctly finished</remarks>
    private void FinishExperiment()
    {
        for (int i = configButtons.Count - 1; i >= 0; i--)
        {
            Destroy(configButtons[i].gameObject);
            configButtons.RemoveAt(i);
        }
        activeExperiment = null;
        expNameText.text = "";
        CurrentPuzzle = null;
    }

    /// <summary>
    /// starts new configuration <paramref name="c"/>
    /// </summary>
    /// <param name="c">starting configuration</param>
    private void StartConfig(Configuration c)
    {
        ActiveConfig = c;

        //fill out the phase scrollview
        var p = Instantiate(namePlatePrefab, phaseScrollContent.transform);
        p.GetComponentInChildren<Text>().text = "start phase";
        phaseLabels.Add(p);
        if (c.withTutorial)
        {
            var q = Instantiate(namePlatePrefab, phaseScrollContent.transform);
            q.GetComponentInChildren<Text>().text = "tutorial phase";
            phaseLabels.Add(q);
        }
        for (int i = 0; i < c.puzzles.Count; i++)
        {
            var q = Instantiate(namePlatePrefab, phaseScrollContent.transform);
            q.GetComponentInChildren<Text>().text = c.puzzles[i].name;
            phaseLabels.Add(q);
        }

        //prepare npc
        if(c.withNPC)
        {
            NpcModel nm = null;
            foreach (NpcModel model in npcModels)
            {
                if(model.modelName==c.modelName)
                {
                    nm = model;
                    break;
                }
            }
            NpcBeahviour nb = null;
            foreach (NpcBeahviour b in npcBehaviours)
            {
                if (b.bahaviourName == c.behaviourName)
                {
                    nb = b;
                    break;
                }
            }
            if(nb!=null && nm!=null)
            {
                CreateCharacter(nm, nb);
            }
        }
        else
        {
            theNpc = null;
        }

        //puzzle specific actions
        CurrentPuzzle.StartConfig(c);

        originalScale = imageHolder.localScale;
        StartStart();
    }

    /// <summary>
    /// ends current configuration
    /// </summary>
    /// <remarks>should be called only after current phase was correctly finished</remarks>
    private void FinishCurrentConfig()
    {
        //puzzle specific actions
        CurrentPuzzle.FinishConfig();

        //clear the phase scrollview
        for (int i = phaseLabels.Count - 1; i >= 0; i--)
        {
            Destroy(phaseLabels[i]);
            phaseLabels.RemoveAt(i);
        }

        imageHolder.localScale = originalScale;
        DestroyCharacter();
        ActiveConfig = null;
        idInuput.text = "";
    }




    //--------------------------------------------------------------------------------------------------------------------------
    // PHASES
    //--------------------------------------------------------------------------------------------------------------------------



    /// <summary>
    /// starts the start phase
    /// </summary>
    private void StartStart()
    {
        //logging
        if (!InReplayMode)
        {
            Logger.instance.SetLoggerPath(null);
        }

        //general preparations
        InStart = true;
        phaseLabels[0].GetComponent<Image>().color = highlightColor;
        nextButton.GetComponentInChildren<Text>().text = "NEXT PHASE";

        //puzzle specific preparation
        CurrentPuzzle.StartStart();

        if (!InReplayMode)
        {
            //make it possible to switch to different configurations
            foreach (var item in configButtons)
            {
                item.interactable = true;
            }
            //and to change player id
            playeridInputField.interactable = true;
        }

        //animation of the NPC
        if (theNpc != null)
        {
            theNpc.GetComponent<Animator>().SetTrigger("StartStart");
        }

        if (InReplayMode)
        {
            StartCoroutine(SwitchPhase());
        }
    }

    private void FinishStartPhase()
    {
        //logging - creating a new logfile
        if ((!InReplayMode) && (!InTestMode) && (idInuput.text != ""))
        {
            string newPath = activeExperiment.resultsFile.Substring(0, activeExperiment.resultsFile.LastIndexOf('/') + 1) + "/" + ActiveConfig.name + "_" + idInuput.text + ".mylog";
            try
            {
                var f = File.Create(newPath); f.Close();
                using (StreamWriter sw = new StreamWriter(newPath, true))
                {
                    sw.WriteLine(ActiveConfig.name);
                    sw.WriteLine(idInuput.text);
                    sw.WriteLine(Time.time);
                    sw.Close();
                }
            }
            catch (System.Exception e)
            {
                ErrorCatcher.instance.Show("Wanted to create new logfile " + activeExperiment.resultsFile + " but it threw error " + e.ToString());
            }
            Logger.instance.SetLoggerPath(newPath);
        }
        if (InReplayMode)
        {
            Logger.instance.ReadLog();
        }
        
        //disable the option to switch to different configuration
        foreach (var item in configButtons)
        {
            item.interactable = false;
        }

        //puzzle specific actions
        CurrentPuzzle.EndStart();

        //general actions
        BasicFinish();
        phaseLabels[0].GetComponent<Image>().color = normalColor;
        InStart = false;

    }


    /// <summary>
    /// starts tutorial phase
    /// </summary>
    private void StartTutorial()
    {
        //general preparations
        InTut = true;
        phaseLabels[1].GetComponent<Image>().color = highlightColor;

        //puzzle specific preparations
        CurrentPuzzle.StartTut();

        //animation of the NPC
        if (theNpc != null)
        {
            theNpc.GetComponent<Animator>().SetTrigger("StartTutorial");
        }
    }

    /// <summary>
    /// end tutorial phase
    /// </summary>
    private void FinishTutorial()
    {
        phaseLabels[1].GetComponent<Image>().color = normalColor;
        InTut = false;
        CurrentPuzzle.EndTut();
        BasicFinish();
    }

    /// <summary>
    /// starts a regular phase (a phase containing a puzzle)
    /// </summary>
    private void PhaseStart()
    {
        ActivePuzzleIndex++;
        //visualise current phase
        if (ActiveConfig.withTutorial)
        {
            phaseLabels[ActivePuzzleIndex + 2].GetComponent<Image>().color = highlightColor;
        }
        else
        {
            phaseLabels[ActivePuzzleIndex + 1].GetComponent<Image>().color = highlightColor; 
        }

        //puzzle specific actions
        CurrentPuzzle.StartPhase();

        //start timer
        timeLeft = ActiveConfig.timeLimit;
        countingDown = true;

        //play PuzzleStart animation
        if(theNpc!=null)
            theNpc.GetComponent<Animator>().SetTrigger("StartNewPuzzle");

    }

    /// <summary>
    /// finishes a regular phase (a phase containing a puzzle)
    /// </summary>
    private void PhaseFinish()
    {
        //cancel phase label highlight
        if (ActiveConfig.withTutorial)
        {
            phaseLabels[ActivePuzzleIndex + 2].GetComponent<Image>().color = normalColor;
        }
        else
        {
            phaseLabels[ActivePuzzleIndex + 1].GetComponent<Image>().color = normalColor;
        }

        //check if it should save data
        if ((!InTestMode)&&(!InReplayMode))
        {
            string dataToSave;
            Puzzle puzzle = ActiveConfig.puzzles[ActivePuzzleIndex];
            if (!PhaseFinished)//if this was a forced phase finish, instead of time and score "invalid" will be saved
            {
                dataToSave = idInuput.text + "," + ActiveConfig.name + "," + puzzle.name + "," + puzzle.widthpx + "," + puzzle.heigthpx + "," + "invalid" + "," + "invalid";
            }
            else
            {
                //save data (format: id,config,puzzle,w,h,time left,score)
                if (timeLeft <= 0)//if ended because time ran out
                {
                    dataToSave = idInuput.text + "," + ActiveConfig.name + "," + puzzle.name + "," + puzzle.widthpx + "," + puzzle.heigthpx + "," + "max" + "," + skore;
                }
                else//if ended because puzzle was solved
                {
                    dataToSave = idInuput.text + "," + ActiveConfig.name + "," + puzzle.name + "," + puzzle.widthpx + "," + puzzle.heigthpx + "," + (ActiveConfig.timeLimit - timeLeft) + "," + skore;
                }
            }
            try
            {
                if (File.Exists(activeExperiment.resultsFile))
                {
                    using (StreamWriter sw = new StreamWriter(activeExperiment.resultsFile, true))
                    {
                        sw.WriteLine(dataToSave);
                        sw.Close();
                    }
                }
                else
                {
                    ErrorCatcher.instance.Show("Wanted to write a line to results but the file " + activeExperiment.resultsFile + " does not exist.");
                }
            }
            catch(System.Exception e)
            {
                ErrorCatcher.instance.Show("Wanted to write a line to results to file " + activeExperiment.resultsFile + " but it threw error "+e.ToString());
            }
        }

        //finish
        CurrentPuzzle.EndPhase();
        BasicFinish();

        //reset timer
        timeLeft = 0;
        countingDown = false;
        timerText.text = "0:00.0";
    }


    /// <summary>
    /// basic stuff done after finishing any kind of phase
    /// </summary>
    private void BasicFinish()
    {
        //clear other info
        scoreText.text = "0";
        PhaseFinished = false;
        nextButton.image.color = normalColor;
        skore = 0;
    }





    //--------------------------------------------------------------------------------------------------------------------------
    // SWITCHING PHASES
    //--------------------------------------------------------------------------------------------------------------------------

    
    /// <summary>
    /// if current phase is marked as finished or <paramref name="skipCondition"/>=true this sitches to next phase, othervise creates a popup panle "Do you really want to skip?"
    /// </summary>
    /// <param name="skipCondition">true = do not check if phase is finished, just switch to the next one</param>
    public void TrySwitchPhase(bool skipCondition)
    {
        if (InStart && (!InTestMode)&&(!InReplayMode))//in start phase only, check if playerID is filled and unique
        {
            if ((!ContainsWhitespaceOnly(idInuput.text))&& IsValidForCsv(idInuput.text) && (!activeExperiment.ids.Contains(idInuput.text)))
            {
                messageOutput.text = "";
                playeridInputField.interactable = false;
            }
            else
            {
                messageOutput.text = "Player ID must be filled, must not contain commas or quotes and must be unique!!!";
                return;
            }
        }
        //try switch phase
        if (skipCondition || PhaseFinished)
        {
            StartCoroutine(SwitchPhase());
        }
        else
        {
            popupPanel.SetActive(true);
        }
    }



    /// <summary>
    /// correctly finishes current phase, waits a few seconds and then starts the next phase
    /// </summary>
    /// <returns></returns>
    private IEnumerator SwitchPhase()
    {
        Switching = true;
        phaseLoadingPanel.SetActive(true);//prevents experimentor from clicking any buttons during the switching

        //logging
        if (!InReplayMode)
            Logger.instance.Log(Time.time + " Next");

        if (InStart)
        {
            //finish
            if ((!InTestMode)&&(!InReplayMode))
            {
                activeExperiment.ids.Add(idInuput.text);
            }
            FinishStartPhase();

            //wait
            yield return new WaitForSeconds(1);

            //start next
            if (ActiveConfig.withTutorial)
            {
                StartTutorial();
            }
            else
            {
                ActivePuzzleIndex = -1;
                if (ActiveConfig.puzzles.Count == 1)
                {
                    nextButton.GetComponentInChildren<Text>().text = "SAVE RESULT";
                }
                PhaseStart();
            }
        }
        else
        {
            if(InTut)
            {
                //finish
                FinishTutorial();

                //wait
                yield return new WaitForSeconds(1);

                //start next
                ActivePuzzleIndex = -1;
                if (ActiveConfig.puzzles.Count==1)
                {
                    nextButton.GetComponentInChildren<Text>().text = "SAVE RESULT";
                }
                PhaseStart();
            }
            else
            {
                if (ActivePuzzleIndex == ActiveConfig.puzzles.Count - 1)//if just finished the last puzzle
                {
                    //finish
                    PhaseFinish();

                    //wait
                    yield return new WaitForSeconds(2);

                    //start next
                    if (InReplayMode)
                    {
                        phaseLoadingPanel.SetActive(false);
                        yield break;//zastav prehravani (jinak by se to soatalo do startu a tam se prehravani spustilo znova)
                    }
                    else
                    {
                        StartStart();
                    }
                }
                else//if just finished any other puzzle
                {
                    //finish
                    PhaseFinish();

                    //wait
                    yield return new WaitForSeconds(1);

                    //start next
                    if (ActivePuzzleIndex == ActiveConfig.puzzles.Count - 2)
                    {
                        nextButton.GetComponentInChildren<Text>().text = "SAVE RESULT";
                    }
                    PhaseStart();
                }
            }
        }
        phaseLoadingPanel.SetActive(false);//allow the experimentor to use GUI again
        Switching = false;
    }





    //--------------------------------------------------------------------------------------------------------------------------
    // HELPER FUNTIONS
    //--------------------------------------------------------------------------------------------------------------------------


    /// <summary>
    /// checks if <paramref name="s"/> contains whitespace characters only
    /// </summary>
    /// <param name="s">checked string</param>
    /// <returns>true = contains only whitespace characters</returns>
    private bool ContainsWhitespaceOnly(string s)
    {
        foreach (char c in s)
        {
            if (!char.IsWhiteSpace(c))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// checks if <paramref name="s"/> does not contain commas and quotation marks
    /// </summary>
    /// <param name="s">checked strind</param>
    /// <returns>true = does not contain forbidden chars</returns>
    private bool IsValidForCsv(string s)
    {
        foreach (char c in s)
        {
            if (c==','||c=='"')
            {
                return false;
            }
        }
        return true;
    }






    //--------------------------------------------------------------------------------------------------------------------------
    // UPDATING DURING A PHASE & PUBLIC FUNCTIONS
    //--------------------------------------------------------------------------------------------------------------------------

    private void Update()
    {
        if (countingDown && (!PhaseFinished))
        {
            //show timer
            int minutes = (int)timeLeft / 60;
            float seconds = timeLeft % 60;
            timerText.text = string.Format("{0:00}:{1:00.0}", minutes, seconds);

            //check timer
            if (timeLeft <= 0)
            {
                CurrentPuzzle.OnTimerStop();//puzzle specific action
                countingDown = false;
                PhaseFinished = true;
                nextButton.image.color = new Color(1, 0.5f, 0);
            }
            //update timer
            timeLeft -= Time.deltaTime;
        }

    }

    /// <summary>
    /// decreases score by one (if current phase is not finished yet)
    /// </summary>
    public void DecreaseScore()
    {
        if (!PhaseFinished)
        {
            skore--;
            scoreText.text = skore.ToString();
        }
    }

    /// <summary>
    /// increases score by one (if current phase is not finished yet)
    /// </summary>
    public void IncreaseScore()
    {
        if (!PhaseFinished)
        {
            skore++;
            scoreText.text = skore.ToString();
        }
    }

    /// <summary>
    /// marks current phase as finished
    /// </summary>
    public void SetPhaseComplete()
    {
        if (!PhaseFinished)
        {
            PhaseFinished = true;
            nextButton.image.color = highlightColor;
        }
    }

    /// <summary>
    /// sets the content of the wall picture
    /// </summary>
    /// <param name="t">the image to display</param>
    public void SetWallPicture(Texture t)
    {
        modelPictureFrame.material.mainTexture = t;
    }

    /// <summary>
    /// returns wall picture to its original size
    /// </summary>
    public void ResetWallPictureSize()
    {
        imageHolder.localScale = originalScale;
    }

    /// <summary>
    /// scales the wall picture
    /// </summary>
    /// <param name="x">multiplicator for the x axis</param>
    /// <param name="y">multiplicator for the y axis</param>
    public void MultiplyWallpictureScale(float x, float y)
    {
        imageHolder.localScale = originalScale;
        imageHolder.localScale = new Vector3(imageHolder.localScale.x * x, imageHolder.localScale.y * y, 0.2f);
    }

    /// <summary>
    /// cancels running experiment and returns back to menu
    /// </summary>
    public void OnQuitClicked()
    {
        playeridInputField.text = "";
        StopAllCoroutines();

        if (InStart)
        {
            FinishStartPhase();
        }
        else
        {
            if (InTut)
            {
                FinishTutorial();
            }
            else
            {
                PhaseFinish();
            }
        }

        FinishCurrentConfig();
        FinishExperiment();
        Logger.instance.StopLogger();
        UImangr.MakeSureTimeIsntStopped();

        MenuLogic.instance.spectatorCanvas.SetActive(false);
        MenuLogic.instance.chooseMenuCanvas.SetActive(true);
    }





    //--------------------------------------------------------------------------------------------------------------------------
    // THINGS FOR THE NPC
    //--------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// creates a NPC from <paramref name="model"/> and <paramref name="bahaviour"/>
    /// </summary>
    /// <param name="model"></param>
    /// <param name="bahaviour"></param>
    private void CreateCharacter(NpcModel model, NpcBeahviour bahaviour)
    {
        GameObject npc = Instantiate(model.modelObject, npcPoint.position, npcPoint.rotation);
        npc.GetComponent<Animator>().runtimeAnimatorController = bahaviour.behaviourAnimController as RuntimeAnimatorController;
        theNpc = npc;
        npcName = model.modelName;
    }

    /// <summary>
    /// destroyes currently used NPC
    /// </summary>
    private void DestroyCharacter()
    {
        Destroy(theNpc);
        theNpc = null;
        npcName = "";
    }

}



/// <summary>
/// class attaching a name to a NPC model, which is used for selecting the model in menu
/// </summary>
[System.Serializable]
public class NpcModel
{
    public string modelName;
    public GameObject modelObject;
}



/// <summary>
/// class attaching a name to a NPC behaviour (NPC animator), which is used for selecting the behaviour in menu
/// </summary>
[System.Serializable]
public class NpcBeahviour
{
    public string bahaviourName;
    public RuntimeAnimatorController behaviourAnimController;
}



/// <summary>
/// class for WelcomeSpeechBehaviour containing info abou one line o text
/// </summary>
[System.Serializable]
public class Sentence
{ 
    //+++++++++++++++++++++++++++++++++++++podivne komentare!!!
    public string text;///<the sentence
    public bool replaceAlex;///<if true, replace "Alex" in the sentence by the name of currently used NPC
    public AudioClip audio;///<optional, used only if useSound==true in WelcomeSpeechBehaviour
}
