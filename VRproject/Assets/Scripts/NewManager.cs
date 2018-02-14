using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class NewManager : MonoBehaviour {

    public bool Switching { get; private set; }
    
    //sigleton stuff
    public static NewManager instance;

    [Header("puzzle types")]
    public AbstractPuzzle cubePuzzle;
    public AbstractPuzzle pipePuzzle;

    //info o stavu
    public AbstractPuzzle CurrentPuzzle { get; private set; }

    private Experiment activeExperiment = null;
    public Configuration ActiveConfig { get; private set; }
    public int ActivePuzzleIndex { get; private set; }
    public bool InStart { get; private set; }
    public bool InTut { get; private set; }
    public bool PhaseFinished { get; private set; }

    public bool InTestMode { get; private set; }
    public bool InReplayMode { get; private set; }

    [Header("model picture")]
    public Renderer modelPictureFrame;
    public Transform imageHolder;

    //room scale stuff
    [Header("room scale stuff")]
    public Transform level;
    public Transform cameraRigPoint;
    public Transform editorFloorScale;
    private Vector3 originalScale = new Vector3();

    //odpocet a score
    [Header("referencies to UI")]
    public Text timerText;
    private float timeLeft; //in seconds
    private bool countingDown = false;
    public Text scoreText;
    private int skore;

    //next button
    public Button nextButton;//the NEXT button will be hihghlighted when curret phase is finished
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

    //scrollview ui
    public GameObject phaseScrollContent;
    public GameObject configScrollContent;
    public GameObject namePlatePrefab;
    private List<GameObject> phaseLabels = new List<GameObject>();
    private List<Button> configButtons = new List<Button>();
    public Button configNameButtonPrafab;

    [Header("for changing NPC models and behaviours")]
    public GameObject theNpc;
    public Transform npcPoint;
    public List<NpcModel> npcModels = new List<NpcModel>();//vsechny musi mit komponentu Animator!
    public List<NpcBeahviour> npcBehaviours = new List<NpcBeahviour>();

    [Header("Animation stuff")]//pouziva to WelcomeSpeechBehaviour...
    public string npcName = "";
    public GameObject subtitlesCanvas;
    public int whoWantsSubtitles = -1;

    public UImanagerScript UImangr;


    //***********************************************THE VERY BEGINNING********************************************************************
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

        //inicializace
        modelPictureFrame.material.mainTexture = null;
        modelPictureFrame.material.color = Color.white;
        InTestMode = false;
        InReplayMode = false;
    }

    private void ScaleRoomToFitPlayArea()//scale the virtual room so that it is the same size as the real room
    {
        //get info from play area
        Valve.VR.HmdQuad_t rect = new Valve.VR.HmdQuad_t();

        //tady to chce nejaky wait, aby se rozmery mistnosti stihly nacist....muze to chvilku trvat..
        while (!SteamVR_PlayArea.GetBounds(SteamVR_PlayArea.Size.Calibrated, ref rect))
        {
            System.Threading.Thread.Sleep(1000);
        }

        //count new scale
        float floorScaleFactorX = editorFloorScale.localScale.x;//sirka podlahy v editoru
        float floorScaleFactorZ = editorFloorScale.localScale.z/2;//delka podlahy v editoru kam dosahuje camera rig        
        Vector3 newScale = new Vector3(Mathf.Abs(rect.vCorners0.v0 - rect.vCorners2.v0)/ floorScaleFactorX, 1,Mathf.Abs(rect.vCorners0.v2 - rect.vCorners2.v2)/ floorScaleFactorZ);
        //scale the room
        level.localScale = newScale;
        //move camera rig to original position
        this.transform.position = cameraRigPoint.position;
        //save this scale of model picture so that puzzles can change it but they also can go back to this size
        originalScale = imageHolder.localScale;

        //adjust scale to objects which should keep original size
        foreach (Transform x in cubePuzzle.nonScalables)
        {
            x.localScale = new Vector3(1 / x.lossyScale.x, 1, 1 / x.lossyScale.z);
        }

        foreach (Transform x in pipePuzzle.nonScalables)
        {
            x.localScale = new Vector3(1 / x.lossyScale.x, 1, 1 / x.lossyScale.z);
        }
    }

    //************************************** STARTING & FINISHING EXPERIMENTS & CONFIGURATIONS *****************************************************

    public void StartExperiment(Experiment e,bool justTesting,bool justReplay)
    {
        InTestMode = justTesting;
        testModeHighlight.SetActive(InTestMode);
        InReplayMode = justReplay;//*****************************

        activeExperiment = e;
        expNameText.text = e.name;
        //decide which puzzle is being used
        if (e.puzzleType == "CubePuzzle")
            CurrentPuzzle = cubePuzzle;
        if (e.puzzleType == "PipePuzzle")
            CurrentPuzzle = pipePuzzle;
        //fill out the configs scrollview
        for (int i = 0; i < e.configs.Count; i++)
        {
            int iForDelegate = i;
            var p = Instantiate(configNameButtonPrafab, configScrollContent.transform);
            p.GetComponentInChildren<Text>().text = e.configs[i].name;
            configButtons.Add(p);
            p.onClick.AddListener(delegate { if (InStart) { FinishStartPhase(); FinishCurrentConfig(); StartConfig(e.configs[iForDelegate]); } });
        }

        //*******************
        if(InReplayMode)//deaktivuj nezadouci ovladaci prvky
        {
            SetControlsInteractible(false);
        }
        else
        {
            SetControlsInteractible(true);
        }
        //*******************

        //start the first configuration (obviously every experiment must have at least one)
        StartConfig(e.configs[0]);
    }
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

    private void FinishExperiment()//should be called only after current configuration was correctly finished
    {
        //clear the configButtons
        for (int i = configButtons.Count - 1; i >= 0; i--)
        {
            Destroy(configButtons[i].gameObject);
            configButtons.RemoveAt(i);
        }
        //...anything else?
        activeExperiment = null;
        expNameText.text = "";
        CurrentPuzzle = null;
    }

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
            //search for the chosen model and behavior (by name), then call CreateCharacter
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

        CurrentPuzzle.StartConfig(c);
        originalScale = imageHolder.localScale;//urcite....??
        StartStart();
    }

    private void FinishCurrentConfig()//can be called only after current phase was correctly finished
    {

        CurrentPuzzle.FinishConfig();

        //clear the phase scrollview...
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

    //********************************************************* PHASES **********************************************************************

    private void PhaseStart()
    {
        ActivePuzzleIndex++;
        //visualise current phase
        if (ActiveConfig.withTutorial)
        {
            phaseLabels[ActivePuzzleIndex + 2].GetComponent<Image>().color = Color.green;
        }
        else
        {
            phaseLabels[ActivePuzzleIndex + 1].GetComponent<Image>().color = Color.green;
        }

        CurrentPuzzle.StartPhase();

        //start timer
        timeLeft = ActiveConfig.timeLimit;
        countingDown = true;

        //dale by melo spustit spravnou animaci
        if(theNpc!=null)
            theNpc.GetComponent<Animator>().SetTrigger("StartNewPuzzle");

    }
    private void PhaseFinish()
    {
        //cancel phase label highlight
        if (ActiveConfig.withTutorial)
        {
            phaseLabels[ActivePuzzleIndex + 2].GetComponent<Image>().color = Color.white;
        }
        else
        {
            phaseLabels[ActivePuzzleIndex + 1].GetComponent<Image>().color = Color.white;
        }

        //check if it should save data
        if ((!InTestMode)&&(!InReplayMode))
        {
            string dataToSave;
            if (!PhaseFinished)//to se stane kdyz nekdo klikne na Yes v popup Opravdu chcete pokracovat? (ulozi se to, ale misto casu a score tam bude "invalid")
            {
                dataToSave = idInuput.text + "," + ActiveConfig.name + "," + ActiveConfig.puzzles[ActivePuzzleIndex].name + "," + ActiveConfig.puzzles[ActivePuzzleIndex].widthpx + "," + ActiveConfig.puzzles[ActivePuzzleIndex].heigthpx + "," + "invalid" + "," + "invalid";
            }
            else
            {
                //save data (ve tvaru: id,config,puzzle,w,h,time left,score)
                if (timeLeft <= 0)//pokud skoncil protoze mu dosel cas
                {
                    dataToSave = idInuput.text + "," + ActiveConfig.name + "," + ActiveConfig.puzzles[ActivePuzzleIndex].name + "," + ActiveConfig.puzzles[ActivePuzzleIndex].widthpx + "," + ActiveConfig.puzzles[ActivePuzzleIndex].heigthpx + "," + "max" + "," + skore;
                }
                else//pokud skoncil tim ze dokoncil puzzle
                {
                    dataToSave = idInuput.text + "," + ActiveConfig.name + "," + ActiveConfig.puzzles[ActivePuzzleIndex].name + "," + ActiveConfig.puzzles[ActivePuzzleIndex].widthpx + "," + ActiveConfig.puzzles[ActivePuzzleIndex].heigthpx + "," + (ActiveConfig.timeLimit - timeLeft) + "," + skore;
                }
            }
            try
            {
                if (File.Exists(activeExperiment.resultsFile))
                {
                    using (StreamWriter sw = new StreamWriter(activeExperiment.resultsFile, true))//true for append
                    {
                        sw.WriteLine(dataToSave);
                        sw.Close();
                    }
                }
                else
                {
                    //Debug.Log("error, missing results file!!!");
                    ErrorCatcher.instance.Show("Wanted to write a line to results but the file " + activeExperiment.resultsFile + " does not exist.");
                    //tady by se melo vsechno nejak pauznout, vypnout....
                }
            }
            catch(System.Exception e)
            {
                ErrorCatcher.instance.Show("Wanted to write a line to results to file " + activeExperiment.resultsFile + " but it threw error "+e.ToString());
            }
        }

        CurrentPuzzle.EndPhase();
        BasicFinish();

        //reset timer
        timeLeft = 0;
        countingDown = false;
        timerText.text = "0:00.0";
    }

    public void StartStart()//sets up the start phase
    {
        //**************************************
        if (!InReplayMode)
        {
            Logger.instance.SetLoggerPath(null);
        }
        //******************************************

        //general preparations...
        InStart = true;
        phaseLabels[0].GetComponent<Image>().color = Color.green;
        nextButton.GetComponentInChildren<Text>().text = "NEXT PHASE";

        //puzzle specific preparation
        CurrentPuzzle.StartStart();

        if (!InReplayMode)//pri prehravani nechci zaktivnovat zadne ovladaci prvky krome quit a changeCam buttonu
        {
            //make it possible to switch to different configuration
            foreach (var item in configButtons)
            {
                item.interactable = true;
            }
            //...and to change id
            playeridInputField.interactable = true;
        }

        //animace NPC
        if (theNpc != null)
        {
            theNpc.GetComponent<Animator>().SetTrigger("TabletUp");
            theNpc.GetComponent<Animator>().SetTrigger("StartStart");
        }

        if(InReplayMode)//****************
        {
            StartCoroutine(SwitchPhase());
        }
    }

    public void FinishStartPhase()
    {
        //***************logging*********************************
        if ((!InReplayMode)&&(!InTestMode)&&(idInuput.text!=""))
        {
            //tady by mohlo vadit, kdyby se dve konfigurace jmenovaly stejne!!!!!!!!!!!!!!!!!!!!!
            string newPath = activeExperiment.resultsFile.Substring(0, activeExperiment.resultsFile.LastIndexOf('/') + 1) + "/" + ActiveConfig.name + "_" + idInuput.text + ".mylog";
            try
            {
                var f = File.Create(newPath); f.Close();//urcite takovy jeste neexistuje, protoze je zajisteno, ze id jsou v ramci jednoho experimentu unikatni
                using (StreamWriter sw = new StreamWriter(newPath, true))
                {
                    sw.WriteLine(ActiveConfig.name);
                    sw.WriteLine(idInuput.text);
                    sw.WriteLine(Time.time);
                    sw.Close();
                }
            }
            catch(System.Exception e)
            {
                ErrorCatcher.instance.Show("Wanted to create new logfile " + activeExperiment.resultsFile + " but it threw error " + e.ToString());
            }
            Logger.instance.SetLoggerPath(newPath);
        }

        if(InReplayMode)
        {
            Logger.instance.ReadLog();//zacni prehravat
        }
        //*******************************************************


        phaseLabels[0].GetComponent<Image>().color = Color.white;
        InStart = false;

        //disable the option to switch to different configuration
        foreach (var item in configButtons)
        {
            item.interactable = false;
        }

        CurrentPuzzle.EndStart();

        BasicFinish();

    }

    public void StartTutorial()//sets up tutorial phase - ADD OPTION TO SELECT YOUR OWN TUTORIAL PICTURE ETC. !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    {
        //general preparations...
        InTut = true;
        phaseLabels[1].GetComponent<Image>().color = Color.green;

        CurrentPuzzle.StartTut();

        //animace NPC
        if (theNpc != null)
        {
            theNpc.GetComponent<Animator>().SetTrigger("TabletDown");
            theNpc.GetComponent<Animator>().SetTrigger("StartWelcomeSpeech");
        }
    }

    public void FinishTutorial()
    {
        phaseLabels[1].GetComponent<Image>().color = Color.white;
        InTut = false;

        CurrentPuzzle.EndTut();
        BasicFinish();
    }

    public void BasicFinish()
    {
        //imageHolder.localScale = originalScale;?

        //clear other info
        scoreText.text = "0";
        PhaseFinished = false;
        nextButton.image.color = normalColor;
        skore = 0;
    }

    //*****************************************SWITCHING********************************************************

    public void TrySwitchPhase(bool skipCondition)
    {
        //specialne pro welcome phase - kontrola jestli bylo zadano unikatni playerID
        if (InStart && (!InTestMode)&&(!InReplayMode))
        {
            if ((!ContainsWhitespaceOnly(idInuput.text))&& IsValidForCsv(idInuput.text) && (!activeExperiment.ids.Contains(idInuput.text)))//je neprazdny a je unikatni
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
        if (skipCondition || PhaseFinished)//tady bude kontrola, jestli hrac splnil ukol teto faze, pokud ne, program se zepta, jestli opravdu chceme pokracovat
        {
            StartCoroutine(SwitchPhase());
        }
        else
        {
            //popup window
            popupPanel.SetActive(true);
        }
    }

    //mezi fazemi chvilku cekat....hlavne na konci main phase, aby mohl v klidu sundat headset...
    private IEnumerator SwitchPhase()//(public jen kvuli loggeru)
    {
        Switching = true;
        phaseLoadingPanel.SetActive(true);//zamezi koordinatorovi na cokoliv klikat v prubehu prepinani faze
                                          //ani hrac v teto dobe nemuze nic pokazit, protoze OnCubePlaced pripadne otoceni trubky probehne jen pokud neni phaseFinished
                                          //a sebranim krychle se nic nezkazi (respawnuji se i z ruky)....a nic dalsiho uz hrac delat neumi, takze ok

        if (!InReplayMode)
            Logger.instance.Log(Time.time + " Next");//*******logging*****************

        if (InStart)//if just finished start phase
        {
            if ((!InTestMode)&&(!InReplayMode))
            {
                activeExperiment.ids.Add(idInuput.text);
            }
            FinishStartPhase();
            yield return new WaitForSeconds(1);
            if (ActiveConfig.withTutorial)
            {
                StartTutorial();
            }
            else
            {
                ActivePuzzleIndex = -1;//od ted zacinaji main puzzly
                if (ActiveConfig.puzzles.Count == 1)//pokud je v aktualni konfiguraci jen jeden puzzle, je to zaroven posledni puzzle
                {
                    nextButton.GetComponentInChildren<Text>().text = "SAVE RESULT";
                }
                PhaseStart();
            }
        }
        else
        {
            if(InTut)//if just finished tutorial phase
            {
                FinishTutorial();
                yield return new WaitForSeconds(1);
                ActivePuzzleIndex = -1;//od ted zacinaji main puzzly, pripravi se prvni puzzle
                if (ActiveConfig.puzzles.Count==1)//pokud je v aktualni konfiguraci jen jeden puzzle, je to zaroven posledni puzzle
                {
                    nextButton.GetComponentInChildren<Text>().text = "SAVE RESULT";
                }
                PhaseStart();
            }
            else
            {
                if (ActivePuzzleIndex == ActiveConfig.puzzles.Count - 1)//if just finished the last puzzle
                {
                    PhaseFinish();
                    yield return new WaitForSeconds(2);

                    if (InReplayMode)//*************************
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
                    PhaseFinish();
                    yield return new WaitForSeconds(1);
                    if (ActivePuzzleIndex == ActiveConfig.puzzles.Count - 2)//pokud nasleduje posledni puzzle...
                    {
                        nextButton.GetComponentInChildren<Text>().text = "SAVE RESULT";
                    }
                    PhaseStart();
                }
            }
        }
        phaseLoadingPanel.SetActive(false);//=>koordinator zase muze volne klikat
        Switching = false;
    }

    //********************************************** POMOCNE FUNKCE ***************************************************************************
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

    private bool IsValidForCsv(string s)//tedy neobsahuje carku (ani uvozovky projistotu)(protoze se to pak bude ukladat do .csv )
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

    //***********************************************UPDATING DURING A PHASE & PUBLIC FUNCTIONS********************************************************************

    private void Update()
    {
        if (countingDown && (!PhaseFinished))//aka if main phase && !finished
        {
            //zobrazeni timeru
            int minutes = (int)timeLeft / 60;
            float seconds = timeLeft % 60;
            timerText.text = string.Format("{0:00}:{1:00.0}", minutes, seconds);

            //kontrola dobehnuti timeru
            if (timeLeft <= 0)
            {
                CurrentPuzzle.OnTimerStop();//puzzle specific action
                countingDown = false;
                PhaseFinished = true;
                nextButton.image.color = new Color(1, 0.5f, 0);
            }
            //update timeru
            timeLeft -= Time.deltaTime;
        }

        //and here maybe some CustomUpdate function (such as fill water maybe?)... currentPuzzle.CustomUpdate();

    }

    public void DecreaseScore()
    {
        if (!PhaseFinished)
        {
            skore--;
            scoreText.text = skore.ToString();
        }
    }
    public void IncreaseScore()
    {
        if (!PhaseFinished)
        {
            skore++;
            scoreText.text = skore.ToString();
        }
    }

    public void SetPhaseComplete()
    {
        if (!PhaseFinished)
        {
            PhaseFinished = true;
            nextButton.image.color = highlightColor;
        }
    }
    public void SetWallPicture(Texture t)
    {
        modelPictureFrame.material.mainTexture = t;
    }
    public void ResetWallPictureSize()
    {
        imageHolder.localScale = originalScale;
    }
    public void MultiplyWallpictureScale(float x, float y)
    {
        imageHolder.localScale = originalScale;
        imageHolder.localScale = new Vector3(imageHolder.localScale.x * x, imageHolder.localScale.y * y, 0.2f);
    }

    public void OnQuitClicked()
    {
        playeridInputField.text = "";//************
        StopAllCoroutines();//************

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
        MenuLogic.instance.spectatorCanvas.SetActive(false);
        MenuLogic.instance.chooseMenuCanvas.SetActive(true);
        //staci to takhle, nebo jeste nejake specialni chovani....

        //******************************************************
        Logger.instance.StopLogger();
        UImangr.MakeSureTimeIsntStopped();
    }

    //********************************************************** NPC stuff ***************************************************************
    private void CreateCharacter(NpcModel model, NpcBeahviour bahaviour)//instanciuje zvoleny model npc a pripoji k nemu zvoleny animator (chovani)
    {
        GameObject npc = Instantiate(model.modelObject, npcPoint.position, npcPoint.rotation);
        npc.GetComponent<Animator>().runtimeAnimatorController = bahaviour.behaviourAnimController as RuntimeAnimatorController;
        theNpc = npc;
        npcName = model.modelName;
    }
    private void DestroyCharacter()
    {
        Destroy(theNpc);
        theNpc = null;
        npcName = "";
    }

}

//krome toho ze je to model, musi to mit jeste nejake jmeno, podle ktereho se model vybere v menu create config (stejne tak behaviour)
[System.Serializable]
public class NpcModel
{
    public string modelName;
    public GameObject modelObject;
}
[System.Serializable]
public class NpcBeahviour
{
    public string bahaviourName;
    public RuntimeAnimatorController behaviourAnimController;
}

[System.Serializable]
public class Sentence//pro WelcomeSpeechBehaviour
{
    public string text;//veta
    public bool replaceAlex;//if true, nahradi vyskyty "Alex" ve vete jmenem aktualne vybraneho NPC
    public AudioClip audio;//optional, pouzije se pouze pokud celkove bude useSound==true v tom konretnim behaviouru
}
