using System.Collections.Generic;
using UnityEngine;

//ovladani pro NewSpectatorCanvas

public class UImanagerScript : MonoBehaviour {

    //prepinani kamer
    public List<GameObject> cams = new List<GameObject>();
    private int camCount;
    private int currentCam;

    //vyskakujici yes/no okno
    public GameObject popupPanel;

    private void Start()
    {
        camCount = cams.Count;
        currentCam = 0;
    }

    //v cyklu prepina mezi ruznymi kamerami pro koordinatora
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

    public void NextPhaseButtonClick()
    {
        NewManager.instance.TrySwitchPhase(false);
    }

    public void NoButtonClick()
    {
        popupPanel.SetActive(false);
    }

    public void YesButtonClick()
    {
        popupPanel.SetActive(false);
        NewManager.instance.TrySwitchPhase(true);
    }


    public void RestartButtonClick()
    {
        //finish current phase
        //myslim ze je to ok i pro main phase, protoze pomoci bool phaseFinished dokaze rozpoznat, ze timhle restartem byla ukonce predcasne => neulozi data...
        //co kdyz hrac dokoncil main phase, ale koordinator nedal next, dal misto toho restrart? TO BY SE DATA ULOZILA!!! a to asi nechci.......je treba vyresit...
        //initiate welcome phase
    }

    private bool timeStopped = false;
    public void PauseButtonClick()
    {
        //funguje pouze pro ReplayMode, v normalnim modu by nemelo smysl

        if(NewManager.instance.InReplayMode)
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
    public void MakeSureTimeIsntStopped()//nutne kdyby kliknul na quit zatimco je prehravani pauznute
    {
        Time.timeScale = 1;
        timeStopped = false;
    }

}
