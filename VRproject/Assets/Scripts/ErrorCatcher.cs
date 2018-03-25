using UnityEngine;
using UnityEngine.UI;



/// <summary>
/// for displaying error messages
/// </summary>
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

    /// <summary>
    /// shows a window with <paramref name="errorText"/> and stops time
    /// </summary>
    /// <param name="errorText">the displayed text</param>
    public void Show(string errorText)
    {
        catchedError = true;

        errorCanvas.SetActive(true);
        errorDescription.text = errorText;

        Logger.instance.StopLogger();
        Time.timeScale = 0;
    }

    /// <summary>
    /// exits application
    /// </summary>
    public void OnErrorQuitClicked()
    {
        Application.Quit();
    }

}
