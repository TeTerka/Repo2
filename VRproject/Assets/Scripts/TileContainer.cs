using UnityEngine;


/// <summary>
/// represents one field of the grid
/// </summary>
public class TileContainer : MonoBehaviour {

    //charakteristics
    public int ContainerIndex { get; private set; }//containers numbered from down left corner, see example (for size 3x5):
    // ---------------------
    // |10 |11 |12 |13 |14 |
    // ---------------------
    // | 5 | 6 | 7 | 8 | 9 |
    // ---------------------
    // | 0 | 1 | 2 | 3 | 4 |
    // ---------------------

    //references
    private Transform grid;//position of the grid
    private CubePuzzle cp;
    //state
    public bool isEmpty = true;
    public bool Matches { get; set; }

    //highliting
    private Color basicColor = Color.white;
    private Color highlightColor = Color.red;
    private Renderer myRndr;

    private void Start()
    {
        foreach (AbstractPuzzle puzzleType in NewManager.instance.puzzleTypes)
        {
            if (puzzleType.typeName == "CubePuzzle")
                cp = (CubePuzzle)puzzleType;
        }
        if (cp==null)
        {
            ErrorCatcher.instance.Show("Error, puzzle type CubePuzzle is not available");
        }

        grid = cp.containersHolder.transform;
        if((myRndr=GetComponent<Renderer>())==null)
        {
            myRndr=gameObject.AddComponent<Renderer>();
        }
        Matches = false;
    }

    /// <summary>
    /// called after creating a container, sets up its index
    /// </summary>
    /// <param name="index"></param>
    public void Initialize(int index)
    {
        ContainerIndex = index;
    }

    /// <summary>
    /// highlights this container
    /// </summary>
    public void Highlight()
    {
        myRndr.material.color = highlightColor;
    }
    /// <summary>
    /// cancels highlight on this container
    /// </summary>
    public void CancelHighlight()
    {
        myRndr.material.color = basicColor;
    }

    /// <summary>
    /// checks if <paramref name="tile"/> is correctly placed
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="tileIndex"></param>
    public void SetMatches(Transform tile, int tileIndex)
    {
        int n = cp.ModelPictureNumber;//always zero (because we use now only one face of the cube)
        Matches = (CheckIndex(tileIndex) && CheckFace(tile, n) && CheckFaceRotation(tile, n));
    }

    /// <summary>
    /// check if placed to correct container
    /// </summary>
    /// <param name="tileIndex"></param>
    /// <returns></returns>
    private bool CheckIndex(int tileIndex)
    {
        return tileIndex == ContainerIndex;
    }
    /// <summary>
    /// check if it is the correct face up
    /// </summary>
    /// <param name="cube"></param>
    /// <param name="n"></param>
    /// <returns></returns>
    private bool CheckFace(Transform cube, int n)
    {
        //texture
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
        switch (n)
        {
            //describes the mapping of texture above on the model of the cube
            case 0: return grid.up == -cube.up;
            case 1: return grid.up == -cube.right;
            case 2: return grid.up == cube.right;
            case 3: return grid.up == cube.forward;
            case 4: return grid.up == cube.up;
            case 5: return grid.up == -cube.forward;
            default: return false;
        }
    }
    /// <summary>
    /// check if the cube is correctly rotated
    /// </summary>
    /// <param name="cube"></param>
    /// <param name="n"></param>
    /// <returns></returns>
    private bool CheckFaceRotation(Transform cube, int n)
    {
        switch (n)
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
