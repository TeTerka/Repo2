using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.UI;
using System.IO;

public class Usage : MonoBehaviour
{
    [Header("FileBrowser")]
    public GUISkin customSkin;
    protected string m_textPath;

    protected FileBrowser m_fileBrowser;

    [SerializeField]
    protected Texture2D m_directoryImage,
                        m_fileImage;

    [Header("MenuLogic")]
    public float tableWidth;
    public float tableHeigth;
    public float tileSize;

    public GameObject table;

    private int maxWidth;
    private int maxHeigth;

    public ListOfConfigurations availableConfigs = new ListOfConfigurations();

    [Header("References to UI eleents")]
    public Toggle npcToggle;
    public Toggle tutorialToggle;
    public Dropdown numberOfPuzles;
    public InputField configName;
    public List<string> texturePaths = new List<string>();
    private List<GameObject> puzzlePanels = new List<GameObject>();
    public GameObject puzzlePanelPrefab;
    public GameObject scrollViewContent;

    int activeButton = 0;

    public GameObject blockingPanel;
    public GameObject errorText;

    private void Start()
    {
        tileSize = ManagerScript.instance.tileSize;
        tableWidth = table.transform.lossyScale.x;
        tableHeigth = table.transform.lossyScale.z;
        Prepare();
    }
    private void Prepare()
    {
        //zjisti kolik se na stul vejde max kostek na vysku/sirku
        maxWidth = (int)((tableWidth / 3) / tileSize);
        maxHeigth = (int)(tableHeigth / tileSize);

        Debug.Log(maxHeigth);
        Debug.Log(maxWidth);

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


        //vygeneruj panely do scroll view
        OnNumberDropdownChanged();

        //if there is a config file ----- musim kontrolovat aby to nespadlo atak....staci File.Exists?....asi jo
        //"load" it
        if (File.Exists(Application.dataPath + "/fff.xml"))
        {
            var ser = new XmlSerializer(typeof(ListOfConfigurations));
            using (var stream = new FileStream(Application.dataPath + "/fff.xml", FileMode.Open))
            {
                availableConfigs = ser.Deserialize(stream) as ListOfConfigurations;
            }
        }

        RefreshAvailableConfigs();
    }

    public void OnNumberDropdownChanged()
    {
        if (numberOfPuzles.value + 1 <= puzzlePanels.Count)
        {
            //smaz stare nadbytecne (pokud jich puvodne bylo vice nez novy pocet)
            for (int i = puzzlePanels.Count -1; i >= numberOfPuzles.value + 1; i--)
            {
                Destroy(puzzlePanels[i]);
                puzzlePanels.RemoveAt(i);
                texturePaths.RemoveAt(i);
            }
        }
        else
        {
            //insanciuj nove (pokud jich puvodne bylo mene nez novy pocet)
            for (int i = puzzlePanels.Count ; i < numberOfPuzles.value + 1; i++)
            {
                int iForDelegate = i;
                var p = Instantiate(puzzlePanelPrefab, scrollViewContent.transform);
                puzzlePanels.Add(p);
                p.GetComponentInChildren<Button>().onClick.AddListener(delegate { OnSelectPuzzleImageClick(iForDelegate); });
                p.GetComponentInChildren<InputField>().onValueChanged.AddListener(delegate { OnInputTextEdited(p.GetComponentInChildren<InputField>()); });
                texturePaths.Add(null);
            }
        }
    }

    protected void OnGUI()
    {
        GUI.skin = customSkin;
        if (m_fileBrowser != null)
        {
            m_fileBrowser.OnGUI();
        }
    }

    public void OnSelectPuzzleImageClick(int i)
    {
        blockingPanel.SetActive(true);
        activeButton = i;
        //GUI.skin = customSkin;
        m_fileBrowser = new FileBrowser(
            new Rect(100, 100, 600, 500),//tady by to mohlo byt nepevne...aby se to vzdycky veslo na obrazovku!!!....zatim nevim jak s tim oknem hybat...
            "Choose PNG File",
            FileSelectedCallback
        );

        m_fileBrowser.SelectionPattern = "*.png";//jak dovolit dva patterny? napr .png nebo .jpg???????//asi budu muset hrabnout do FileBrowseru
        m_fileBrowser.DirectoryImage = m_directoryImage;
        m_fileBrowser.FileImage = m_fileImage;
    }


    protected void FileSelectedCallback(string path)
    {
        m_fileBrowser = null;
        // m_textPath = path;
        texturePaths[activeButton] = path;
        if (path != null)//mozna nejaka sofistikovanejsi kontrola...jestli to je fakt obrazek, jestli neni read only....atd!
        {
            puzzlePanels[activeButton].GetComponentInChildren<Button>().image.sprite = LoadNewSprite(path);
        }
        else
        {
            puzzlePanels[activeButton].GetComponentInChildren<Button>().image.sprite = null;
        }
        blockingPanel.SetActive(false);
        puzzlePanels[activeButton].GetComponentInChildren<Button>().image.color = Color.white;//cancel possible error highlight..even if path is still null (he tried)
    }

    public void OnInputTextEdited(InputField i)
    {
        i.image.color = Color.white;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////
    //https://forum.unity.com/threads/generating-sprites-dynamically-from-png-or-jpeg-files-in-c.343735/
    public Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 100.0f)
    {

        // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference

        Sprite NewSprite = new Sprite();
        Texture2D SpriteTexture = LoadTexture(FilePath);
        NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), PixelsPerUnit);

        return NewSprite;
    }

    public Texture2D LoadTexture(string FilePath)
    {

        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails

        Texture2D Tex2D;
        byte[] FileData;

        if (File.Exists(FilePath))
        {
            FileData = File.ReadAllBytes(FilePath);
            Tex2D = new Texture2D(2, 2);           // Create new "empty" texture
            if (Tex2D.LoadImage(FileData))           // Load the imagedata into the texture (size is set automatically)
                return Tex2D;                 // If data = readable -> return texture
        }
        return null;                     // Return null if load failed
    }
    ////////////////////////////////////////////////////////////////////////////////////////////////////////

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
    public void OnSaveClick()
    {
        //create only if everything is correctly filled out
        bool ok = true;
        if (configName.text == null || ContainsWhitespaceOnly(configName.text))
        {
            ok = false;
            configName.image.color = Color.red;
        }
        else
        {
            configName.image.color = Color.white;
        }
        for (int i = 0; i <= numberOfPuzles.value; i++)
        {
            GameObject q = puzzlePanels[i];
            if (q.GetComponentInChildren<InputField>().text == null || ContainsWhitespaceOnly(q.GetComponentInChildren<InputField>().text))
            {
                ok = false;
                q.GetComponentInChildren<InputField>().image.color = Color.red;
            }
            else
            {
                q.GetComponentInChildren<InputField>().image.color = Color.white;
            }
            if (texturePaths[i] == null)
            {
                ok = false;
                q.GetComponentInChildren<Button>().image.color = Color.red;
            }
            else
            {
                q.GetComponentInChildren<Button>().image.color = Color.white;
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
        c.name = configName.text;
        c.withNPC = npcToggle.isOn;
        c.withTutorial = tutorialToggle.isOn;
        for (int i = 0; i <= numberOfPuzles.value; i++)
        {
            Puzzle p = new Puzzle();
            GameObject q = puzzlePanels[i];

            p.name = q.GetComponentInChildren<InputField>().text;
            List<Dropdown> droplist = new List<Dropdown>();
            q.GetComponentsInChildren<Dropdown>(droplist);
            p.heigthpx = droplist[1].value + 1;//jak ty 2 rozlisit/...<?
            p.widthpx = droplist[0].value + 1;
            p.pathToImage = texturePaths[i];
            c.puzzles.Add(p);
        }
        //add it to the list
        availableConfigs.configs.Add(c);
        Debug.Log("created"+ c.name);
        ////////////////////////tohle asi nebude pri kazdem save ne?.....co treba OnAppQuit nebo tak neco...
        ////////////////////////serialize it (all?)
        //////////////////////var ser = new XmlSerializer(typeof(ListOfConfigurations));
        //////////////////////var stream = new System.IO.FileStream(Application.dataPath+"/fff.xml", System.IO.FileMode.Create);
        //////////////////////ser.Serialize(stream, availableConfigs);
        //////////////////////stream.Close();

        //switch back to expMenu
        expMenuCanvas.SetActive(true);
        confMenuCanvas.SetActive(false);
        RefreshAvailableConfigs();
    }
    ///////////////////////////////////////////////////////////////////////////////////////
    //odsud dolu jsou veci pro create experiment menu canvas (nahore je to pro create configuration menu canvas)
    //////////////////////////////////////////////////////////////////////////////////////

    public Button availableConfigInfoButtonPrefab;
    private List<Button> availableConfigInfoButtons = new List<Button>();
    public GameObject availableConfigScrollViewContent;

    public GameObject expMenuCanvas;
    public GameObject confMenuCanvas;
    public GameObject mainMenuCanvas;
    public GameObject chooseMenuCanvas;

    public GameObject puzzleInfoPanelPrefab;
    private List<GameObject> puzzleInfoPanels = new List<GameObject>();
    private GameObject InnerScrollViewContent;

    private List<Button> chosenConfigInfoButtons = new List<Button>();
    public GameObject chosenConfigScrollViewContent;

    public InputField expName;
    public Text expErrorText;

    private ListOfExperiments availableExperiments = new ListOfExperiments();
    private List<Configuration> chosenConfigs = new List<Configuration>();
    public void RefreshAvailableConfigs()//asi by to pak nemel byt jeden MenuLogic, ale ConfMenu logic, exprmenu logic, mainMenu logic...
    {
        //remove old ones
        for (int i = availableConfigInfoButtons.Count-1; i >= 0; i--)
        {
            Destroy(availableConfigInfoButtons[i].gameObject);
            Debug.Log("destroyed");
        }
        availableConfigInfoButtons.Clear();
        //add all available configs
        for (int i = 0; i < availableConfigs.configs.Count; i++)
        {
            var c = availableConfigs.configs[i];
            var p = Instantiate(availableConfigInfoButtonPrefab, availableConfigScrollViewContent.transform);
            List<Text> texts = new List<Text>();
            p.GetComponentsInChildren<Text>(texts);
            texts[0].text = c.name;
            if (c.withNPC == true)
                texts[1].text = "with NPC";
            else
            {
                texts[1].text = "without NPC";
            }
            if (c.withTutorial == true)
                texts[2].text = "with tutorial";
            else
            {
                texts[2].text = "without tutorial";
            }

            InnerScrollViewContent = p.GetComponentInChildren<ContentSizeFitter>().gameObject;
            for (int j = 0; j < c.puzzles.Count; j++)
            {
                var q = Instantiate(puzzleInfoPanelPrefab, InnerScrollViewContent.transform);
                q.GetComponentInChildren<Text>().text = c.puzzles[j].widthpx + " x "+ c.puzzles[j].heigthpx;
                List<Image> images = new List<Image>();
                q.GetComponentsInChildren<Image>(images);
                images[1].sprite = LoadNewSprite(c.puzzles[j].pathToImage);
            }

            int iForDelegate = i;
            p.onClick.AddListener(delegate { OnAvailableConfigClick(iForDelegate); });
            availableConfigInfoButtons.Add(p);
            Debug.Log("added");
        }
    }

    public void OnAvailableConfigClick(int buttonIndex)
    {
        Button p = Instantiate(availableConfigInfoButtons[buttonIndex], chosenConfigScrollViewContent.transform);
        chosenConfigInfoButtons.Add(p);
        chosenConfigs.Add(availableConfigs.configs[buttonIndex]);
        var iForDelegate = chosenConfigInfoButtons.Count - 1;
        var buttonIndexD = buttonIndex;
        p.onClick.AddListener(delegate { OnChosenConfigClick(p,buttonIndexD); });
    }
    public void OnChosenConfigClick(Button b,int configIndex)
    {
        chosenConfigs.Remove(availableConfigs.configs[configIndex]);
        b.gameObject.SetActive(false);
        Destroy(b.gameObject);
        chosenConfigInfoButtons.Remove(b);
    }

    public void OnCreateNewConfigClicked()
    {
        confMenuCanvas.SetActive(true);
        expMenuCanvas.SetActive(false);
    }
    public void OnCreateNewExperimentClicked()
    {
        expMenuCanvas.SetActive(true);
        mainMenuCanvas.SetActive(false);
    }
    public void OnCancelInExpMenuClicked()
    {
        //clear stuff...
        //...
        //switch menus
        mainMenuCanvas.SetActive(true);
        expMenuCanvas.SetActive(false);

    }
    public void OnCancelInChooseMenuClicked()
    {
        mainMenuCanvas.SetActive(true);
        chooseMenuCanvas.SetActive(false);
    }
    public void OnChooseExperimentClicked()
    {
        chooseMenuCanvas.SetActive(true);
        mainMenuCanvas.SetActive(false);
    }
    public void OnCancelInConfigMenuClicked()
    {
        //cancel highlights
        configName.image.color = Color.white;
        for (int i = 0; i <= numberOfPuzles.value; i++)
        {
            GameObject q = puzzlePanels[i];
            if (q.GetComponentInChildren<InputField>().text == null || ContainsWhitespaceOnly(q.GetComponentInChildren<InputField>().text))
            {
                q.GetComponentInChildren<InputField>().image.color = Color.white;
                q.GetComponentInChildren<Button>().image.color = Color.white;
            }
        }
        errorText.SetActive(false);

        //switch to expMenu
        expMenuCanvas.SetActive(true);
        confMenuCanvas.SetActive(false);
    }
    public void OnSaveExperimentClick()
    {
        //create only if everything is correctly filled out
        bool ok = true;
        if (expName.text == null || ContainsWhitespaceOnly(expName.text))
        {
            ok = false;
            expName.image.color = Color.red;
        }
        else
        {
            expName.image.color = Color.white;
        }
        if(chosenConfigInfoButtons.Count==0)
        {
            ok = false;
        }
        if (!ok)
        {
            errorText.SetActive(true);
            return;
        }
        else
        {
            expErrorText.gameObject.SetActive(false);
        }
        //create experiment (class)
        Experiment e = new Experiment();
        e.name = expName.text;
        for (int i = 0; i < chosenConfigs.Count; i++)
        {
            e.configs.Add(chosenConfigs[i]);
        }
        //add it to the list
        availableExperiments.experiments.Add(e);
        Debug.Log("created experiment" + e.name);
       /////////////////////////////////////////tohle asi nebude pri kazdem save ne?.....co treba OnAppQuit nebo tak neco...
       /////////////////////////////////////////serialize it (all?)
       ///////////////////////////////////////var ser = new XmlSerializer(typeof(ListOfExperiments));
       ///////////////////////////////////////var stream = new System.IO.FileStream(Application.dataPath+"/eee.xml", System.IO.FileMode.Create);
       ///////////////////////////////////////ser.Serialize(stream, availableExperiments);
       ///////////////////////////////////////stream.Close();

        //switch back to mainMenu
        mainMenuCanvas.SetActive(true);
        expMenuCanvas.SetActive(false);
    }

}
