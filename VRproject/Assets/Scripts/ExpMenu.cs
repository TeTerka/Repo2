using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using UnityEngine.UI;
using System.IO;

/// <summary>
/// Controls for the "create experiment" menu page
/// </summary>
public class ExpMenu : MonoBehaviour {

    [Header("UI reference")]
    public InputField expName;
    public GameObject errorText;
    public GameObject chosenConfigScrollViewContent;
    public GameObject availableConfigScrollViewContent;
    private GameObject InnerScrollViewContent;
    public CofigMenu cm;

    [Header("PopupPanel")]
    public GameObject popupPanel;
    public Button yesButton;
    public Text popupConfNameText;

    [Header("prefabs")]
    public Button availableConfigInfoButtonPrefab;

    [Header("to distinguish various puzzle types")]
    public Text headline;
    private AbstractPuzzle currentPuzzleType;

    //lists...
    private List<Configuration> chosenConfigs = new List<Configuration>();
    private List<Button> chosenConfigInfoButtons = new List<Button>();

    /// <summary>
    /// adds a configuration to the list of available configurations
    /// </summary>
    /// <param name="c">added configuration</param>
    public void AddOneNewConfig(Configuration c)
    {
        //instantiate a config info button from prefab and fill it out with config info
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
        texts[3].text = c.timeLimit.ToString();

        InnerScrollViewContent = p.GetComponentInChildren<ContentSizeFitter>().gameObject;
        for (int j = 0; j < c.puzzles.Count; j++)//inside fill out info about each puzzle that the configuration contains
        {
            var q = Instantiate(currentPuzzleType.infoPanelPrefab, InnerScrollViewContent.transform);
            currentPuzzleType.FillTheInfoPanel(q, c.puzzles[j]);
        }

        //make the button clickable (so that it can be selected for an experiment by left clicking or deleted by right clicking)
        p.gameObject.GetComponent<RightClick>().leftClick.AddListener(delegate { OnAvailableConfigClick(p, c); });
        p.gameObject.GetComponent<RightClick>().rightClick.AddListener(delegate { OnRightClick(p, c); });
    }

    /// <summary>
    /// adds a configuration to the list of selected configurations (selects it for the experiment)
    /// </summary>
    /// <param name="availableButton">button representing the added configuration</param>
    /// <param name="c">added configuration</param>
    public void OnAvailableConfigClick(Button availableButton,Configuration c)
    {
        if (c.puzzleType != this.currentPuzzleType.typeName)//make sure the experiment consists of configurations of the right type
            return;

        foreach (var item in chosenConfigs)//make sure the experiment cosists of uniquely named configurations
        { 
            if (c.name == item.name)
                return;
        }

        Button chosenButton = Instantiate(availableButton, chosenConfigScrollViewContent.transform);
        chosenConfigInfoButtons.Add(chosenButton);
        chosenConfigs.Add(c);
        chosenButton.onClick.AddListener(delegate { OnChosenConfigClick(chosenButton, c); });//make it removable from experiment
    }

    /// <summary>
    /// deletes configuration from list of selected configurations (from the experiment)
    /// </summary>
    /// <param name="b">button representing the deleted configuration</param>
    /// <param name="c">deleted configuration</param>
    public void OnChosenConfigClick(Button b, Configuration c)
    {
        //remove the configuration
        chosenConfigs.Remove(c);
        //remove the button
        b.gameObject.SetActive(false);
        Destroy(b.gameObject);
        chosenConfigInfoButtons.Remove(b);
    }

    /// <summary>
    /// brings up a popup panel "Do you really want to completely delete this config?"
    /// </summary>
    /// <param name="b">button representing the configuration to be deleted</param>
    /// <param name="c">configuration to be deleted</param>
    public void OnRightClick(Button b, Configuration c)
    {
        popupPanel.SetActive(true);
        yesButton.onClick.RemoveAllListeners();
        yesButton.onClick.AddListener(delegate { DeleteConfig(b, c); });
        popupConfNameText.text = c.name;
    }

    /// <summary>
    /// deletes configuration from the list of available configurations
    /// </summary>
    /// <param name="b">button representing the deleted configuration</param>
    /// <param name="c">deleted configuration</param>
    public void DeleteConfig(Button b, Configuration c)
    {
        Destroy(b.gameObject);
        MenuLogic.instance.availableConfigs.configs.Remove(c);
        for (int i = chosenConfigs.Count - 1; i >= 0; i--)
        {
            if (chosenConfigs[i].name == c.name)
            {
                Destroy(chosenConfigInfoButtons[i].gameObject);
                chosenConfigInfoButtons.RemoveAt(i);
                chosenConfigs.RemoveAt(i);
            }
        }
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
    /// action for "Create New Config" button
    /// </summary>
    public void OnCreateNewConfigClicked()
    {
        MenuLogic.instance.confMenuCanvas.SetActive(true);
        MenuLogic.instance.expMenuCanvas.SetActive(false);
        cm.SetPuzzleType(currentPuzzleType.typeName);
    }

    /// <summary>
    /// action for "Cancel" button
    /// </summary>
    public void OnCancelInExpMenuClicked()
    {
        //switch menu pages
        MenuLogic.instance.mainMenuCanvas.SetActive(true);
        MenuLogic.instance.expMenuCanvas.SetActive(false);
        ClearThisMenuPage();

    }

    /// <summary>
    /// action for "Save" button
    /// </summary>
    /// <remarks>
    /// creates a new experiment containing selected configurations and creates its folder and a results file on disc, then switches to main menu page
    /// </remarks>
    public void OnSaveExperimentClick()
    {
        //create only if everything is correctly filled out (corretly = has a name and consists of at leat one configuration)
        bool ok = true;
        if (expName.text == null || MenuLogic.instance.ContainsWhitespaceOnly(expName.text))//correct name
        {
            ok = false;
            expName.image.color = Color.red;
        }
        else
        {
            expName.image.color = Color.white;
        }
        if (chosenConfigs.Count == 0)//contains at least on config
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
            errorText.gameObject.SetActive(false);
        }
        //create experiment (class)
        Experiment e = new Experiment();
        e.name = expName.text;
        e.puzzleType = currentPuzzleType.typeName;
        for (int i = 0; i < chosenConfigs.Count; i++)
        {
            e.configs.Add(chosenConfigs[i]);
        }
        try
        {
            //create folder for results
            int j = 0;
            while (Directory.Exists(Application.dataPath + "/" + e.name + j))
            {
                j++;
            }
            Directory.CreateDirectory(Application.dataPath + "/" + e.name + j);
            //create file for results
            var f = File.Create(Application.dataPath + "/" + e.name + j + "/results.csv");
            f.Close();
            e.resultsFile = Application.dataPath + "/" + e.name + j + "/results.csv";
            //add a header to the file
            using (StreamWriter sw = new StreamWriter(e.resultsFile, true))
            {
                sw.WriteLine("id,config name,puzzle name,width,heigth,time spent,score");
                sw.Close();
            }
            //create file with experiment info
            var ser = new XmlSerializer(typeof(ListOfConfigurations));
            using (var stream = new FileStream(Application.dataPath + "/" + e.name + j + "/configsInfo.xml", FileMode.Create))
            {
                ListOfConfigurations loc = new ListOfConfigurations();
                loc.configs = e.configs;
                ser.Serialize(stream, loc);
                stream.Close();
            }
        }
        catch(System.Exception exc)
        {
            ErrorCatcher.instance.Show("Wanted to create files for new experiment " + e.name + " but it threw error " + exc.ToString());
            return;
        }

        //add it to the list
        MenuLogic.instance.availableExperiments.experiments.Add(e);

        //switch back to mainMenu
        MenuLogic.instance.mainMenuCanvas.SetActive(true);
        MenuLogic.instance.expMenuCanvas.SetActive(false);
        ClearThisMenuPage();
    }

    /// <summary>
    /// sets the current puzzle type to be <paramref name="type"/>
    /// </summary>
    /// <remarks>
    /// should be called everytime when switching to this menu page
    /// </remarks>
    /// <param name="type">puzzle type</param>
    public void SetPuzzleType(string type)
    {
        foreach (AbstractPuzzle puzzleType in NewManager.instance.puzzleTypes)
        {
            if (puzzleType.typeName == type)
                currentPuzzleType = puzzleType;
        }
        headline.text = "Create new "+type+" experiment";
    }

    /// <summary>
    /// clears name field and all selected configurations
    /// should be called everytime when switching away from this menu page
    /// </summary>
    private void ClearThisMenuPage()
    {
        expName.text = "";
        expName.image.color = Color.white;
        errorText.gameObject.SetActive(false);

        for (int i=chosenConfigInfoButtons.Count-1;i>=0;i--)
        {
            Destroy(chosenConfigInfoButtons[i].gameObject);
        }
        chosenConfigs.Clear();
        chosenConfigInfoButtons.Clear();
    }


}