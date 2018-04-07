using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls for the spectator canvas
/// </summary>
public class UImanagerScript : MonoBehaviour {

    //switching cameras

    /// <summary> list of available cameras in the virtual room </summary>
    public List<GameObject> cams = new List<GameObject>();
    private int camCount;
    private int currentCam;

    /// <summary> popup panel with Yes and No buttons </summary>
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
    /// action for "ChangeCamera" button, cycles through available cameras
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
    /// action for "No" button on the popup panel, disables the popup panel
    /// </summary>
    public void NoButtonClick()
    {
        popupPanel.SetActive(false);
    }

    /// <summary>
    /// action for "Yes" button on the popup panel, switches the phase without testing if current phase was compeled
    /// </summary>
    public void YesButtonClick()
    {
        popupPanel.SetActive(false);
        NewManager.instance.TrySwitchPhase(true);
    }

}
