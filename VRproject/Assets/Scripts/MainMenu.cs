using UnityEngine;

//ovladani pro MainMenuCanvas

public class MainMenu : MonoBehaviour {

    public ChooseMenu cm;
    public ExpMenu em;

    public void OnChooseExperimentClicked()
    {
        MenuLogic.instance.chooseMenuCanvas.SetActive(true);
        MenuLogic.instance.mainMenuCanvas.SetActive(false);

        cm.AddAllNewExpToAvailable();
    }

    public void OnCreateNewExperimentClicked()
    {
        MenuLogic.instance.expMenuCanvas.SetActive(true);
        MenuLogic.instance.mainMenuCanvas.SetActive(false);
        em.SetPuzzleType("CubePuzzle");
    }

    public void OnCreateNewPipeExperimentClicked()
    {
        MenuLogic.instance.expMenuCanvas.SetActive(true);
        MenuLogic.instance.mainMenuCanvas.SetActive(false);
        em.SetPuzzleType("PipePuzzle");
    }
}
