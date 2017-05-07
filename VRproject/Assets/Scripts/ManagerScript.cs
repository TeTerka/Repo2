using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerScript : MonoBehaviour {


    public GameObject VRavailable;
    public GameObject VRunavailable;
    private void Awake()
    {
        if(SteamVR.instance!=null)
        {
            ActivateRig(VRavailable);
        }
        else
        {
            ActivateRig(VRunavailable);
        }
    }

    private void ActivateRig(GameObject rig)
    {
        VRavailable.SetActive(rig == VRavailable);
        VRunavailable.SetActive(rig == VRunavailable);
    }
}
