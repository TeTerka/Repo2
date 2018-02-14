using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorCatcher : MonoBehaviour {

    public static ErrorCatcher instance;

    public GameObject errorCanvas;
    public Text errorDescription;

    public bool catchedError;

    private void Awake()
    {
        instance = this;
        catchedError = false;
    }

    public void Show(string errorText)
    {
        catchedError = true;

        errorCanvas.SetActive(true);
        errorDescription.text = errorText;

        Logger.instance.StopLogger();
        //+nejak stopnout vsechno (koordinator je stopnuty pomoci bloskingPanelu, ale co hrac a updaty?)
        Time.timeScale = 0;//asi - trackovani hlavy a rukou normalne pokracuje, ale zastavi se vsechny Updaty, takze se nic moc neda udelat, jen se rozhlizet
        //mozna jeste setActive(false) obe ruce
    }

    public void OnErrorQuitClicked()
    {
        //........
        errorCanvas.SetActive(false);
        Application.Quit();//asi
    }

}
