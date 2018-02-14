using UnityEngine;

//kontajner neboli jedno policko mrizky
//umi se highlightovat, vi jestli je obsazene, vi ktera krychle na nej patri,...

public class TileContainer : MonoBehaviour {

    //charakteristika
    public int ContainerIndex { get; private set; }//cislovani krychli/mrizky je od leveho dolniho rohu, napr. takto(pro rozlozeni 3x5):
    // ---------------------
    // |10 |11 |12 |13 |14 |
    // ---------------------
    // | 5 | 6 | 7 | 8 | 9 |
    // ---------------------
    // | 0 | 1 | 2 | 3 | 4 |
    // ---------------------

    //reference
    private Transform grid;//mrizka - vzhledem k ni se urcuji smery pri snapovani, referenci ziska od Manageru
    private CubePuzzle cp;
    //stav
    public bool isEmpty = true;
    public bool Matches { get; set; }

    //highlitovani policek mrizky
    private Color basicColor = Color.white;
    private Color highlightColor = Color.red;
    private Renderer myRndr;

    private void Start()
    {
        cp = (CubePuzzle)NewManager.instance.cubePuzzle;
        grid = cp.containersHolder.transform;
        if((myRndr=GetComponent<Renderer>())==null)
        {
            myRndr=gameObject.AddComponent<Renderer>();
        }
        Matches = false;
    }

    public void Initialize(int index)//tahle funkce se vola pri vytvareni kontajneru (aby se nepsalo primo containerIndex = k;)
    {
        ContainerIndex = index;
    }

    public void Highlight()
    {
        myRndr.material.color = highlightColor;
    }
    public void CancelHighlight()
    {
        myRndr.material.color = basicColor;
    }

    public void SetMatches(Transform tile, int tileIndex)//container zjisti, jestli na nej pasuje takto natoceni dilek (info o dilku predavano v parametrech)
    {
        int n = cp.ModelPictureNumber;//zjisti od Managera co za obrazek se ma skladat (v tomto pripade je na kotce jen jeden obrazek, takze tohle je vzdy nula)
        Matches = (CheckIndex(tileIndex) && CheckFace(tile, n) && CheckFaceRotation(tile, n));//zjisti jestli tedy dilek pasuje
    }




    private bool CheckIndex(int tileIndex)//spravna pozice?
    {
        return tileIndex == ContainerIndex;
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
            //udaje jsou vypozorovany z toho, jak se mapuje textura na moji krychli...
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
