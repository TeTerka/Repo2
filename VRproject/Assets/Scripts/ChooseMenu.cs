using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls for the "choose experiment" menu page
/// </summary>
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

    /// <summary>
    /// makes sure that list of available experimnts contains all of the available experiments
    /// </summary>
    public void AddAllNewExpToAvailable()
    {
        for (int i = expButtons.Count; i < MenuLogic.instance.availableExperiments.experiments.Count; i++)
        {
            AddExpToAvailables(MenuLogic.instance.availableExperiments.experiments[i]);
        }
    }

    /// <summary>
    /// adds new experiment to the list of available experiments
    /// </summary>
    /// <param name="e">added experiment</param>
    private void AddExpToAvailables(Experiment e)
    {
        Button b = Instantiate(expNameButtonPrefab, availableExpsScrollViewContent.transform);
        b.GetComponentInChildren<Text>().text = e.name;
        b.GetComponent<RightClick>().leftClick.AddListener(delegate { OnAvailableExpClick(b,e); });
        b.GetComponent<RightClick>().rightClick.AddListener(delegate { OnRightClick(b, e); });
        expButtons.Add(b);
    }

    /// <summary>
    /// choose this experiment and show info about it
    /// </summary>
    /// <param name="b">button representing chosen experiment</param>
    /// <param name="e">chosen experiment</param>
    public void OnAvailableExpClick(Button b, Experiment e)
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

            AbstractPuzzle currentPuzzleType=null;
            foreach (AbstractPuzzle puzzleType in NewManager.instance.puzzleTypes)
            {
                if (puzzleType.typeName == e.puzzleType)
                    currentPuzzleType = puzzleType;
            }
            if(currentPuzzleType==null)
            {
                ErrorCatcher.instance.Show("Unknown type of puzzle");
                return;
            }

            InnerScrollViewContent = p.GetComponentInChildren<ContentSizeFitter>().gameObject;
            for (int j = 0; j < c.puzzles.Count; j++)//inside fill out info about each puzzle that the configuration contains
            {
                var q = Instantiate(currentPuzzleType.infoPanelPrefab, InnerScrollViewContent.transform);
                bool success = currentPuzzleType.FillTheInfoPanel(q, c.puzzles[j]);
                if (!success)
                    loadSuccessful = false;
            }
            if(!File.Exists(e.resultsFile))//in case of problems with access to results file
            {
                loadSuccessful = false;
                missingStuff += e.resultsFile + "\n";
            }
        }
    }

    /// <summary>
    /// brings up a popup panel "Do you really want to delete this experiment?"
    /// </summary>
    /// <param name="b">button representing the experiment to be deleted</param>
    /// <param name="e">experiment to be deleted</param>
    public void OnRightClick(Button b, Experiment e)
    {
        popupPanel.SetActive(true);
        yesButton.onClick.RemoveAllListeners();
        yesButton.onClick.AddListener(delegate { DeleteExperiment(b, e); });
        popupExpNameText.text = e.name;
    }

    /// <summary>
    /// completely deletes an experiment
    /// </summary>
    /// <param name="b">button representing the deleted experiment</param>
    /// <param name="e">deleted experiment</param>
    public void DeleteExperiment(Button b,Experiment e)
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
        popupPanel.SetActive(false);
    }

    /// <summary>
    /// deactivates the popup panel
    /// </summary>
    public void NoClick()
    {
        popupPanel.SetActive(false);
    }

    /// <summary>
    /// action for "Test experiment" button
    /// </summary>
    public void OnTestClicked()
    {
        StartChosenExperinemt(true);
    }

    /// <summary>
    /// action for "Start experiment" button
    /// </summary>
    public void OnRunClicked()
    {
        StartChosenExperinemt(false);
    }

    /// <summary>
    /// starts currently chosen experiment
    /// </summary>
    /// <param name="testOnly">true = run in test mode, false = run in normal mode</param>
    private void StartChosenExperinemt(bool testOnly)
    {
        if (chosenExperiment != null && loadSuccessful)
        {
            NewManager.instance.StartExperiment(chosenExperiment, testOnly,false);
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

    /// <summary>
    /// action for "Cancel" button
    /// </summary>
    public void OnCancelInChooseMenuClicked()
    {
        MenuLogic.instance.mainMenuCanvas.SetActive(true);
        MenuLogic.instance.chooseMenuCanvas.SetActive(false);
    }

    /// <summary>
    /// draws the file browser
    /// </summary>
    protected void OnGUI()
    {
        GUI.skin = customSkin;
        if (m_fileBrowser != null)
        {
            m_fileBrowser.OnGUI();
        }
    }

    /// <summary>
    /// brings up the file browser to choose a logfile to be replayed
    /// </summary>
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

    /// <summary>
    /// Starts replaying the selected logfile
    /// </summary>
    /// <param name="path">path to the selected file</param>
    protected void FileSelectedCallback(string path)
    {
        m_fileBrowser = null;        
        blockingPanel.SetActive(false);

        if (path != null)
        {
            //deserialize the configsInfo.xml (correctly adjust path to that)
            string newPath = path.Substring(0, path.LastIndexOf('\\') + 1) + "\\configsInfo.xml";
            ListOfConfigurations allConfigs = new ListOfConfigurations();
            try
            {
                if (File.Exists(newPath))
                {
                    var ser = new XmlSerializer(typeof(ListOfConfigurations));
                    using (var stream = new FileStream(newPath, FileMode.Open))
                    {
                        allConfigs = ser.Deserialize(stream) as ListOfConfigurations;
                    }
                }
                else
                {
                    ErrorCatcher.instance.Show("Wanted to deserialize configinfo but file " + newPath + " does not exist.");
                    return;
                }
            }
            catch (System.Exception exc)
            {
                ErrorCatcher.instance.Show("Wanted to deserialize file " + newPath + " but it threw error " + exc.ToString());
                return;
            }

            //load the file
            Logger.instance.SetLoggerPath(path);
            string id = null;
            string name = null;
            try
            {
                if (!File.Exists(path))
                {
                    ErrorCatcher.instance.Show("Wanted to read logfile " + path + " but it does not exist.");
                    return;
                }
                using (StreamReader file = new StreamReader(path))
                {
                    //read info from first 2 lines of log file (there should be the config name and th eplayer id)
                    name = file.ReadLine();
                    id = file.ReadLine();
                    file.Close();
                }
            }
            catch(System.Exception exc)
            {
                ErrorCatcher.instance.Show("Wanted to read logfile " + path + " but it threw error " + exc.ToString());
                return;
            }
            //find this in all the configs
            Configuration c = null;
            foreach (Configuration conf in allConfigs.configs)
            {
                if (conf.name == name)
                {
                    c = conf;
                    break;
                }
            }
            //check if it is ok
            if (c == null)
            {
                ErrorCatcher.instance.Show("The config "+name+" in logfile " + path + " does not exist.");
                return;
            }
            //create fake experiment containing only this config (but dont create any result folders etc., also dont add it to MenuLogic list of experiments)
            Experiment e = new Experiment();
            e.name = "Replay " + id + ", " + name;
            e.puzzleType = c.puzzleType;
            e.configs = new List<Configuration> { c };
            //start that experiment in replay mode (so that the rest of the log file will be used to simulate player&coordinator actions)
            NewManager.instance.StartExperiment(e, false, true);
            MenuLogic.instance.chooseMenuCanvas.SetActive(false);
            MenuLogic.instance.spectatorCanvas.SetActive(true);
        }
    }
}
