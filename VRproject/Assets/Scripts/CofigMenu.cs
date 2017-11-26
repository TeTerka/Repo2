using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.UI;
using System.IO;

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
    public float tileSize;
    public GameObject table;

    private int maxWidth;
    private int maxHeigth;

    [Header("References to UI elements")]
    public Toggle npcToggle;
    public Toggle tutorialToggle;
    public Dropdown numberOfPuzlesDropwdown;
    public InputField configNameField;
    public GameObject puzzlePanelPrefab;
    public GameObject scrollViewContent;
    public GameObject blockingPanel;
    public GameObject errorText;
    //new stuff
    public InputField timeLimitField;
    public Dropdown modelDropdown;
    public Dropdown behaviourDropdown;
    //

    public ExpMenu em;

    //seznamy...
    private List<string> texturePaths = new List<string>();
    private List<GameObject> puzzlePanels = new List<GameObject>();
    int activeButton = 0;

    private void Start()
    {
        //zjisti kolik se na stul vejde max kostek na vysku/sirku
        tileSize = NewManager.instance.tileSize;
        tableWidth = table.transform.lossyScale.x;
        tableHeigth = table.transform.lossyScale.z;

        maxWidth = (int)((tableWidth / 3) / tileSize);
        maxHeigth = (int)(tableHeigth / tileSize);

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
                p.GetComponentInChildren<Button>().onClick.AddListener(delegate { OnSelectPuzzleImageClick(iForDelegate); });
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

    public void OnSelectPuzzleImageClick(int i)//kliknuti na button na panelu (=nastavovani png)
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
            //jenom to naznaci ze je tu problem (zobrazi missingImage), ale konfiguraci to dovoli vyrobit => musi se pozdeji znovu provest kontrola...
            if (puzzlePanels[activeButton].GetComponentInChildren<Button>().image.sprite == null)//neboli if picture loadig failed
            {
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
        if (configNameField.text == null || MenuLogic.instance.ContainsWhitespaceOnly(configNameField.text))
        {
            ok = false;
            configNameField.image.color = Color.red;
        }
        else
        {
            configNameField.image.color = Color.white;
        }
        //new stuff
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
        //
        for (int i = 0; i <= numberOfPuzlesDropwdown.value; i++)
        {
            GameObject q = puzzlePanels[i];
            if (q.GetComponentInChildren<InputField>().text == null || MenuLogic.instance.ContainsWhitespaceOnly(q.GetComponentInChildren<InputField>().text))
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
            p.pathToImage = texturePaths[i];
            c.puzzles.Add(p);
        }

        c.modelName = modelDropdown.captionText.text;
        c.behaviourName = behaviourDropdown.captionText.text;

        //add it to the list
        MenuLogic.instance.availableConfigs.configs.Add(c);
        ////////////////////////tohle asi nebude pri kazdem save ne?.....co treba OnAppQuit nebo tak neco...
        ////////////////////////serialize it (all?)
        //////////////////////var ser = new XmlSerializer(typeof(ListOfConfigurations));
        //////////////////////var stream = new System.IO.FileStream(Application.dataPath+"/fff.xml", System.IO.FileMode.Create);
        //////////////////////ser.Serialize(stream, availableConfigs);
        //////////////////////stream.Close();

        //switch back to expMenu
        MenuLogic.instance.expMenuCanvas.SetActive(true);
        MenuLogic.instance.confMenuCanvas.SetActive(false);
        em.AddOneNewConfig(c);
    }
    public void OnCancelInConfigMenuClicked()
    {
        //cancel highlights
        configNameField.image.color = Color.white;
        for (int i = 0; i <= numberOfPuzlesDropwdown.value; i++)
        {
            GameObject q = puzzlePanels[i];
            if (q.GetComponentInChildren<InputField>().text == null || MenuLogic.instance.ContainsWhitespaceOnly(q.GetComponentInChildren<InputField>().text))
            {
                q.GetComponentInChildren<InputField>().image.color = Color.white;
                q.GetComponentInChildren<Button>().image.color = Color.white;
            }
        }
        errorText.SetActive(false);

        //switch to expMenu
        MenuLogic.instance.expMenuCanvas.SetActive(true);
        MenuLogic.instance.confMenuCanvas.SetActive(false);
    }
}
