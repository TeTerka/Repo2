using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//kontajner neboli jedno policko mrizky
//umi se highlightovat, vi jestli je obsazene, vi ktera krychle na nej patri,...

public class TileContainer : MonoBehaviour {

    //charakteristika
    private int containerIndex;
    //reference
    private Transform grid;//mrizka - vzhledem k ni se urcuji smery pri snapovani, referenci ziska od Manageru
    //stav
    public bool isEmpty = true;
    public bool Matches { get; set; }

    //highlitovani policek mrizky
    private Color basicColor = Color.white;
    private Color highlightColor = Color.red;
    private Renderer myRndr;

    public void Initialize(int index)//tahle funkce se vola pri vytvareni kontajneru (aby se nepsalo primo containerIndex = k;)
    {
        containerIndex = index;
    }

    public void Highlight()
    {
        myRndr.material.color = highlightColor;
    }
    public void CancelHighlight()
    {
        myRndr.material.color = basicColor;
    }

    private void Start()
    {
        grid = ManagerScript.instance.containersHolder.transform;
        myRndr = GetComponent<Renderer>();//prafab ho musi mit!
        Matches = false;
    }

    public void SetMatches(Transform tile, int tileIndex)//container zjisti, jestli na nej pasuje takto natoceni dilek (info o dilku predavano v parametrech)
    {
        int n = ManagerScript.instance.modelPictureNumber;//zjisti od Managera co za obrazek se ma skladat
        Matches = (CheckIndex(tileIndex) && CheckFace(tile, n) && CheckFaceRotation(tile, n));//zjisti jestli tedy dilek pasuje
    }

    private bool CheckIndex(int tileIndex)//spravna pozice?
    {
        return tileIndex == containerIndex;
    }
    private bool CheckFace(Transform cube, int n)//sparavnou stenou nahoru?
    {
        //textura
        // -------------------------
        // |       |       |       |
        // |       |       |       |
        // -------------------------
        // |       |       |       |
        // |   3   |   4   |   5   |
        // -------------------------
        // |       |       |       |
        // |   0   |   1   |   2   |
        // -------------------------
        switch (n)//podle toho, ktera strana ma byt nahore (cisla jsou podle textury)
        {
            //udaje jsou vypozorovany z toho, jak se mapuje textura na moji krychli...
            case 0: return grid.up == -cube.up;
            case 1: return grid.up == -cube.right;
            case 2: return grid.up == cube.right;
            case 3: return grid.up == cube.forward;
            case 4: return grid.up == cube.up;
            case 5: return grid.up == -cube.forward;
            default: return false;
        }
    }
    private bool CheckFaceRotation(Transform cube, int n)//spravne natoceno?
    {
        switch (n)//podle toho, ktera strana ma byt nahore (cisla jsou podle textury)
        {
            case 0: return grid.forward == cube.forward;
            case 1: return grid.forward == -cube.up;
            case 2: return grid.forward == cube.up;
            case 3: return grid.forward == -cube.right;
            case 4: return grid.forward == -cube.right;
            case 5: return grid.forward == -cube.right;
            default: return false;
        }
    }
}
