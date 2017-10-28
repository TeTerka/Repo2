using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//jen pomocne kresleni zelenych a cervenych ctverecku.....to tam pak nebude

public class CanvasUpdator : MonoBehaviour {

    public List<Image> imgs = new List<Image>();

    private void Start()
    {

    }
    private void OnGUI()
    {
        var arr = ManagerScript.instance.containerList;
        for (int i = 0; i <arr.Count ; i++)
        {
            if(arr[i].GetComponent<TileContainer>().Matches==true)
            {
                imgs[i].color = Color.green;
            }
            else
            {
                imgs[i].color = Color.red;
            }
        }
    }
}
