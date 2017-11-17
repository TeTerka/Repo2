using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour {

    public ChooseMenu cm;

    public void OnChooseExperimentClicked()
    {
        MenuLogic.instance.chooseMenuCanvas.SetActive(true);
        MenuLogic.instance.mainMenuCanvas.SetActive(false);

        //cm.RefreshExpsList();
        cm.AddAllNewExpToAvailable();
    }

    public void OnCreateNewExperimentClicked()
    {
        MenuLogic.instance.expMenuCanvas.SetActive(true);
        MenuLogic.instance.mainMenuCanvas.SetActive(false);
    }
}
