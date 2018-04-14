using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls for the "create configuration" menu page
/// </summary>
public class ConfigMenu : MonoBehaviour {

    [Header("References to UI elements")]
    [SerializeField] private Toggle npcToggle;
    [SerializeField] private Toggle tutorialToggle;
    [SerializeField] private Dropdown numberOfPuzlesDropwdown;
    [SerializeField] private InputField configNameField;
    [SerializeField] private GameObject scrollViewContent;
    [SerializeField] private GameObject errorText;
    [SerializeField] private InputField timeLimitField;
    [SerializeField] private Dropdown modelDropdown;
    [SerializeField] private Dropdown behaviourDropdown;
    [SerializeField] private ExpMenu em;

    private List<GameObject> puzzlePanels = new List<GameObject>();

    [Header("to distinguish various puzzle types")]
    [SerializeField] private Text headline;
    private AbstractPuzzle currentPuzzleType;

    /// <summary>
    /// sets the current puzzle type to be <paramref name="type"/>
    /// </summary>
    /// <remarks>
    /// should be called everytime when switching to this menu page
    /// </remarks>
    /// <param name="type">puzzle type</param>
    public void SetPuzzleType(string type)
    {
        foreach(AbstractPuzzle puzzleType in NewManager.instance.puzzleTypes)
        {
            if(puzzleType.TypeName==type)
                currentPuzzleType = puzzleType;
        }
        headline.text = "Create new " + type + " configuration";
        for (int i = puzzlePanels.Count - 1; i==0; i--)
        {
            Destroy(puzzlePanels[i]);
            puzzlePanels.RemoveAt(i);
        }
        OnNumberDropdownChanged();
    }

    private void Start()
    {
        //prepare dropdowns for NPC models and NPC behaviours

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
    }

    /// <summary>
    /// refreshes the list of created puzzles so that it contains the newly selected number of puzzles
    /// </summary>
    public void OnNumberDropdownChanged()
    {
        if (numberOfPuzlesDropwdown.value + 1 <= puzzlePanels.Count)
        {
            //delete exess puzzles
            for (int i = puzzlePanels.Count - 1; i >= numberOfPuzlesDropwdown.value + 1; i--)
            {
                Destroy(puzzlePanels[i]);
                puzzlePanels.RemoveAt(i);
            }
        }
        else
        {
            //insantiate new puzzles
            for (int i = puzzlePanels.Count; i < numberOfPuzlesDropwdown.value + 1; i++)
            {
                int iForDelegate = i;
                var p = Instantiate(currentPuzzleType.interactibleInfoPanelPrefab, scrollViewContent.transform);
                puzzlePanels.Add(p);
                currentPuzzleType.PrepareInteractibleInfoPanel(p, iForDelegate);
            }
        }
    }

    /// <summary>
    /// cancel possible error highlight of <paramref name="i"/>
    /// </summary>
    /// <param name="i"></param>
    public void OnInputTextEdited(InputField i)
    {
        i.image.color = Color.white;
    }

    /// <summary>
    /// action for "Save" button, tries to create a new configuration containing the created puzzles and add it to the list of available configurations
    /// </summary>
    public void OnSaveClick()
    {
        //create only if everything is correctly filled out
        bool ok = true;
        if (configNameField.text == null || (!MenuLogic.instance.IsValidName(configNameField.text)))//valid name
        {
            ok = false;
            configNameField.image.color = Color.red;
        }
        else
        {
            configNameField.image.color = Color.white;
        }
        int time=42;
        if (timeLimitField.text == null || !int.TryParse(timeLimitField.text,out time) || time<=0)//valid time limit
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
            InputField puzName = currentPuzzleType.GetPuzzleName(q);
            if (puzName == null || puzName.text==null || (!MenuLogic.instance.IsValidName(puzName.text)))//valid puzzle name
            {
                ok = false;
                puzName.image.color = Color.red;
            }
            if(!currentPuzzleType.CheckFillingCorrect(q, i))//custom validity check
            {
                ok = false;
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

        //create configuration
        Configuration c = new Configuration();

        c.puzzleType = currentPuzzleType.TypeName;
        c.name = configNameField.text;
        c.withNPC = npcToggle.isOn;
        c.withTutorial = tutorialToggle.isOn;
        c.timeLimit = time;
        for (int i = 0; i <= numberOfPuzlesDropwdown.value; i++)
        {
            PuzzleData p = new PuzzleData();
            GameObject q = puzzlePanels[i];
            p = currentPuzzleType.CreatePuzzle(q, i);
            p.name = currentPuzzleType.GetPuzzleName(q).text;

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

    /// <summary>
    /// action for "Cancel" button, switches back to expMenu
    /// </summary>
    public void OnCancelInConfigMenuClicked()
    {
        //switch to expMenu
        MenuLogic.instance.expMenuCanvas.SetActive(true);
        MenuLogic.instance.confMenuCanvas.SetActive(false);

        CleanUp();
    }

    /// <summary>
    /// clears all the created puzzles
    /// should be called everytime when switching away from this menu page
    /// </summary>
    private void CleanUp()
    {
        for (int i = puzzlePanels.Count - 1; i >= 0; i--)
        {
            Destroy(puzzlePanels[i]);
            puzzlePanels.RemoveAt(i);
        }
        numberOfPuzlesDropwdown.value = 0;
        OnNumberDropdownChanged();
    }
}
