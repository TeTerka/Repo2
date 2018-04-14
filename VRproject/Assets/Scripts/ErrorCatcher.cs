using UnityEngine;
using UnityEngine.UI;



/// <summary>
/// for displaying error messages
/// </summary>
public class ErrorCatcher : MonoBehaviour {

    public static ErrorCatcher instance;

    [SerializeField] private GameObject errorCanvas;
    [SerializeField] private Text errorDescription;

    /// <summary>states whether any error was catched</summary>
    public bool CatchedError { get; private set; }

    private void Awake()
    {
        instance = this;
        CatchedError = false;
    }

    /// <summary>
    /// shows a window with <paramref name="errorText"/> and stops time
    /// </summary>
    /// <param name="errorText">the displayed text</param>
    public void Show(string errorText)
    {
        CatchedError = true;

        errorCanvas.SetActive(true);
        errorDescription.text = errorText;

        if(Logger.instance!=null)
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
