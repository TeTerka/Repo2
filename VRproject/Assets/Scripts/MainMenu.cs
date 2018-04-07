using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls for the "main menu" menu page
/// </summary>
public class MainMenu : MonoBehaviour {

    [Header("references to other menu pages")]
    public ChooseMenu cm;
    public ExpMenu em;

    [Header("for generating various types of puzzles")]
    public Button buttonPrefab;
    public GameObject buttonPanel;

    /// <summary>
    /// switch to the "choose experiment" menu page
    /// </summary>
    public void OnChooseExperimentClicked()
    {
        MenuLogic.instance.chooseMenuCanvas.SetActive(true);
        MenuLogic.instance.mainMenuCanvas.SetActive(false);

        cm.AddAllNewExpToAvailable();
    }

    /// <summary>
    /// switch to the "create experiment" menu page
    /// </summary>
    /// <param name="typeName">type of experiment that will be created there</param>
    public void OnCreateExpClick(string typeName) 
    {
        MenuLogic.instance.expMenuCanvas.SetActive(true);
        MenuLogic.instance.mainMenuCanvas.SetActive(false);
        em.SetPuzzleType(typeName);
    }

    private void Start()
    {
        //create a button for each existing puzzle type
        foreach (AbstractPuzzle puzzleType in NewManager.instance.puzzleTypes)
        {
            AbstractPuzzle p = puzzleType;
            Button b = Instantiate(buttonPrefab,buttonPanel.transform);
            b.GetComponentInChildren<Text>().text = p.typeName;
            b.onClick.AddListener(delegate { OnCreateExpClick(p.typeName); });
        }
    }
}

