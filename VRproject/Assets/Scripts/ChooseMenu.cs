using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

//ovladani pro ChooseExpCanvas

public class ChooseMenu : MonoBehaviour {

    private Experiment chosenExperiment;

    [Header("UI references")]
    public GameObject availableExpsScrollViewContent;
    public GameObject expInfoScrollViewContent;
    public GameObject errorText;
    private GameObject InnerScrollViewContent;
    public GameObject otherErrorText;
    public Text runButtonText;

    [Header("Popup")]
    public GameObject popupPanel;
    public Button yesButton;
    public Text popupExpNameText;

    [Header("Prefabs")]
    public Button expNameButtonPrefab;
    public GameObject expConfInfoPanelPrefab;
    public GameObject puzzleInfoPanelPrefab;

    private List<Button> expButtons = new List<Button>();
    private List<GameObject> expInfoPanels = new List<GameObject>();
    private bool loadSuccessful;
    private string missingStuff;

    [Header("FileBrowser")]
    public GUISkin customSkin;
    protected FileBrowser m_fileBrowser;
    [SerializeField]
    protected Texture2D m_directoryImage,
                        m_fileImage;
    public GameObject blockingPanel;

    private void Start()
    {
        AddAllNewExpToAvailable();
    }

    public void AddAllNewExpToAvailable()
    {
        for (int i = expButtons.Count; i < MenuLogic.instance.availableExperiments.experiments.Count; i++)
        {
            AddExpToAvailables(MenuLogic.instance.availableExperiments.experiments[i]);
        }
    }

    private void AddExpToAvailables(Experiment e)
    {
        Button b = Instantiate(expNameButtonPrefab, availableExpsScrollViewContent.transform);
        b.GetComponentInChildren<Text>().text = e.name;
        b.GetComponent<RightClick>().leftClick.AddListener(delegate { OnAvailableExpClick(b,e); });
        b.GetComponent<RightClick>().rightClick.AddListener(delegate { OnRightClick(b, e); });
        expButtons.Add(b);
    }

    public void OnAvailableExpClick(Button b, Experiment e)//choose this experiment and show info about it (left click)
    {
        if(e.ids.Count>0)
        {
            runButtonText.text = "Continue experiment";
        }
        else
        {
            runButtonText.text = "Start experiment";
        }

        loadSuccessful = true;
        //choose it
        chosenExperiment = e;
        errorText.SetActive(false);
        //visualize the choice
        foreach (Button bu in expButtons)
            bu.image.color = Color.white;
        b.image.color = Color.green;
        //hide previous info
        for (int i = expInfoScrollViewContent.transform.childCount-1; i >=0; i--)
        {
            Destroy(expInfoScrollViewContent.transform.GetChild(i).gameObject);
        }
        //show info
        expInfoPanels.Clear();
        foreach (Configuration c in e.configs)
        {
            //instantiate a config info panel from prefab and fill it out with config info
            var p = Instantiate(expConfInfoPanelPrefab, expInfoScrollViewContent.transform);
            expInfoPanels.Add(p);
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
            texts[3].text = c.timeLimit.ToString();

            InnerScrollViewContent = p.GetComponentInChildren<ContentSizeFitter>().gameObject;
            for (int j = 0; j < c.puzzles.Count; j++)//inside fill out info about each puzzle that the configuration contains
            {
                var q = Instantiate(puzzleInfoPanelPrefab, InnerScrollViewContent.transform);
                q.GetComponentInChildren<Text>().text = c.puzzles[j].widthpx + " x " + c.puzzles[j].heigthpx;
                List<Image> images = new List<Image>();
                q.GetComponentsInChildren<Image>(images);
                if (e.puzzleType == "PipePuzzle")
                {
                    images[1].sprite = MenuLogic.instance.pipeImage;
                }
                if (e.puzzleType == "CubePuzzle")
                {
                    images[1].sprite = MenuLogic.instance.LoadNewSprite(c.puzzles[j].pathToImage);
                    //**************************************************************************************
                    //pozn.: predpokladam ze obsah xml nebude nikdo menit, jedine co se tedy muze pokazit je, ze ulozena cesta prestane vest k obrazku
                    //je tedy potreba to zkontrolovat:
                    if (images[1].sprite == null)//neboli if picture loadig failed
                    {
                        loadSuccessful = false;
                        images[1].sprite = MenuLogic.instance.missingImage;
                        missingStuff += c.puzzles[j].pathToImage + "\n";
                    }
                    //**************************************************************************************
                }
            }
            if(!System.IO.File.Exists(e.resultsFile))//jeste muze byt problem, ze prestane existovat slozka s vysledky, tak je to taky treba osetrit:
            {
                loadSuccessful = false;
                missingStuff += e.resultsFile + "\n";
            }
        }
    }

    public void OnRightClick(Button b, Experiment e)//pravym kliknutim se experiment zcela vymaze (ze senamu available experimentu)
                                                       //a co jiz "rozehrane" ci jiz dokoncene experimenty? mazat jim data z vysledku< asi ne, ne?
                                                       //takze to je jakoze mazany "sablony pro experiment"?
    {
        popupPanel.SetActive(true);
        yesButton.onClick.RemoveAllListeners();
        yesButton.onClick.AddListener(delegate { DeleteExperiment(b, e); });
        popupExpNameText.text = e.name;
    }

    public void DeleteExperiment(Button b,Experiment e)//yes click
    {
        if (chosenExperiment == e)
        {
            for (int i = expInfoPanels.Count- 1; i >= 0; i--)
            {
                Destroy(expInfoPanels[i]);
                expInfoPanels.RemoveAt(i);
            }
            chosenExperiment = null;
        }
        expButtons.Remove(b);
        Destroy(b.gameObject);
        MenuLogic.instance.availableExperiments.experiments.Remove(e);
        //RefreshExpsList();
        popupPanel.SetActive(false);
    }

    public void NoClick()
    {
        popupPanel.SetActive(false);
    }


    public void OnTestClicked()
    {
        StartChosenExperinemt(true);
    }

    public void OnRunClicked()
    {
        StartChosenExperinemt(false);
    }

    private void StartChosenExperinemt(bool testOnly)//true = only test run of the experiment (no data will be saved etc.)
    {
        if (chosenExperiment != null && loadSuccessful)
        {
            NewManager.instance.StartExperiment(chosenExperiment, testOnly);
            MenuLogic.instance.chooseMenuCanvas.SetActive(false);
            MenuLogic.instance.spectatorCanvas.SetActive(true);
        }
        else
        {
            if (chosenExperiment == null)
            {
                errorText.SetActive(true);
            }
            if (!loadSuccessful)
            {
                otherErrorText.gameObject.SetActive(true);
                otherErrorText.GetComponentInChildren<Text>().text = "missing file(s) at:\n" + missingStuff;
            }
        }
    }

    public void OnCancelInChooseMenuClicked()
    {
        MenuLogic.instance.mainMenuCanvas.SetActive(true);
        MenuLogic.instance.chooseMenuCanvas.SetActive(false);
    }

    public void OnReplayClick()
    {
        blockingPanel.SetActive(true);
        m_fileBrowser = new FileBrowser(
            new Rect(100, 100, Screen.width - 200, Screen.height - 200),
            "Choose a log file",
            FileSelectedCallback
        );

        m_fileBrowser.SelectionPattern = "*.mylog";
        m_fileBrowser.DirectoryImage = m_directoryImage;
        m_fileBrowser.FileImage = m_fileImage;
    }
    protected void FileSelectedCallback(string path)//nastavi nahled zvoleneho obrazku a zapamatuje si cestu k nemu
    {
        m_fileBrowser = null;        
        blockingPanel.SetActive(false);
       ////////////////////**load the file
       //////////////////logger.pathToLogFile = path;
       ////////////////////**deserialize the configsInfo.xml (correctly adjust path to that!)
       //////////////////ListOfConfigurations allConfigs = new ListOfConfigurations();
       //////////////////if (File.Exists(adjusted path + "/configsInfo.xml"))
       //////////////////{
       //////////////////    var ser = new XmlSerializer(typeof(ListOfConfigurations));
       //////////////////    using (var stream = new FileStream(adjusted path + "/configsInfo.xml", FileMode.Open))
       //////////////////    {        //        
       //////////////////        allConfigs = ser.Deserialize(stream) as ListOfConfigurations;
       //////////////////    }
       //////////////////}
       ////////////////////**read info from first line of log file (there should be the config name)
       //////////////////string name = ...
       ////////////////////**find this in all the configs
       //////////////////Configuration c = null;
       //////////////////foreach(Configuration conf in allConfigs)
       //////////////////{
       //////////////////    if (conf.name == name)
       //////////////////        c = conf;
       //////////////////}
       ////////////////////**check if it is ok
       //////////////////if(c==null)
       //////////////////{
       //////////////////    //errrrrrror
       //////////////////    return;
       //////////////////}
       ////////////////////**create fake exp containing only this config (but dont create any result folders etc., also dont add it to MenuLogic list of experiments...)
       //////////////////Experiment e = new Experiment();
       //////////////////e.name = "Replay "+id;
       //////////////////e.puzzleType = c.puzzleType;
       //////////////////e.configs = new List<Configuration> { c };
       ////////////////////**start that exp in replay mode (so that the rest of the log file will be used to simulate player&coordinator actions)
       //////////////////NewManager.instance.StartExperiment(e, replayOnly);
       //////////////////MenuLogic.instance.chooseMenuCanvas.SetActive(false);
       //////////////////MenuLogic.instance.spectatorCanvas.SetActive(true);
    }
}







//public void RefreshExpsList()
//{
//    //destroy old ones
//    for (int i = expButtons.Count-1; i >=0; i--)
//    {
//        Destroy(expButtons[i].gameObject);
//    }
//    expButtons.Clear();
//    //create new ones
//    for (int i = 0; i < MenuLogic.instance.availableExperiments.experiments.Count; i++)
//    {
//        int iForDelegate = i;
//        Button b = Instantiate(expNameButtonPrefab, availableExpsScrollViewContent.transform);
//        b.GetComponentInChildren<Text>().text = MenuLogic.instance.availableExperiments.experiments[i].name;
//        b.GetComponent<RightClick>().leftClick.AddListener(delegate { OnAvailableExpClick(b, MenuLogic.instance.availableExperiments.experiments[iForDelegate]); });
//        b.GetComponent<RightClick>().rightClick.AddListener(delegate { OnRightClick(b, MenuLogic.instance.availableExperiments.experiments[iForDelegate]); });
//        expButtons.Add(b);
//    }
//}