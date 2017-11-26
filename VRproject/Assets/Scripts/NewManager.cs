using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewManager : MonoBehaviour {

    //sigleton stuff
    public static NewManager instance;

    ////headset mode / debug mode (VR stuff)
    //[Header("headset mode / debug mode")]
    //public GameObject VRavailable;
    //public GameObject VRunavailable;

    //kontainery 
    [Header("containers")]
    public GameObject containerPrefab;
    public GameObject containersHolder;
    public GameObject center;//center of the table
    [Header("this shoul be hidden somehow...")]
    public List<TileContainer> containerList = new List<TileContainer>();

    //pucliky (sebratelne dilky)
    [Header("tiles")]
    public GameObject tilePrafab;
    public List<Transform> spawnPoints = new List<Transform>();//seznam napevno urcenych spawnpointu (ty pod stolem)
    public GameObject tileHolder;
    [Header("this shoul be hidden somehow...")]
    public float tileSize = 0.15f;
    private List<GameObject> tileList = new List<GameObject>();

    private List<List<Texture2D>> texturesForCubes = new List<List<Texture2D>>();

    //co se ma skladat
    [Header("puzzle to solve")]
    public Renderer modelPictureFrame;
    [Header("this shoul be hidden somehow...")]
    public int modelPictureNumber;//bude vzdy nula....

    //room scale stuff
    [Header("room scale stuff.")]
    public SteamVR_PlayArea VRplayArea;
    public Transform level;
    public Transform cameraRigPoint;
    public GameObject theNpc;
    public Transform npcPoint;
    public Transform imageHolder;
    public Transform editorFloorScale;


    [Header("start phase stuff")]
    public Texture welcomePicture;
    public Texture2D startCubeTexture;
    [Header("tutorial phase stuff")]
    public Texture tutorialPicture;
    public Texture2D tutInputPicture;
    private List<Texture2D> tutTexturesForCubes = new List<Texture2D>();

    //info o aktualni fazi
    private Experiment activeExperiment = null;
    private Configuration activeConfig;
    private int activePuzzleIndex;
    bool inStart;
    bool inTut;
    bool phaseFinished;

    //odpocet a score
    [Header("referencies to UI")]
    private float timeLeft; //in seconds
    public Text timerText;
    private bool countingDown = false;
    public Text scoreText;
    private int skore;

    //next button
    public Button nextButton;//the NEXT button will be hihghlighted when curret phase is finished
    private Color highlightColor = Color.green;
    private Color normalColor = Color.white;

    //other UI
    public Text idInuput;
    ///////////////////////////private List<string> ids = new List<string>();
    public Text messageOutput;
    public InputField playeridInputField;
    public GameObject popupPanel;
    public GameObject phaseLoadingPanel;

    //scrollview ui
    public GameObject phaseScrollContent;
    public GameObject configScrollContent;
    public GameObject namePlatePrefab;
    private List<GameObject> phaseLabels = new List<GameObject>();
    private List<Button> configButtons = new List<Button>();
    public Button configNameButtonPrafab;


    private Vector3 originalScale = new Vector3();
    private List<Texture2D> modelPictures = new List<Texture2D>();
    public Text expNameText;


    //testing worldspace results canvas
    [Header("for testing only")]
    public Text resultsText;

    //cislovani krychli/mrizky je od leveho dolniho rohu:
    // ---------------------
    // |10 |11 |12 |13 |14 |
    // ---------------------
    // | 5 | 6 | 7 | 8 | 9 |
    // ---------------------
    // | 0 | 1 | 2 | 3 | 4 |
    // ---------------------
    //textura jedne kostky (0=selected image, 1-5 = "grey parts")
    // -------------------------
    // |       |       |       |
    // |       |       |       |
    // -------------------------
    // |       |       |       |
    // |   3   |   4   |   5   |
    // -------------------------
    // |       |       |       |
    // |   0   |   1   |   2   |
    // -------------------------

    //***********************************************THE VERY BEGINNING********************************************************************
    private void Awake()
    {
        //singleton stuff
        if (instance != null)
        {
            Debug.Log("Multiple Managers in one scene!");
        }
        instance = this;

        //development vr unavailble stuff
        //if (SteamVR.instance != null)
        //{
           // ActivateRig(VRavailable);
            ScaleRoomToFitPlayArea();
       //}
       //else
       //{
       //    ActivateRig(VRunavailable);
       //}

        //obsah sceny pred dokoncenim setupu
        modelPictureFrame.material.mainTexture = null;
        modelPictureFrame.material.color = Color.white;
        ////////////////////////////////////////theNpc.gameObject.SetActive(false);
    }

    //private void ActivateRig(GameObject rig)
    //{
    //    VRavailable.SetActive(rig == VRavailable);
    //    VRunavailable.SetActive(rig == VRunavailable);
    //}

    private void ScaleRoomToFitPlayArea()
    {
        //get info from play area
        Valve.VR.HmdQuad_t rect = new Valve.VR.HmdQuad_t();

        //tady to chce nejaky wait, aby se rozmery mistnosti stihly nacist....muze to chvilku trvat..puvidne tu bylo //SteamVR_PlayArea.GetBounds(SteamVR_PlayArea.Size.Calibrated, ref rect);
        while (!SteamVR_PlayArea.GetBounds(SteamVR_PlayArea.Size.Calibrated, ref rect))
        {
            System.Threading.Thread.Sleep(1000);//je ok takhle to cely uspat???
        }

        float floorScaleFactorX = editorFloorScale.localScale.x;//sirka podlahy v editoru
        float floorScaleFactorZ = editorFloorScale.localScale.z/2;//delka podlahy v editoru kam dosahuje camera rig
        //Vector3 newScale = new Vector3(floorScaleFactorX / Mathf.Abs(rect.vCorners0.v0 - rect.vCorners2.v0), 1, floorScaleFactorZ / Mathf.Abs(rect.vCorners0.v2 - rect.vCorners2.v2));
        Vector3 newScale = new Vector3(Mathf.Abs(rect.vCorners0.v0 - rect.vCorners2.v0)/ floorScaleFactorX, 1,Mathf.Abs(rect.vCorners0.v2 - rect.vCorners2.v2)/ floorScaleFactorZ);
        float safetyBorder = 0.02f;
        newScale.x += safetyBorder;
        newScale.z += safetyBorder;
        //scale room
        level.localScale = newScale;
        //move npc and camera rig
        /////////////////////////////////////////////////////////////////theNpc.position = npcPoint.position;
        this.transform.position = cameraRigPoint.position;
        ////adjust model picture height (to preserve the correct ratio)
        ////imageHolder.localScale = new Vector3(imageHolder.localScale.x, imageHolder.localScale.y * floorScaleFactorX / Mathf.Abs(rect.vCorners0.v0 - rect.vCorners2.v0), imageHolder.localScale.z);
        //adjust container size (to be squares)
        containersHolder.transform.localScale = new Vector3(1 / containersHolder.transform.lossyScale.x, 1, 1 / containersHolder.transform.lossyScale.z);
        //adjust tile size (to be cubes)
        tileHolder.transform.localScale = new Vector3(1 / tileHolder.transform.lossyScale.x, 1, 1 / tileHolder.transform.lossyScale.z);


        originalScale = imageHolder.localScale;
    }

    //********************************************************PHASES**********************************************************************************

    public void StartExperiment(Experiment e)
    {
        activeExperiment = e;
        //do stuff...
        expNameText.text = e.name;
        //fill out the configs scrollview...
        for (int i = 0; i < e.configs.Count; i++)
        {
            int iForDelegate = i;
            var p = Instantiate(configNameButtonPrafab, configScrollContent.transform);
            p.GetComponentInChildren<Text>().text = e.configs[i].name;
            configButtons.Add(p);
            p.onClick.AddListener(delegate { if (inStart) { FinishStartPhase(); FinishCurrentConfig(); StartConfig(e.configs[iForDelegate]); } });//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        }

        StartConfig(e.configs[0]);
    }
    private void FinishExperiment()//can be called only after current configuration was correctly finished
    {
        //clear the configButtons...
        for (int i = configButtons.Count - 1; i >= 0; i--)
        {
            Destroy(configButtons[i]);
            configButtons.RemoveAt(i);
        }
        //...
        activeExperiment = null;
    }

    public void OnQuitClicked()
    {
        if (inStart)
        {
            FinishStartPhase();
        }
        else
        {
            if (inTut)
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
    }
    private void FinishCurrentConfig()
    {
        //can be called only after current phase was correctly finished!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //tady by se to melo zeptat, jestli opravdu chci prepnout na jinou konfiguraci.....

        //finish current phase properly....destroy cubes.....any special cases???????????????????????????????????
        activePuzzleIndex = -1;
        texturesForCubes.Clear();
        modelPictures.Clear();
        tutTexturesForCubes.Clear();
        DestroyCharacter();

        //clear the phase scrollview...
        for (int i = phaseLabels.Count - 1; i >= 0; i--)
        {
            Destroy(phaseLabels[i]);
            phaseLabels.RemoveAt(i);
        }

        imageHolder.localScale = originalScale;

        DestroyCharacter();
    }
    private void StartConfig(Configuration c)
    {
        //fill out the phase scrollview...
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
        //////////////////////////////////////////theNpc.gameObject.SetActive(c.withNPC);
        //////////////////////////////////////////theNpc.position = npcPoint.position;
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

        activeConfig = c;
        //create textures for tutorial
        if (c.withTutorial)
        {
            tutTexturesForCubes=CreateTexturesForCubes(2, 2, tutInputPicture);
        }
        //create textures for puzzles
        //and create txtures for model pictures
        for (int i = 0; i < c.puzzles.Count; i++)
        {
            Texture2D tt;
            if (MenuLogic.instance.LoadTexture(c.puzzles[i].pathToImage) == null)//v pripade chyby nahrad obrazek vykricnikem....ale nic neukoncuj, je to jen varovani
            {
                texturesForCubes.Add(CreateTexturesForCubes(c.puzzles[i].heigthpx, c.puzzles[i].widthpx, MenuLogic.instance.missingImage.texture));
                tt = MenuLogic.instance.missingImage.texture;
            }
            else//jinak normalne nahraj obrazky ze souboru
            {
                texturesForCubes.Add(CreateTexturesForCubes(c.puzzles[i].heigthpx, c.puzzles[i].widthpx, MenuLogic.instance.LoadTexture(c.puzzles[i].pathToImage)));
                tt = MenuLogic.instance.LoadTexture(activeConfig.puzzles[i].pathToImage);
            }
            int sideWidth = Mathf.Min(tt.height / activeConfig.puzzles[i].heigthpx, tt.width / activeConfig.puzzles[i].widthpx);
            Texture2D t = new Texture2D(sideWidth * activeConfig.puzzles[i].widthpx, sideWidth * activeConfig.puzzles[i].heigthpx);
            Color[] block = tt.GetPixels(0, 0, sideWidth * activeConfig.puzzles[i].widthpx, sideWidth * activeConfig.puzzles[i].heigthpx);
            t.SetPixels(0, 0, sideWidth * activeConfig.puzzles[i].widthpx, sideWidth * activeConfig.puzzles[i].heigthpx, block);
            t.Apply();
            modelPictures.Add(t);
        }

        originalScale = imageHolder.localScale;

        StartStart();
    }

    private void PhaseStart()
    {
        activePuzzleIndex++;
        if (activeConfig.withTutorial)
        {
            phaseLabels[activePuzzleIndex + 2].GetComponent<Image>().color = Color.green;
        }
        else
        {
            phaseLabels[activePuzzleIndex + 1].GetComponent<Image>().color = Color.green;
        }
        //testing scaling of the model picture...
        imageHolder.localScale = originalScale;
        float x = activeConfig.puzzles[activePuzzleIndex].widthpx * tileSize * 3f;
        float y = activeConfig.puzzles[activePuzzleIndex].heigthpx * tileSize * 3f;
        imageHolder.transform.localScale = new Vector3(imageHolder.transform.localScale.x * x, imageHolder.transform.localScale.y * y, 0.2f);

        modelPictureFrame.material.mainTexture = modelPictures[activePuzzleIndex];
        GeneratePuzzleTiles(activeConfig.puzzles[activePuzzleIndex].heigthpx, activeConfig.puzzles[activePuzzleIndex].widthpx, texturesForCubes[activePuzzleIndex]);

        //start timer
        timeLeft = activeConfig.timeLimit;
        countingDown = true;

        //dale by melo spustit spravnou animaci
        if(theNpc!=null)
            theNpc.GetComponent<Animator>().SetTrigger("StartNewPuzzle");
        //a zecit resit nejake logovani a ukladani vysledku a tak...
    }
    private void PhaseFinish()
    {
        if (activeConfig.withTutorial)
        {
            phaseLabels[activePuzzleIndex + 2].GetComponent<Image>().color = Color.white;
        }
        else
        {
            phaseLabels[activePuzzleIndex + 1].GetComponent<Image>().color = Color.white;
        }
        if (!phaseFinished)//to se stane kdyz nekdo klikne na Yes v popup Opravdu chcete pokracovat?
        {
            Debug.Log("Ended too soon!!! The player did not finish, I should not save this data!!!");
        }
        //save data
        //...
        Debug.Log("-----------MainPhaseFinished-----------");
        Debug.Log("saving data to: ....ehm");
        Debug.Log("id: " + idInuput.text);
        Debug.Log("hasNPC: " + activeConfig.withNPC);
        Debug.Log("time: " + (activeConfig.timeLimit - timeLeft));
        Debug.Log("score: " + skore);
        Debug.Log("---------------------------------------");

        if (timeLeft > 0)
        {
            resultsText.text = "You finished it in " + (activeConfig.timeLimit - timeLeft) + "seconds!";//je to treba jeste zaokrouhlit...
        }
        else
        {
            resultsText.text = "You ran out of time, number of tiles placed correctly: " + scoreText.text;
        }
        resultsText.text += "\n NOW TAKE OFF THE HEADSET";

        BasicFinish();
        //reset time
        timeLeft = 0;
        countingDown = false;
        timerText.text = "0:00.0";
        //...
    }

    public void StartStart()
    {
        playeridInputField.interactable = true;

        inStart = true;
        phaseLabels[0].GetComponent<Image>().color = Color.green;
        imageHolder.localScale = originalScale;
        float x = 5 * tileSize * 3f;
        float y = 3* tileSize * 3f;
        imageHolder.transform.localScale = new Vector3(imageHolder.transform.localScale.x * x, imageHolder.transform.localScale.y * y, 0.2f);
        modelPictureFrame.material.mainTexture = welcomePicture;
        modelPictureFrame.material.color = Color.white;

        List<Texture2D> list = new List<Texture2D>();
        for (int i = 0; i < 6; i++)
        {
            list.Add(startCubeTexture);
        }

        GeneratePuzzleTiles(1, 1, list);

        foreach (var item in configButtons)
        {
            item.interactable = true;
        }
        nextButton.GetComponentInChildren<Text>().text = "NEXT PHASE";

        //animace NPC
        if (theNpc != null)
        {
            theNpc.GetComponent<Animator>().SetTrigger("TabletUp");
            theNpc.GetComponent<Animator>().SetTrigger("StartStart");
        }
        //text (rec NPC nebo napis na tabuli)
        //...
    }

    public void FinishStartPhase()
    {
        phaseLabels[0].GetComponent<Image>().color = Color.white;
        inStart = false;

        foreach (var item in configButtons)
        {
            item.interactable = false;
        }

        BasicFinish();

    }

    public void StartTutorial()
    {
        inTut = true;
        phaseLabels[1].GetComponent<Image>().color = Color.green;
        imageHolder.localScale = originalScale;
        float x = 5 * tileSize * 3f;
        float y = 3 * tileSize * 3f;
        imageHolder.transform.localScale = new Vector3(imageHolder.transform.localScale.x * x, imageHolder.transform.localScale.y * y, 0.2f);
        modelPictureFrame.material.mainTexture = tutorialPicture;
        modelPictureFrame.material.color = Color.white;

        GeneratePuzzleTiles(2, 2, tutTexturesForCubes);

        //animace NPC
        if (theNpc != null)
        {
            theNpc.GetComponent<Animator>().SetTrigger("TabletDown");
            theNpc.GetComponent<Animator>().SetTrigger("StartWelcomeSpeech");
        }
        //text (rec NPC nebo napis na tabuli)
        //...
    }

    public void FinishTutorial()
    {
        phaseLabels[1].GetComponent<Image>().color = Color.white;
        inTut = false;

        BasicFinish();
    }

    public void BasicFinish()
    {
        //destroy tiles
        foreach (GameObject tile in tileList)
        {
            tile.GetComponent<PuzzleTile>().DestroyYourself();
        }
        tileList.Clear();
        //destroy containers
        foreach (var container in containerList)
        {
            Destroy(container.gameObject);
        }
        containerList.Clear();
        //clear texts
        scoreText.text = "0";
        phaseFinished = false;
        nextButton.image.color = normalColor;
        skore = 0;
    }

    //*****************************************SWITCHING********************************************************

    public void TrySwitchPhase(bool skipCondition)
    {
        //specialne pro welcome phase - kontrola jestli bylo zadano unikatni playerID
        if (inStart)
        {
            if ((!ContainsWhitespaceOnly(idInuput.text)) && (!activeExperiment.ids.Contains(idInuput.text)))//je neprazdny a je unikatni
            {
                messageOutput.text = "";
                playeridInputField.interactable = false;
            }
            else
            {
                messageOutput.text = "Player ID must be filled and must be unique!!!";
                return;
            }
        }
        //try switch phase
        if (skipCondition || phaseFinished)//tady bude kontrola, jestli hrac splnil ukol teto faze, pokud ne, program se zepta, jestli opravdu chceme pokracovat
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
    //ale chtelo by to jeste vylepsit, takhle se pri tom cekani da ledacos pokazit necekany,m klikanim a tak...!!!!!!!!!!!!!!!!!!!!!!!
    private IEnumerator SwitchPhase()
    {
        phaseLoadingPanel.SetActive(true);//zamezi koordinatorovi na cokoliv klikat v prubeho prepinani faze
        //ani hrac v teto dobe nemuze nic pokazit, protoze OnCubePlaced probehne jen pokud neni phaseFinished
        //a sebranim krychle se nic nezkazi (respawnuji se i z ruky)....a nic dalsiho uz hrac delat neumi, takze ok

        if (inStart)//if just finished start phase
        {
            activeExperiment.ids.Add(idInuput.text);
            FinishStartPhase();
            yield return new WaitForSeconds(1);
            if (activeConfig.withTutorial)
            {
                StartTutorial();
            }
            else
            {
                activePuzzleIndex = -1;//od ted zacinaji main puzzly
                PhaseStart();
            }
        }
        else
        {
            if(inTut)//if just finished tutorial phase
            {
                FinishTutorial();
                yield return new WaitForSeconds(1);
                activePuzzleIndex = -1;//od ted zacinaji main puzzly
                if (activeConfig.puzzles.Count==1)
                {
                    nextButton.GetComponentInChildren<Text>().text = "SAVE RESULT";
                }
                PhaseStart();
            }
            else
            {
                if (activePuzzleIndex == activeConfig.puzzles.Count - 1)//if just finished the last puzzle
                {
                    playeridInputField.interactable = true;
                    PhaseFinish();
                    yield return new WaitForSeconds(1);

                    StartStart();
                }
                else//if just finished any other puzzle
                {
                    PhaseFinish();
                    yield return new WaitForSeconds(1);
                    if (activePuzzleIndex == activeConfig.puzzles.Count - 2)
                    {
                        nextButton.GetComponentInChildren<Text>().text = "SAVE RESULT";
                    }
                    PhaseStart();
                }
            }
        }
        phaseLoadingPanel.SetActive(false);
    }
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


    //////////////////////********************************DREATING TEXTURES, CONTAINERS, TILES...********************************************
    private List<Texture2D> CreateTexturesForCubes(int h, int w, Texture2D input)
    {
        int sideWidth = Mathf.Min(input.height / h, input.width / w);
        List<Texture2D> result = new List<Texture2D>();

        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                Texture2D newTexture = new Texture2D(3 * sideWidth, 3 * sideWidth, TextureFormat.RGB565, false); //je to false potreba?                                                       
                //read block of data (one square of puzzle) from image
                Color[] block = input.GetPixels(sideWidth * j, sideWidth * i, sideWidth, sideWidth);
                //write it to new texture to the right position        
                newTexture.SetPixels(0, 0, sideWidth, sideWidth, block);
                //apply changes to texture
                newTexture.Apply();
                //texturesForCubes.Add(newTexture);
                result.Add(newTexture);
            }
        }
        return result;
    }

    private void ShuffleList<T>(List<T> list)//zamicha seznam cehokoliv
    {
        int listCount = list.Count;
        for (int i = 0; i < listCount; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, listCount);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    private void GeneratePuzzleTiles(int h, int w, List<Texture2D> textureSource)
    {
        //umisti sparavne containersHolder
        containersHolder.transform.position = new Vector3(center.transform.position.x-((w*tileSize)/2)+(tileSize/2), center.transform.position.y, center.transform.position.z - ((h * tileSize) / 2) + (tileSize / 2));
        //generuj containers na stul
        int k = 0;
        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                GameObject c = Instantiate(containerPrefab);
                c.transform.SetParent(containersHolder.transform);
                c.transform.localScale = new Vector3(tileSize, 0.02f, tileSize);
                c.transform.localPosition = new Vector3(-j * tileSize, 0, -i * tileSize);//poradi generovani (znamenka) je upraveno aby sedely indexy (viz obr cislovani mrizky)

                c.GetComponent<TileContainer>().Initialize(k);//prefab musi mit TileContainer, jinak to tady spadne...
                containerList.Add(c.GetComponent<TileContainer>());
                k++;
            }
        }
        //generuj spawnpoints
        GenerateSpawnPoints(h,w);
        //generuj dilky (krychle)
        ShuffleList(spawnPositions);
        k = 0;
        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                //get random rotation
                Vector3[] axies = new Vector3[] { Vector3.forward, Vector3.down, Vector3.right, Vector3.back, Vector3.up, Vector3.left };
                //int randomIndex1 = Random.Range(0, 6);
                //int randomIndex2 = Random.Range(0, 6);
                //Quaternion randomRotation = Quaternion.LookRotation(axies[randomIndex1], axies[randomIndex2]);

                //create the cube
                //GameObject t = Instantiate(tilePrafab, spawnPoints[k].position,randomRotation);
                GameObject t = Instantiate(tilePrafab, spawnPositions[k], Quaternion.LookRotation(axies[3],axies[1]));
                t.transform.SetParent(tileHolder.transform);
                t.transform.localScale = new Vector3(tileSize * 100, tileSize * 100, tileSize * 100);// *100 protoze moje krychle z blenderu je omylem 100x mensi nez krychle v unity...

                //add the texture
                MeshRenderer mr = t.GetComponent<MeshRenderer>();//prefab musi mit MeshRenderer, jinak to tady spadne...
                mr.material = new Material(Shader.Find("Standard"));
                mr.material.mainTexture = textureSource[k];

                //index
                t.GetComponent<PuzzleTile>().Initialize(k, spawnPositions[k]);//prefab musi mit PuzzleTile, jinak to tady spadne...//tady bylo <Tile> v old grab system

                tileList.Add(t);
                k++;
            }
        }

    }

    [Header("for generating spawnpoints")]
    public Transform leftPoint;
    public Transform rightPoint;
    private List<Vector3> spawnPositions = new List<Vector3>();
    private void GenerateSpawnPoints(int h, int w)
    {
        spawnPositions.Clear();
        foreach (Transform point in spawnPoints)
        {
            spawnPositions.Add(point.position);
        }

        float offset = tileSize/2;
        float jOffset = tileSize;

        for (int i = 0; i < Mathf.Ceil((float)w / 2); i++)//polovina kostek do leva
        {
            for (int j = 0; j < h; j++)
            {
                float rand = Random.Range(0, offset-0.05f);
                float jRand = Random.Range(0, jOffset-0.05f);
                spawnPositions.Add(new Vector3(leftPoint.position.x - (tileSize + offset) * i - (offset + (tileSize / 2)+rand), leftPoint.position.y, leftPoint.position.z - (tileSize + jOffset) * j+jRand));
            }
        }

        for (int i = 0; i <w/2 ; i++)//druha polovina kostek do prava
        {
            for (int j = 0; j < h; j++)
            {
                float rand = Random.Range(0, offset - 0.05f);
                float jRand = Random.Range(0, jOffset-0.05f);
                spawnPositions.Add(new Vector3(offset + (tileSize / 2) + rightPoint.position.x + (tileSize + offset) * i-rand, rightPoint.position.y, rightPoint.position.z - (tileSize + jOffset) * j+jRand));
            }
        }

    }

    //***********************************************UPDATING DURING A PHASE********************************************************************

    private void Update()
    {
        if (countingDown && (!phaseFinished))//aka if main phase && !finished
        {
            //zobrazeni timeru
            int minutes = (int)timeLeft / 60;
            float seconds = timeLeft % 60;
            timerText.text = string.Format("{0:00}:{1:00.0}", minutes, seconds);

            if (timeLeft <= 0)
            {
                countingDown = false;
                phaseFinished = true;
                nextButton.image.color = new Color(1, 0.5f, 0);
            }
            timeLeft -= Time.deltaTime;
        }

    }

    public void OnCubeRemoved(bool correctlyPlaced)//when player pics cube up from the grid
    {
        if (!phaseFinished)
        {
            if (correctlyPlaced)
            {
                skore--;
            }
            scoreText.text = skore.ToString();
        }
    }

    public void OnCubePlaced(bool correctly)//when player places a cube
    {
        if (!phaseFinished)
        {
            if (inStart)
            {
                phaseFinished = true;
                nextButton.image.color = highlightColor;
                Debug.Log("cube placed, huray!!! nothing else matters");
            }
            else
            {
                //updatovani skore a zjisteni, jestli neni phaseFinished
                if (correctly)
                {
                    skore++;
                }
                scoreText.text = skore.ToString();

                bool finished = true;
                foreach (TileContainer item in containerList)
                {
                    if (item.Matches == false)
                    {
                        finished = false;
                        break;
                    }
                }
                if (finished)
                {
                    phaseFinished = true;
                    nextButton.image.color = highlightColor;
                    //do stuff!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    Debug.Log("puzzle completed, huray!!!");
                }
            }
        }
    }
    //**********************************************************************************************************************************
    [Header("for changing animators...")]
    public List<NpcModel> npcModels = new List<NpcModel>();
    public List<NpcBeahviour> npcBehaviours = new List<NpcBeahviour>();

    private void CreateCharacter(NpcModel model, NpcBeahviour bahaviour)
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

    [Header("Animation stuff")]
    public string npcName = "";
    public GameObject subtitlesCanvas;
    public int whoWantsSubtitles = -1;

}

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
