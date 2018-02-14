using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using UnityEngine.UI;
using System.IO;

//ovladani pro CreateExperimentCanvas

public class ExpMenu : MonoBehaviour {

    [Header("UI refernces")]
    public InputField expName;
    public GameObject errorText;
    public GameObject chosenConfigScrollViewContent;
    public GameObject availableConfigScrollViewContent;
    private GameObject InnerScrollViewContent;

    [Header("PopupPanel")]
    public GameObject popupPanel;
    public Button yesButton;
    public Text popupConfNameText;

    [Header("prefabs")]
    public Button availableConfigInfoButtonPrefab;
    public GameObject puzzleInfoPanelPrefab;

    //lists...
    private List<Configuration> chosenConfigs = new List<Configuration>();
    private List<Button> chosenConfigInfoButtons = new List<Button>();
    //private List<Button> availableConfigInfoButtons = new List<Button>();
    private List<GameObject> puzzleInfoPanels = new List<GameObject>();

    private string puzzleType;
    public Text headline;

    private void Start()
    {
        ////if there is a config file ----- musim kontrolovat aby to nespadlo atak....staci File.Exists?....asi zatim jo
        ////"load" it
        //if (File.Exists(Application.dataPath + "/fff.xml"))
        //{
        //    var ser = new XmlSerializer(typeof(ListOfConfigurations));
        //    using (var stream = new FileStream(Application.dataPath + "/fff.xml", FileMode.Open))
        //    {
        //        MenuLogic.instance.availableConfigs = ser.Deserialize(stream) as ListOfConfigurations;
        //    }
        //}
        //
        //for (int i = 0; i < MenuLogic.instance.availableConfigs.configs.Count; i++)
        //{
        //    AddOneNewConfig(MenuLogic.instance.availableConfigs.configs[i]);
        //}
    }

    public void AddOneNewConfig(Configuration c)//gets called after creating new configuration
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
            var q = Instantiate(puzzleInfoPanelPrefab, InnerScrollViewContent.transform);
            q.GetComponentInChildren<Text>().text = c.puzzles[j].widthpx + " x " + c.puzzles[j].heigthpx;
            List<Image> images = new List<Image>();
            q.GetComponentsInChildren<Image>(images);
            if (c.puzzleType=="PipePuzzle")
            {
                images[1].sprite = MenuLogic.instance.pipeImage;
            }
            if (c.puzzleType == "CubePuzzle")
            {
                images[1].sprite = MenuLogic.instance.LoadNewSprite(c.puzzles[j].pathToImage);
                if (images[1].sprite == null)//neboli if picture loadig failed
                {
                    images[1].sprite = MenuLogic.instance.missingImage;
                }
            }
        }

        //make the button clickable (so that it can be selected for an experiment...and deletable by right clicking)
        p.gameObject.GetComponent<RightClick>().leftClick.AddListener(delegate { OnAvailableConfigClick(p, c); });
        p.gameObject.GetComponent<RightClick>().rightClick.AddListener(delegate { OnRightClick(p, c); });
        //////////p.onClick.AddListener(delegate { OnAvailableConfigClick(p, c); });

        //availableConfigInfoButtons.Add(p);
    }

    public void OnAvailableConfigClick(Button availableButton,Configuration c)//selects this button/config for experiment (and put it to chosenConfigsList)
    {
        if (c.puzzleType != this.puzzleType)//******************************************************************
            return;

        Button chosenButton = Instantiate(availableButton, chosenConfigScrollViewContent.transform);
        chosenConfigInfoButtons.Add(chosenButton);
        chosenConfigs.Add(c);
        chosenButton.onClick.AddListener(delegate { OnChosenConfigClick(chosenButton, c); });//make it removable from experiment
    }
    public void OnChosenConfigClick(Button b, Configuration c)//removes this button/config from experiment (and removes it from chosenConfigsList)
    {
        chosenConfigs.Remove(c);
        b.gameObject.SetActive(false);
        Destroy(b.gameObject);
        chosenConfigInfoButtons.Remove(b);
    }

    public void OnRightClick(Button b, Configuration c)//pravym kliknutim se configurace zcela vymaze (ze senamu available konfiguraci)
        //a co jiz vytvorene experimenty, ktere tyto konfigurace pouzivaji? maji se smazat?nebo ne? vadi to?.......
        //je vubec tahle moznost mazani potreba?
    {
        popupPanel.SetActive(true);
        yesButton.onClick.RemoveAllListeners();
        yesButton.onClick.AddListener(delegate { DeleteConfig(b, c); });
        popupConfNameText.text = c.name;
    }

    public void DeleteConfig(Button b, Configuration c)//yes click
    {
        Destroy(b.gameObject);
        MenuLogic.instance.availableConfigs.configs.Remove(c);
        for (int i = chosenConfigs.Count - 1; i >= 0; i--)
        {
            if (chosenConfigs[i].name == c.name)//ehm, porovnavat to podle jmena?...ale ted me nenapada nic lepsiho...
            {
                Destroy(chosenConfigInfoButtons[i].gameObject);
                chosenConfigInfoButtons.RemoveAt(i);
                chosenConfigs.RemoveAt(i);
            }
        }
        popupPanel.SetActive(false);
    }

    public void NoClick()
    {
        popupPanel.SetActive(false);
    }


    //obsluhy tech tri tlacitek na CreateExperimentCanvas:
    public void OnCreateNewConfigClicked()
    {
        MenuLogic.instance.confMenuCanvas.SetActive(true);
        MenuLogic.instance.expMenuCanvas.SetActive(false);
    }

    public void OnCancelInExpMenuClicked()
    {
        //clear stuff...
        //...maybe

        //switch menus
        MenuLogic.instance.mainMenuCanvas.SetActive(true);
        MenuLogic.instance.expMenuCanvas.SetActive(false);
        ClearThisMenuPage();

    }

    public void OnSaveExperimentClick()
    {
        //create only if everything is correctly filled out (corretly = has a name and consists of at leat one configuration)
        bool ok = true;
        if (expName.text == null || MenuLogic.instance.ContainsWhitespaceOnly(expName.text))
        {
            ok = false;
            expName.image.color = Color.red;
        }
        else
        {
            expName.image.color = Color.white;
        }
        if (chosenConfigs.Count == 0)
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
        e.puzzleType = puzzleType;//*********************************************
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
            using (StreamWriter sw = new StreamWriter(e.resultsFile, true))//true for append
            {
                sw.WriteLine("id,config name,puzzle name,width,heigth,time spent,score");
                sw.Close();
            }
            //create file with experiment info
            var ser = new XmlSerializer(typeof(ListOfConfigurations));
            using (var stream = new System.IO.FileStream(Application.dataPath + "/" + e.name + j + "/configsInfo.xml", System.IO.FileMode.Create))
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

    public void SetPuzzleType(string type)
    {
        puzzleType = type;
        headline.text = "Create new "+type+" experiment";
    }

    private void ClearThisMenuPage()//clear only selected configs, not (yet) the available configs...
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