using System.Collections.Generic;
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
                images[1].sprite = MenuLogic.instance.LoadNewSprite(c.puzzles[j].pathToImage);
                //**************************************************************************************
                //pozn.: predpokladam ze obsah xml nebude nikdo menit, jedine co se tedy muze pokazit je, ze ulozena cesta prestane vest k obrazku
                //je tedy potreba to zkontrolovat:
                if(images[1].sprite==null)//neboli if picture loadig failed
                {
                    loadSuccessful = false;
                    images[1].sprite = MenuLogic.instance.missingImage;
                    missingStuff += c.puzzles[j].pathToImage+"\n";
                }
                //**************************************************************************************
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
                otherErrorText.GetComponentInChildren<Text>().text = "missing filee(s) at:\n" + missingStuff;
            }
        }
    }

    public void OnCancelInChooseMenuClicked()
    {
        MenuLogic.instance.mainMenuCanvas.SetActive(true);
        MenuLogic.instance.chooseMenuCanvas.SetActive(false);
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