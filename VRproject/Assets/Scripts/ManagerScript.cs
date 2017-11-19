////using System.Collections;
////using System.Collections.Generic;
////using UnityEngine;
////using UnityEngine.UI;
////
//////hlavni manager
////
////public class ManagerScript : MonoBehaviour {
////
////    //sigleton stuff
////    public static ManagerScript instance;
////
////    //headset mode / debug mode (VR stuff)
////    [Header("headset mode / debug mode")]
////    public GameObject VRavailable;
////    public GameObject VRunavailable;
////
////    //kontainery 
////    [Header("containers")]
////    public GameObject containerPrefab;
////    public GameObject containersHolder;
////    [Header("this shoul be hidden somehow...")]
////    public List<TileContainer> containerList = new List<TileContainer>();
////
////    //pucliky (sebratelne dilky)
////    [Header("tiles")]
////    public GameObject tilePrafab;
////    public List<Transform> spawnPoints = new List<Transform>();//musi jich byt 3*5!!!!!!
////    public GameObject tileHolder;
////    public float tileSize;//ideal je tak 0.15f
////    private List<GameObject> tileList = new List<GameObject>();
////
////    //vyrabeni textur na kostky
////    //private int kostekNaVysku = 3;
////    //private int kostekNaSirku = 5;
////    private List<Texture2D> inputPictures = new List<Texture2D>();
////    private List<Texture2D> texturesForCubes = new List<Texture2D>();
////
////    //co se ma skladat
////    [Header("puzzle to solve")]
////    public Renderer modelPictureFrame;
////    [Header("this shoul be hidden somehow...")]
////    public int modelPictureNumber;//musi byt 0 az 5!!!
////    //matchingPieces[i] obsahuje info o tom, jestli krychle s indexem i je spravne umistena
////    //[Header("this shoul be hidden somehow...")]
////    //public bool[] matchingPieces;//mozna by neskodilo oznacit tohle jako [Hide In Editor]...
////
////    //room scale stuff
////    [Header("room scale stuff.")]
////    public SteamVR_PlayArea VRplayArea;
////    public Transform level;
////    public Transform cameraRigPoint;
////    public Transform simpleMan;
////    public Transform simpleManPoint;
////    public Transform imageHolder;
////    public Transform editorFloorScale;
////
////    //info from the setup page
////    private bool hasNPC;
////    private int pictureSet;
////    private string savePath;
////
////    //odpocet a score
////    [Header("referencies to UI")]
////    private float timeLeft; //in seconds
////    public Text timerText;
////    private bool countingDown = false;
////    public Text scoreText;
////    private float maxTime = 140;//kolik sekund ma na postaveni skladacky
////    private int skore;
////
////    //current phase info
////    private Phase currentPhase;
////    private enum Phase { welcome=1, tutorial, main };
////    public Text phaseName;
////    private bool phaseFinished = false;
////    public Button nextButton;//the NEXT button will be hihghlighted when curret phase is finished
////    private Color highlightColor = Color.green;
////    private Color normalColor = Color.white;
////
////    [Header("other stuff")]
////
////    //odkaz na tridu, ktera drzi seznam setu obrazku
////    public PictureSets availableSets;
////
////    //welcome stuff
////    public Texture welcomePicture;
////    public Texture2D startCubeTexture;
////    //tutorial stuff
////    public Texture tutorialPicture;
////    private List<Texture2D> tutInputPictures = new List<Texture2D>();
////    private List<Texture2D> tutTexturesForCubes = new List<Texture2D>();
////
////    //testing worldspace results canvas
////    public Text resultsText;
////
////    public Text idInuput;
////    public Text messageOutput;
////    public InputField playeridInputField;
////    public GameObject popupPanel;
////
////    private List<string> ids = new List<string>();
////
////    public GameObject phaseLoadingPanel;
////
////    //cislovani krychli/mrizky je od leveho dolniho rohu:
////    // ---------------------
////    // |10 |11 |12 |13 |14 |
////    // ---------------------
////    // | 5 | 6 | 7 | 8 | 9 |
////    // ---------------------
////    // | 0 | 1 | 2 | 3 | 4 |
////    // ---------------------
////    //textura jedne kostky
////    // -------------------------
////    // |       |       |       |
////    // |       |       |       |
////    // -------------------------
////    // |       |       |       |
////    // |   3   |   4   |   5   |
////    // -------------------------
////    // |       |       |       |
////    // |   0   |   1   |   2   |
////    // -------------------------
////
////
////
////    //***********************************************THE VERY BEGINNING********************************************************************
////    private void Awake()
////    {
////        //singleton stuff
////        if (instance != null)
////        {
////            Debug.Log("Multiple ManagerScripts in one scene!");
////        }
////        instance = this;
////
////        //development vr unavailble stuff
////        if (SteamVR.instance != null)
////        {
////            ActivateRig(VRavailable);
////            ScaleRoomToFitPlayArea();
////        }
////        else
////        {
////            ActivateRig(VRunavailable);
////        }
////
////        //obsah sceny pred dokoncenim setupu
////        modelPictureFrame.material.mainTexture = null;
////        modelPictureFrame.material.color = Color.white;
////        simpleMan.gameObject.SetActive(false);
////    }
////
////    private void ActivateRig(GameObject rig)
////    {
////        VRavailable.SetActive(rig == VRavailable);
////        VRunavailable.SetActive(rig == VRunavailable);
////    }
////
////    private void ScaleRoomToFitPlayArea()
////    {
////        //get info from play area
////        Valve.VR.HmdQuad_t rect = new Valve.VR.HmdQuad_t();
////
////        //tady to chce nejaky wait, aby se rozmery mistnosti stihly nacist....muze to chvilku trvat..puvidne tu bylo //SteamVR_PlayArea.GetBounds(SteamVR_PlayArea.Size.Calibrated, ref rect);
////        while (!SteamVR_PlayArea.GetBounds(SteamVR_PlayArea.Size.Calibrated, ref rect))
////        {
////            System.Threading.Thread.Sleep(1000);//je ok takhle to cely uspat???
////        }
////
////        float floorScaleFactorX = editorFloorScale.localScale.x / 2;//sirka podlahy v editoru/2 (nyni 1.75)
////        float floorScaleFactorZ = editorFloorScale.localScale.z / 4;//delka podlahy v editoru kam dosahuje camera rig/2 (nyni 1.50)
////        Vector3 newScale = new Vector3(floorScaleFactorX / Mathf.Abs(rect.vCorners0.v0 - rect.vCorners2.v0), 1, floorScaleFactorZ / Mathf.Abs(rect.vCorners0.v2 - rect.vCorners2.v2));
////        //scale room
////        level.localScale = newScale;
////        //move npc and camera rig
////        simpleMan.position = simpleManPoint.position;
////        this.transform.position = cameraRigPoint.position;
////        //adjust model picture height (to preserve the correct ratio)
////        imageHolder.localScale = new Vector3(imageHolder.localScale.x, imageHolder.localScale.y * floorScaleFactorX / Mathf.Abs(rect.vCorners0.v0 - rect.vCorners2.v0), imageHolder.localScale.z);
////        //adjust container size (to be squares)
////        containersHolder.transform.localScale = new Vector3(1 / containersHolder.transform.lossyScale.x, 1, 1 / containersHolder.transform.lossyScale.z);
////        //adjust tile size (to be cubes)
////        tileHolder.transform.localScale = new Vector3(1 / tileHolder.transform.lossyScale.x, 1, 1 / tileHolder.transform.lossyScale.z);
////    }
////
////    //******************************************************************************************************************************************
////
////    //***********************************************UPDATING DURING A PHASE********************************************************************
////
////    private void Update()
////    {
////        if (countingDown && (!phaseFinished))//aka if main phase && !finished
////        {
////            //zobrazeni timeru
////            int minutes = (int)timeLeft / 60;
////            float seconds = timeLeft % 60;
////            timerText.text = string.Format("{0:00}:{1:00.0}", minutes, seconds);
////
////            if (timeLeft <= 0)
////            {
////                countingDown = false;
////                phaseFinished = true;
////                nextButton.image.color = new Color(1, 0.5f, 0);
////            }
////            timeLeft -= Time.deltaTime;
////        }
////
////    }
////
////    public void OnCubeRemoved(bool correctlyPlaced)//when player pics cube up from the grid
////    {
////        if (!phaseFinished)
////        {
////            if (correctlyPlaced)
////            {
////                skore--;
////            }
////            scoreText.text = skore.ToString();
////        }
////    }
////
////    public void OnCubePlaced(bool correctly)//when player places a cube
////    {
////        if (!phaseFinished)
////        {
////            if (currentPhase == Phase.welcome)
////            {
////                phaseFinished = true;
////                nextButton.image.color = highlightColor;
////                Debug.Log("cube placed, huray!!! nothing else matters");
////            }
////            else
////            {
////                //updatovani skore a zjisteni, jestli neni phaseFinished
////                if (correctly)
////                {
////                    skore++;
////                }
////                scoreText.text = skore.ToString();
////
////                bool finished = true;
////                foreach (TileContainer item in containerList)
////                {
////                    if (item.Matches == false)
////                    {
////                        finished = false;
////                        break;
////                    }
////                }
////                if (finished)
////                {
////                    phaseFinished = true;
////                    nextButton.image.color = highlightColor;
////                    //do stuff!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
////                    Debug.Log("puzzle completed, huray!!!");
////                }
////            }
////        }
////    }
////    //**********************************************************************************************************************************
////
////    //***********************************************PHASE SWITCHING********************************************************************
////
////    //zapise si nastaveni z uvodniho setupu
////    public void InitiationAfterSetup(bool withNPC, int pictureTheme, string path)
////    {
////        hasNPC = withNPC;
////        pictureSet = pictureTheme;
////        savePath = path;
////
////        //apply setup info - enable/disable the NPC
////        simpleMan.gameObject.SetActive(hasNPC);
////
////        //apply setup info - create textures based on picture theme
////        switch (pictureSet)
////        {
////            case 0: inputPictures = availableSets.landscapeSet; break;
////            case 1: inputPictures = availableSets.monumentsSet; break;
////            case 2: inputPictures = availableSets.animalsSet; break;
////        }
////        CreateTexturesForCubes(3, 5, texturesForCubes, inputPictures);
////        //apply setup info - path ???
////        //...
////
////        //create textures for tutorial cubes
////        tutInputPictures = availableSets.tutorialSet;
////        CreateTexturesForCubes(2, 2, tutTexturesForCubes, tutInputPictures);
////
////        InitiateWelcomePhase();
////    }
////    public void TrySwitchPhase(bool skipCondition)
////    {
////        //specialne pro welcome phase - kontrola jestli bylo zadano unikatni playerID
////        if (currentPhase == Phase.welcome)
////        {
////            if ((!ContainsWhitespaceOnly(idInuput.text)) && (!ids.Contains(idInuput.text)))//je neprazdny a je unikatni
////            {
////                messageOutput.text = "";
////                playeridInputField.interactable = false;
////            }
////            else
////            {
////                messageOutput.text = "Player ID must be filled and must be unique!!!";
////                return;
////            }
////        }
////        //try switch phase
////        if (skipCondition || phaseFinished)//tady bude kontrola, jestli hrac splnil ukol teto faze, pokud ne, program se zepta, jestli opravdu chceme pokracovat
////        {
////            StartCoroutine(SwitchPhase());
////        }
////        else
////        {
////            //popup window
////            popupPanel.SetActive(true);
////        }
////    }
////
////    //mezi fazemi chvilku cekat....hlavne na konci main phase, aby mohl v klidu sundat headset...
////    //ale chtelo by to jeste vylepsit, takhle se pri tom cekani da ledacos pokazit necekany,m klikanim a tak...!!!!!!!!!!!!!!!!!!!!!!!
////    private IEnumerator SwitchPhase()
////    {
////        phaseLoadingPanel.SetActive(true);//zamezi koordinatorovi na cokoliv klikat v prubeho prepinani faze
////        //ani hrac v teto dobe nemuze nic pokazit, protoze OnCubePlaced probehne jen pokud neni phaseFinished
////        //a sebranim krychle se nic nezkazi (respawnuji se i z ruky)....a nic dalsiho uz hrac delat neumi, takze ok
////
////        switch(currentPhase)
////        {
////            case Phase.welcome: {
////                        ids.Add(idInuput.text);
////                        FinishWelcomePhase();
////                        yield return new WaitForSeconds(1);//co ale kdyby v tehle dobe na tlacitko next zmacknul vickrat za sebou???
////                        //...proto jsem sem pridalo to phaseLoadingPanel.setActive
////                        InitiateTutorialPhase();
////                    break;
////                }
////            case Phase.tutorial: FinishTutorialPhase(); yield return new WaitForSeconds(1); InitiateMainPhase(); break;
////            case Phase.main: FinishMainPhase(); yield return new WaitForSeconds(2); InitiateWelcomePhase(); playeridInputField.interactable = true; break;
////            default:break;
////        }
////
////        phaseLoadingPanel.SetActive(false);
////    }
////
////    private bool ContainsWhitespaceOnly(string s)
////    {
////        foreach(char c in s)
////        {
////            if(!char.IsWhiteSpace(c))
////            {
////                return false;
////            }
////        }
////        return true;
////    }
////
////
////    //spawnuje veci potrebne pro tutorial na ovladani ve VR
////    private void InitiateWelcomePhase()
////    {
////        currentPhase = Phase.welcome;
////        phaseName.text = "WELCOME PHASE";
////        modelPictureFrame.material.mainTexture = welcomePicture;
////        modelPictureFrame.material.color = Color.white;
////
////        List<Texture2D> list = new List<Texture2D>();
////        for (int i = 0; i<6;i++)
////        {
////            list.Add(startCubeTexture);
////        }
////
////        GeneratePuzzleTiles(1, 1, list);
////
////        //animace NPC
////        //text (rec NPC nebo napis na tabuli)
////        //...
////
////    }
////
////    //spawnuje veci potrebne pro tutorial na skladani puzzlu
////    private void InitiateTutorialPhase()
////    {
////        currentPhase = Phase.tutorial;
////        phaseName.text = "TUTORIAL PHASE";
////        modelPictureFrame.material.mainTexture = tutorialPicture;
////        modelPictureFrame.material.color = Color.white;
////
////        GeneratePuzzleTiles(2,2,tutTexturesForCubes);
////
////        //animace NPC
////        //text (rec NPC nebo napis na tabuli)
////        //...
////    }
////
////    //spawnuje veci potrebne na samotny experiment (mrizku, dilky, casovy odpocet)
////    private void InitiateMainPhase()
////    {
////        currentPhase = Phase.main;
////        phaseName.text = "MAIN PHASE";
////
////        modelPictureFrame.material.mainTexture = inputPictures[modelPictureNumber];
////        GeneratePuzzleTiles(3,5,texturesForCubes);
////        Debug.Log("main count " + containerList.Count);
////
////        //start timer
////        timeLeft = maxTime;
////        countingDown = true;
////
////        //dale by melo spustit spravnou animaci
////        //a zecit resit nejake logovani a ukladani vysledku a tak...
////    }
////
////    //spolecna cast uklidu na konci faze (vsechny 3 faze tohle provadeji)
////    private void BasicFinish()
////    {
////        //destroy tiles
////        foreach (GameObject tile in tileList)
////        {
////            tile.GetComponent<PuzzleTile>().DestroyYourself();
////        }
////        tileList.Clear();
////        //destroy containers
////        foreach (var container in containerList)
////        {
////            Destroy(container.gameObject);
////        }
////        containerList.Clear();
////        //clear texts
////        scoreText.text = "0";
////        phaseFinished = false;
////        nextButton.image.color = normalColor;
////        skore = 0;
////    }
////
////    private void FinishWelcomePhase()
////    {
////        BasicFinish();
////    }
////    private void FinishTutorialPhase()
////    {
////        BasicFinish();
////    }
////    private void FinishMainPhase()
////    {
////        if (!phaseFinished)//to se stane kdyz nekdo klikne na Yes v popup Opravdu chcete pokracovat?
////        {
////            Debug.Log("Ended too soon!!! The player did not finish, I should not save this data!!!");
////        }
////        //save data
////        //...
////        Debug.Log("-----------MainPhaseFinished-----------");
////        Debug.Log("saving data to: " + savePath);
////        Debug.Log("id: "+idInuput.text);
////        Debug.Log("hasNPC: "+hasNPC);
////        Debug.Log("time: "+ (maxTime - timeLeft));
////        Debug.Log("score: " + skore);
////        Debug.Log("---------------------------------------");
////
////        if (timeLeft > 0)
////        {
////            resultsText.text = "You finished it in " + (maxTime - timeLeft) + "seconds!";//je to treba jeste zaokrouhlit...
////        }
////        else
////        {
////            resultsText.text = "You ran out of time, number of tiles placed correctly: "+scoreText.text;
////        }
////        resultsText.text += "\n NOW TAKE OFF THE HEADSET";
////
////        BasicFinish();
////        //reset time
////        timeLeft = 0;
////        countingDown = false;
////        timerText.text = "0:00.0";
////        //...
////    }
////    private void NextPartOfMainPhase()
////    {
////        //pokud bude jeden clovek skladat vic puzzlu za sebou...
////        //change modelPictureNumber
////        //something like finish main phase and then initiate main phase, ale bez ukladani dat, napisu Take off the headset atd......
////        //pokud se tohle bude pouzivat, je traba upravit i stavajici finish main phase a taky aby se ukladala data o lehke+stredni+tezke skladacce...
////    }
////
////    //***********************************************************************************************************************************
////
////    //***************************************CREATING TEXTURES, CUBES AND CONTAINERS*****************************************************
////
////
////    private void CreateTexturesForCubes(int h,int w,List<Texture2D> result,List<Texture2D> input)
////    {
////        //predpoklady: obrazku je 6, jsou rozmeru 3:5,...
////        //nekde na zacatku by mela probehnout nejaka kontrola!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
////        int pictureWidth = input[0].width;//sirka vstupniho obrazku v pixelech
////        int sideWidth = pictureWidth / w;//hrana jedne krychle v pixelech
////
////        for (int i = 0; i < h; i++)                                                 
////        {                                                                                       
////            for (int j = 0; j < w; j++)                                             
////            {
////                Texture2D newTexture = new Texture2D(3 * sideWidth, 3 * sideWidth , TextureFormat.RGB565, false); //je to false potreba?                                                       
////                for (int k = 0; k < 6; k++)                                                     
////                {
////                    //read block of data (one square of puzzle) from image k
////                    Color[] block = input[k].GetPixels(sideWidth*j,sideWidth*i,sideWidth,sideWidth);
////                    //write it to new texture to the right position (based on k)           
////                    newTexture.SetPixels(sideWidth*(k%3), sideWidth * (k / 3), sideWidth, sideWidth, block);     
////                }
////                //apply changes to texture
////                newTexture.Apply();
////                //texturesForCubes.Add(newTexture);
////                result.Add(newTexture);
////            }
////        }
////    }
////
////    private void ShuffleList<T>(List<T> list)//zamicha seznam cehokoliv
////    {
////        int listCount = list.Count;
////        for (int i = 0; i < listCount; i++)
////        {
////            T temp = list[i];
////            int randomIndex = Random.Range(i, listCount);
////            list[i] = list[randomIndex];
////            list[randomIndex] = temp;
////        }
////    }
////
////    private void GeneratePuzzleTiles(int h,int w,List<Texture2D> textureSource)
////    {
////        //generuj containers na stul
////        int k = 0;
////        for (int i = 0; i < h; i++)
////        {
////            for (int j = 0; j < w; j++)
////            {
////                GameObject c = Instantiate(containerPrefab);
////                c.transform.SetParent(containersHolder.transform);
////                c.transform.localScale = new Vector3(tileSize, 0.02f, tileSize);
////                c.transform.localPosition = new Vector3(-j * tileSize, 0, -i * tileSize);//poradi generovani (znamenka) je upraveno aby sedely indexy (viz obr cislovani mrizky)
////
////                c.GetComponent<TileContainer>().Initialize(k);//prefab musi mit TileContainer, jinak to tady spadne...
////                containerList.Add(c.GetComponent<TileContainer>());
////                k++;
////            }
////        }
////        //generuj dilky (krychle)
////        ShuffleList(spawnPoints);
////        k = 0;
////        for (int i = 0; i < h; i++)
////        {
////            for (int j = 0; j < w; j++)
////            {
////                //get random rotation
////                Vector3[] axies = new Vector3[]{Vector3.forward,Vector3.down,Vector3.right,Vector3.back,Vector3.up,Vector3.left};
////                int randomIndex1 = Random.Range(0, 6);
////                int randomIndex2 = Random.Range(0, 6);
////                Quaternion randomRotation = Quaternion.LookRotation(axies[randomIndex1],axies[randomIndex2]);
////
////                //create the cube
////                GameObject t = Instantiate(tilePrafab,spawnPoints[k].position,randomRotation);
////                t.transform.SetParent(tileHolder.transform);
////                t.transform.localScale = new Vector3(tileSize*100 , tileSize*100 , tileSize*100);// *100 protoze moje krychle z blenderu je omylem 100x mensi nez krychle v unity...
////
////                //add the texture
////                MeshRenderer mr = t.GetComponent<MeshRenderer>();//prefab musi mit MeshRenderer, jinak to tady spadne...
////                mr.material = new Material(Shader.Find("Standard")); 
////                mr.material.mainTexture = textureSource[k];
////
////                //index
////                t.GetComponent<PuzzleTile>().Initialize(k, spawnPoints[k]);//prefab musi mit PuzzleTile, jinak to tady spadne...//tady bylo <Tile> v old grab system
////
////                tileList.Add(t);
////                k++;
////            }
////        }
////
////    }
////    //***********************************************************************************************************************************
////}