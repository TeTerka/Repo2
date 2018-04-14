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
    /// <summary>list of all existing puzzle types</summary>
    public List<AbstractPuzzle> puzzleTypes = new List<AbstractPuzzle>();


    //information about current state:
    private Experiment activeExperiment = null;

    /// <summary>which puzzle type is currently being used</summary>
    public AbstractPuzzle CurrentPuzzle { get; private set; }

    /// <summary>which configuration is currently being used</summary>
    public Configuration ActiveConfig { get; private set; }

    /// <summary>which puzzle phase are we currently in (-1 if in start or tutorial phase)</summary>
    public int ActivePuzzleIndex { get; private set; }

    /// <summary>true = we are now in start phase</summary>
    public bool InStart { get; private set; }

    /// <summary>true = we are now in tutorial phase</summary>
    public bool InTut { get; private set; }

    /// <summary>true = current phase was already finished</summary>
    public bool PhaseFinished { get; private set; }

    /// <summary>true = we are only in test mode (no data will be saved)</summary>
    public bool InTestMode { get; private set; }

    /// <summary>true = we are in replay mode (no control available, only reads the log file)</summary>
    public bool InReplayMode { get; private set; }

    /// <summary>true = we are currently between two phases</summary>
    public bool Switching { get; private set; }

    [Header("model picture")]
    [SerializeField] private Renderer modelPictureFrame;
    [SerializeField] private Transform imageHolder;

    [Header("room scale stuff")]
    [SerializeField] private Transform level;
    [SerializeField] private Transform cameraRigPoint;
    [SerializeField] private Transform editorFloorScale;
    private Vector3 originalScale = new Vector3();

    [Header("referencies to UI")]
    [SerializeField] private UImanagerScript UImangr;
    [SerializeField] private Text timerText;
    private float timeLeft; //in seconds
    private bool countingDown = false;
    [SerializeField] private Text scoreText;
    private int skore;

    //next button
    [SerializeField] private Button nextButton;
    private Color highlightColor = Color.green;
    private Color normalColor = Color.white;

    //other UI
    [SerializeField] private Text idInuput;
    [SerializeField] private Text messageOutput;
    [SerializeField] private InputField playeridInputField;
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private GameObject phaseLoadingPanel;
    [SerializeField] private GameObject testModeHighlight;
    [SerializeField] private Text expNameText;
    [SerializeField] private Button pauseButton;

    //scrollview UI
    [SerializeField] private GameObject phaseScrollContent;
    [SerializeField] private GameObject configScrollContent;
    [SerializeField] private GameObject namePlatePrefab;
    private List<GameObject> phaseLabels = new List<GameObject>();
    private List<Button> configButtons = new List<Button>();
    [SerializeField] private Button configNameButtonPrafab;

    [Header("for changing agent models and behaviours")]
    [SerializeField] private Transform npcPoint;
    /// <summary>lis of all available npc models</summary>
    public List<NpcModel> npcModels = new List<NpcModel>();
    /// <summary>lis of all available npc behaviours</summary>
    public List<NpcBeahviour> npcBehaviours = new List<NpcBeahviour>();

    /// <summary>reference to the currently used virtual agent</summary>
    public GameObject TheNpc { get; private set; }

    [Header("for talking during animation")]
    /// <summary>reference to a panel for displaying subtitles</summary>
    public GameObject subtitlesCanvas;
    /// <summary>used by SpeechBehaviour, -1 means noone needs the subtitleCanvas</summary>
    public int whoWantsSubtitles = -1;
    /// <summary>name of currently used npc model</summary>
    public string NpcName { get; private set; }


    //--------------------------------------------------------------------------------------------------------------------------
    // INITIALIZATION
    //--------------------------------------------------------------------------------------------------------------------------

    private void Awake()
    {
        if (!UnityEngine.VR.VRDevice.isPresent)
        {
            ErrorCatcher.instance.Show("No VR device present!");
            return;
        }
        
        //singleton stuff
        if (instance != null)
        {
            Debug.Log("Multiple Managers in one scene!");
        }
        instance = this;

        //check correctness of possible new npcModels, npcBehaviours or puzzleTypes
        CheckPossibleAddedComponents();

        //prepare room
        ScaleRoomToFitPlayArea();

        //inicialize
        modelPictureFrame.material.mainTexture = null;
        modelPictureFrame.material.color = Color.white;
        InTestMode = false;
        InReplayMode = false;
    }

    /// <summary>
    /// scale the virtual room so that the part where player should walk is the same size as the real room
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
        imageHolder.localScale = new Vector3(1 / imageHolder.lossyScale.x, 1, 1 / imageHolder.lossyScale.z);
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

    /// <summary>
    /// checks if there is any problematic new puzzle type or npc model or npc behaviour
    /// (puzzle types must have unique names, npc models must have component Animator, npc behaviours must have triggers StartStart, StartTutorial and StartNewPuzzle)
    /// </summary>
    private void CheckPossibleAddedComponents()
    {
        //new puzzle types
        List<string> puzzleTypesNames = new List<string>();
        foreach (AbstractPuzzle type in puzzleTypes)
        {
            puzzleTypesNames.Add(type.TypeName);
        }
        if (HasDuplicates(puzzleTypesNames)) ErrorCatcher.instance.Show("Program contains two puzzle types with the same typeName!");

        //new npc models
        foreach (NpcModel model in npcModels)
        {
            if(model.modelObject.GetComponent<Animator>()==null) ErrorCatcher.instance.Show("One of the npc models does not have Animator component!");
        }

        //new npc behaviours
        GameObject emptyObjectForTesting = new GameObject();
        Animator a = emptyObjectForTesting.AddComponent<Animator>();
        foreach (NpcBeahviour behaviour in npcBehaviours)
        {
            a.runtimeAnimatorController = behaviour.behaviourAnimController;
            if (!HasTriggerParam("StartStart", a)) ErrorCatcher.instance.Show("One of the npc behaviours is missing StartStart trigger!");
            if (!HasTriggerParam("StartTutorial", a)) ErrorCatcher.instance.Show("One of the npc behaviours is missing StartTutorial trigger!");
            if (!HasTriggerParam("StartNewPuzzle", a)) ErrorCatcher.instance.Show("One of the npc behaviours is missing StartNewPuzzle trigger!");
        }
        Destroy(emptyObjectForTesting);
    }
    private bool HasDuplicates<T>(List<T> subjects)
    {
        var set = new HashSet<T>();
        foreach (var s in subjects)
            if (!set.Add(s))
            {
                return true;
            }
        return false;
    }
    private bool HasTriggerParam(string parName, Animator anim)
    {
        foreach (AnimatorControllerParameter p in anim.parameters)
        {
            if (p.name == parName && p.type==AnimatorControllerParameterType.Trigger)
                return true;
        }
        return false;
    }



    //--------------------------------------------------------------------------------------------------------------------------
    // STARTING & FINISHING EXPERIMENTS & CONFIGURATIONS
    //--------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// start experiement <paramref name="e"/> in selected mode
    /// </summary>
    /// <param name="e">starting experiment</param>
    /// <param name="justTesting">true = test mode (no data saving)</param>
    /// <param name="justReplay">true = replay mode (replaying from a logfile)</param>
    public void StartExperiment(Experiment e,bool justTesting,bool justReplay)
    {
        messageOutput.text = "";
        InTestMode = justTesting;
        testModeHighlight.SetActive(InTestMode);
        InReplayMode = justReplay;
        activeExperiment = e;
        expNameText.text = e.name;

        //decide which puzzle type is being used
        CurrentPuzzle = null;
        foreach (AbstractPuzzle puzzleType in puzzleTypes)
        {
            if (puzzleType.TypeName == e.puzzleType)
                CurrentPuzzle = puzzleType;
        }
        if(CurrentPuzzle == null)
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

        //prepare the virtual agent
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
            TheNpc = null;
        }

        //puzzle specific actions
        CurrentPuzzle.StartConfig(c);

        originalScale = imageHolder.localScale;
        //go to the start phase of this configuration
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

        //animation of the virtal agent
        if (TheNpc != null)
        {
            TheNpc.GetComponent<Animator>().SetTrigger("StartStart");
        }

        if (InReplayMode)
        {
            StartCoroutine(SwitchPhase());
        }
    }

    /// <summary>
    /// ends start phase
    /// </summary>
    private void FinishStartPhase()
    {
        //logging - creating a new logfile
        if ((!InReplayMode) && (!InTestMode) && (idInuput.text != ""))
        {
            string newPath = Application.dataPath + activeExperiment.resultsFile.Substring(0, activeExperiment.resultsFile.LastIndexOf('/') + 1) + "/" + ActiveConfig.name + "_" + idInuput.text + ".mylog";
            try
            {
                var f = File.Create(newPath); f.Close();
                using (StreamWriter sw = new StreamWriter(newPath, true))
                {
                    sw.WriteLine(ActiveConfig.name);
                    sw.WriteLine(idInuput.text);
                    sw.WriteLine(Time.time.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    sw.Close();
                }
            }
            catch (System.Exception e)
            {
                ErrorCatcher.instance.Show("Wanted to create new logfile " + newPath + " but it threw error " + e.ToString());
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
        if (TheNpc != null)
        {
            TheNpc.GetComponent<Animator>().SetTrigger("StartTutorial");
        }
    }

    /// <summary>
    /// ends tutorial phase
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
        if(TheNpc!=null)
            TheNpc.GetComponent<Animator>().SetTrigger("StartNewPuzzle");

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
            PuzzleData puzzle = ActiveConfig.puzzles[ActivePuzzleIndex];
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
                if (File.Exists(Application.dataPath + activeExperiment.resultsFile))
                {
                    using (StreamWriter sw = new StreamWriter(Application.dataPath + activeExperiment.resultsFile, true))
                    {
                        sw.WriteLine(dataToSave);
                        sw.Close();
                    }
                }
                else
                {
                    ErrorCatcher.instance.Show("Wanted to write a line to results but the file " + Application.dataPath + activeExperiment.resultsFile + " does not exist.");
                }
            }
            catch(System.Exception e)
            {
                ErrorCatcher.instance.Show("Wanted to write a line to results to file " + Application.dataPath + activeExperiment.resultsFile + " but it threw error "+e.ToString());
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
    /// if current phase is marked as finished or <paramref name="skipCondition"/>=true this switches to next phase, othervise creates a popup panle "Do you really want to skip?"
    /// </summary>
    /// <param name="skipCondition">true = do not check if phase is finished, just switch to the next one</param>
    public void TrySwitchPhase(bool skipCondition)
    {
        if (InStart && (!InTestMode)&&(!InReplayMode))//in start phase only, check if playerID is filled, valid and unique
        {
            if (MenuLogic.instance.IsValidName(idInuput.text) && (!activeExperiment.ids.Contains(idInuput.text)))
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
            Logger.instance.Log(Time.time.ToString(System.Globalization.CultureInfo.InvariantCulture) + " Next");

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
                    PhaseStart();
                }
            }
        }
        phaseLoadingPanel.SetActive(false);//allow the experimentor to use GUI again
        Switching = false;
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
    // THINGS FOR THE VIRTUAL AGENT
    //--------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// creates a virtual agent from <paramref name="model"/> and <paramref name="bahaviour"/>
    /// </summary>
    /// <param name="model"></param>
    /// <param name="bahaviour"></param>
    private void CreateCharacter(NpcModel model, NpcBeahviour bahaviour)
    {
        GameObject npc = Instantiate(model.modelObject, npcPoint.position, npcPoint.rotation);
        npc.GetComponent<Animator>().runtimeAnimatorController = bahaviour.behaviourAnimController as RuntimeAnimatorController;
        TheNpc = npc;
        NpcName = model.modelName;
    }

    /// <summary>
    /// destroyes currently used virtual agent
    /// </summary>
    private void DestroyCharacter()
    {
        Destroy(TheNpc);
        TheNpc = null;
        NpcName = "";
    }

}

