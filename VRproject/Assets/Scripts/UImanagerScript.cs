using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls for the spectator canvas
/// </summary>
public class UImanagerScript : MonoBehaviour {

    //switching cameras
    public List<GameObject> cams = new List<GameObject>();
    private int camCount;
    private int currentCam;

    public GameObject popupPanel;
    private bool timeStopped = false;

    /// <summary>
    /// action for "Pause" button, can stop time during the replay mode only
    /// </summary>
    public void PauseButtonClick()
    {
        if (NewManager.instance.InReplayMode)
        {
            if (timeStopped)
            {
                Time.timeScale = 1;
                timeStopped = false;
            }
            else
            {
                Time.timeScale = 0;
                timeStopped = true;
            }
        }
    }
    /// <summary>
    /// unfreezes the time
    /// </summary>
    public void MakeSureTimeIsntStopped()
    {
        Time.timeScale = 1;
        timeStopped = false;
    }

    private void Start()
    {
        camCount = cams.Count;
        currentCam = 0;
    }

    /// <summary>
    /// cycling through available cameras
    /// </summary>
    public void ChangeCamButtonClick()
    {
        cams[currentCam].SetActive(false);
        currentCam++;
        if (currentCam == camCount)
        {
            currentCam = 0;
        }
        cams[currentCam].SetActive(true);
    }

    /// <summary>
    /// action for "Next" button, tries to switch to next phase
    /// </summary>
    public void NextPhaseButtonClick()
    {
        NewManager.instance.TrySwitchPhase(false);
    }

    /// <summary>
    /// disables the popup panel
    /// </summary>
    public void NoButtonClick()
    {
        popupPanel.SetActive(false);
    }

    /// <summary>
    /// switches the phase without testing if current phase was compeled
    /// </summary>
    public void YesButtonClick()
    {
        popupPanel.SetActive(false);
        NewManager.instance.TrySwitchPhase(true);
    }

}
