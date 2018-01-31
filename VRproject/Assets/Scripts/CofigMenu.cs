using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//ovladani pro CreateConfigCanvas

public class CofigMenu : MonoBehaviour {

    [Header("FileBrowser")]
    public GUISkin customSkin;
    protected FileBrowser m_fileBrowser;
    [SerializeField]
    protected Texture2D m_directoryImage,
                        m_fileImage;

    [Header("Adjusting puzzle size")]
    private float tableWidth;
    private float tableHeigth;
    ////////////////////////////////////public float tileSize;//duplicate!!!!!!!!!!!!!!!!!!!!!
    public GameObject table;

    private int maxPipeWidth;
    private int maxPipeHeigth;
    private int maxCubeWidth;
    private int maxCubeHeigth;

    [Header("References to UI elements")]
    public Toggle npcToggle;
    public Toggle tutorialToggle;
    public Dropdown numberOfPuzlesDropwdown;
    public InputField configNameField;
    public GameObject puzzlePanelPrefab;
    public GameObject scrollViewContent;
    public GameObject blockingPanel;
    public GameObject errorText;
    public InputField timeLimitField;
    public Dropdown modelDropdown;
    public Dropdown behaviourDropdown;

    public ExpMenu em;

    //seznamy...
    private List<string> texturePaths = new List<string>();
    private List<GameObject> puzzlePanels = new List<GameObject>();
    private int activeButton = 0;

    public PipePuzzle pp;
    public CubePuzzle cp;

    private void Start()
    {
        //zjisti kolik se na stul vejde max kostek na vysku/sirku
        tableWidth = table.transform.lossyScale.x;
        tableHeigth = table.transform.lossyScale.z;

        maxPipeWidth = (int)((tableWidth / 2) / pp.PipeSize);//ehm
        maxPipeHeigth = (int)(tableHeigth / pp.PipeSize)+1;//ehm...

        maxCubeWidth = (int)((tableWidth / 3) / cp.TileSize);
        maxCubeHeigth = (int)(tableHeigth / cp.TileSize);

        //podle toho nastavi dropdowny u nastavovani sirek/vysek obrazku
        SetNumberOfPuzzlesDropdownContent(maxCubeWidth, maxCubeHeigth);

        //nastav dropdowny na modely npc a chovani npc
        List<string> models = new List<string>();
        foreach (NpcModel model in NewManager.instance.npcModels)
        {
            models.Add(model.modelName);
        }
        modelDropdown.ClearOptions();
        modelDropdown.AddOptions(models);
        List<string> behaviours = new List<string>();
        foreach (NpcBeahviour behaviour in NewManager.instance.npcBehaviours)
        {
            behaviours.Add(behaviour.bahaviourName);
        }
        behaviourDropdown.ClearOptions();
        behaviourDropdown.AddOptions(behaviours);

        //vygeneruj panely reprezentujici jednotlive puzzly do scroll view
        OnNumberDropdownChanged();
    }

    public void OnNumberDropdownChanged()
    {
        if (numberOfPuzlesDropwdown.value + 1 <= puzzlePanels.Count)
        {
            //smaz stare nadbytecne (pokud jich puvodne bylo vice nez novy pocet)
            for (int i = puzzlePanels.Count - 1; i >= numberOfPuzlesDropwdown.value + 1; i--)
            {
                Destroy(puzzlePanels[i]);
                puzzlePanels.RemoveAt(i);
                texturePaths.RemoveAt(i);
            }
        }
        else
        {
            //insanciuj nove (pokud jich puvodne bylo mene nez novy pocet)
            for (int i = puzzlePanels.Count; i < numberOfPuzlesDropwdown.value + 1; i++)
            {
                int iForDelegate = i;
                var p = Instantiate(puzzlePanelPrefab, scrollViewContent.transform);
                puzzlePanels.Add(p);
                if (cubeToggle.isOn)/////////
                    p.GetComponentInChildren<Button>().onClick.AddListener(delegate { OnSelectPuzzleImageClick(iForDelegate); });
                else
                    p.GetComponentInChildren<Button>().image.sprite = MenuLogic.instance.pipeImage;/////////
                p.GetComponentInChildren<InputField>().onValueChanged.AddListener(delegate { OnInputTextEdited(p.GetComponentInChildren<InputField>()); });
                texturePaths.Add(null);
            }
        }
    }

    protected void OnGUI()//vykreslovani file browseru
    {
        GUI.skin = customSkin;
        if (m_fileBrowser != null)
        {
            m_fileBrowser.OnGUI();
        }
    }

    public void OnSelectPuzzleImageClick(int i)//kliknuti na button na panelu (=vyber png/jpg z disku)
    {
        blockingPanel.SetActive(true);
        activeButton = i;
        m_fileBrowser = new FileBrowser(
            new Rect(100, 100, Screen.width-200, Screen.height - 200),
            "Choose PNG or JPG File",
            FileSelectedCallback
        );

        m_fileBrowser.SelectionPattern = "*.png|*.jpg";//pomoci | jsou oddeleny jednotlive patterny (muj zasah do fileBrowseru)
        m_fileBrowser.DirectoryImage = m_directoryImage;
        m_fileBrowser.FileImage = m_fileImage;
    }


    protected void FileSelectedCallback(string path)//nastavi nahled zvoleneho obrazku a zapamatuje si cestu k nemu
    {
        m_fileBrowser = null;
        texturePaths[activeButton] = path;
        if (path != null)
        {
            puzzlePanels[activeButton].GetComponentInChildren<Button>().image.sprite = MenuLogic.instance.LoadNewSprite(path);
            if (puzzlePanels[activeButton].GetComponentInChildren<Button>().image.sprite == null)//neboli if picture loadig failed
            {   //jenom to naznaci ze je tu problem (zobrazi missingImage), ale konfiguraci to dovoli vyrobit => musi se pozdeji znovu provest kontrola...
                puzzlePanels[activeButton].GetComponentInChildren<Button>().image.sprite = MenuLogic.instance.missingImage;
            }
        }
        else
        {
            puzzlePanels[activeButton].GetComponentInChildren<Button>().image.sprite = null;
        }
        blockingPanel.SetActive(false);
        puzzlePanels[activeButton].GetComponentInChildren<Button>().image.color = Color.white;//cancel possible error highlight..even if path is still null (he tried)
    }

    public void OnInputTextEdited(InputField i)//cancel possible error highlight...
    {
        i.image.color = Color.white;
    }

    public void OnSaveClick()//pokusi se ze zadanych informaci vyrobit novou konfiguraci a pridat ji do seznamu availableConfigurations
    {
        //create only if everything is correctly filled out
        bool ok = true;
        if (configNameField.text == null || MenuLogic.instance.ContainsWhitespaceOnly(configNameField.text)||!IsValid(configNameField.text))
        {
            ok = false;
            configNameField.image.color = Color.red;
        }
        else
        {
            configNameField.image.color = Color.white;
        }
        int time=42;
        if (timeLimitField.text == null || !int.TryParse(timeLimitField.text,out time))
        {
            ok = false;
            timeLimitField.image.color = Color.red;
        }
        else
        {
            timeLimitField.image.color = Color.white;
        }
        for (int i = 0; i <= numberOfPuzlesDropwdown.value; i++)
        {
            GameObject q = puzzlePanels[i];
            InputField puzzleNameField = q.GetComponentInChildren<InputField>();
            if (puzzleNameField.text == null || MenuLogic.instance.ContainsWhitespaceOnly(puzzleNameField.text))
            {
                ok = false;
                puzzleNameField.image.color = Color.red;
            }
            else
            {
                puzzleNameField.image.color = Color.white;
            }
            Button selectImageButton = q.GetComponentInChildren<Button>();
            if (cubeToggle.isOn)//////////////
            {
                if (texturePaths[i] == null)
                {
                    ok = false;
                    selectImageButton.image.color = Color.red;
                }
                else
                {
                    selectImageButton.image.color = Color.white;
                }
            }
            else
            {
                selectImageButton.image.color = Color.white;///////////////
            }
        }
        if (!ok)
        {
            errorText.SetActive(true);
            return;
        }
        else
        {
            errorText.SetActive(false);
        }

        //create configuration (class)
        Configuration c = new Configuration();
        if (cubeToggle.isOn)
            c.puzzleType = "CubePuzzle";
        else
            c.puzzleType = "PipePuzzle";

        c.name = configNameField.text;
        c.withNPC = npcToggle.isOn;
        c.withTutorial = tutorialToggle.isOn;
        c.timeLimit = time;
        for (int i = 0; i <= numberOfPuzlesDropwdown.value; i++)
        {
            Puzzle p = new Puzzle();
            GameObject q = puzzlePanels[i];

            p.name = q.GetComponentInChildren<InputField>().text;
            List<Dropdown> droplist = new List<Dropdown>();
            q.GetComponentsInChildren<Dropdown>(droplist);
            p.heigthpx = droplist[1].value + 1;
            p.widthpx = droplist[0].value + 1;
            if (cubeToggle.isOn)
            {
                //priradit cestu k souboru s obrazkem
                p.pathToImage = texturePaths[i];
                //a napevno vygenerovat a ulozit zamichani spawnpointu (aby to meli vsichni stejne a taky aby se to dalo zpetne prehravat(tedy nechci nahodu))
                CubePuzzle cp = (CubePuzzle)NewManager.instance.cubePuzzle;
                p.spawnPointMix = new List<int>();
                int nnnn = cp.spawnPoints.Count + (p.heigthpx*p.widthpx);
                for (int e = 0; e < nnnn; e++)
                {
                    p.spawnPointMix.Add(e);
                }
                ShuffleList(p.spawnPointMix);
            }
            if(pipeToggle.isOn)
            {
                //je treba predem vymyslet a ulozit rozmisteni trubek (aby to meli vsichni stejne a taky aby se to dalo zpetne prehravat(tedy nechci nahodu))
                PipePuzzle pp = (PipePuzzle)NewManager.instance.pipePuzzle;
                p.chosenList = pp.ChoosePath(p.heigthpx, p.widthpx);
            }
            c.puzzles.Add(p);
        }

        c.modelName = modelDropdown.captionText.text;
        c.behaviourName = behaviourDropdown.captionText.text;

        //add it to the list
        MenuLogic.instance.availableConfigs.configs.Add(c);

        //switch back to expMenu
        MenuLogic.instance.expMenuCanvas.SetActive(true);
        MenuLogic.instance.confMenuCanvas.SetActive(false);
        em.AddOneNewConfig(c);

        CleanUp();
    }
    public void OnCancelInConfigMenuClicked()
    {
       ////cancel highlights
       //configNameField.image.color = Color.white;
       //for (int i = 0; i <= numberOfPuzlesDropwdown.value; i++)
       //{
       //    GameObject q = puzzlePanels[i];
       //    InputField puzzleNameField = q.GetComponentInChildren<InputField>();
       //    if (puzzleNameField.text == null || MenuLogic.instance.ContainsWhitespaceOnly(puzzleNameField.text))
       //    {
       //        puzzleNameField.image.color = Color.white;
       //        q.GetComponentInChildren<Button>().image.color = Color.white;
       //    }
       //}
       //errorText.SetActive(false);

        //switch to expMenu
        MenuLogic.instance.expMenuCanvas.SetActive(true);
        MenuLogic.instance.confMenuCanvas.SetActive(false);

        CleanUp();

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

    private bool IsValid(string s)//tedy neobsahuje carku (protoze se to pak bude ukladat do .csv )
    {
        foreach (char c in s)
        {
            if (c == ',')
            {
                return false;
            }
        }
        return true;
    }

    private void CleanUp()
    {
        pipeToggle.isOn = false;
        cubeToggle.isOn = true;
        for (int i = puzzlePanels.Count - 1; i >= 0; i--)
        {
            Destroy(puzzlePanels[i]);
            puzzlePanels.RemoveAt(i);
            texturePaths.RemoveAt(i);
        }
        numberOfPuzlesDropwdown.value = 0;
        OnNumberDropdownChanged();
    }

    public Toggle cubeToggle;
    public Toggle pipeToggle;
    public void OnTypeToggleChange()
    {
        //use different maxWidth
        if(cubeToggle.isOn)
            SetNumberOfPuzzlesDropdownContent(maxCubeWidth, maxCubeHeigth);
        if(pipeToggle.isOn)
            SetNumberOfPuzzlesDropdownContent(maxPipeWidth, maxPipeHeigth);

        //smaz stare
        for (int i = puzzlePanels.Count - 1; i >= 0; i--)
        {
            Destroy(puzzlePanels[i]);
            puzzlePanels.RemoveAt(i);
            texturePaths.RemoveAt(i);
        }
        //insanciuj nove
        for (int i = 0; i < numberOfPuzlesDropwdown.value + 1; i++)
        {
            int iForDelegate = i;
            var p = Instantiate(puzzlePanelPrefab, scrollViewContent.transform);
            puzzlePanels.Add(p);
            if (cubeToggle.isOn)/////////
                p.GetComponentInChildren<Button>().onClick.AddListener(delegate { OnSelectPuzzleImageClick(iForDelegate); });
            else
                p.GetComponentInChildren<Button>().image.sprite = MenuLogic.instance.pipeImage;/////////
            p.GetComponentInChildren<InputField>().onValueChanged.AddListener(delegate { OnInputTextEdited(p.GetComponentInChildren<InputField>()); });
            texturePaths.Add(null);
        }
    }

    private void SetNumberOfPuzzlesDropdownContent(int maxWidth, int maxHeigth)
    {
        //podle toho nastavi dropdowny u nastavovani sirek/vysek obrazku
        List<string> widths = new List<string>();
        for (int i = 1; i <= maxWidth; i++)
        {
            widths.Add(i.ToString());
        }
        List<string> heigths = new List<string>();
        for (int i = 1; i <= maxHeigth; i++)
        {
            heigths.Add(i.ToString());
        }
        List<Dropdown> droplist = new List<Dropdown>();
        puzzlePanelPrefab.GetComponentsInChildren<Dropdown>(droplist);
        droplist[0].ClearOptions();
        droplist[0].AddOptions(widths);
        droplist[1].ClearOptions();
        droplist[1].AddOptions(heigths);
    }

}
