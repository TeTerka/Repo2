using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A type of puzzle
/// </summary>
/// <remarks>
/// Puzzle description: The goal of the puzzle is to build the model picture by placing cubes with parts of the picture on the grid
/// </remarks>
public class CubePuzzle : AbstractPuzzle
{
    //textures
    private List<Texture2D> tutTexturesForCubes = new List<Texture2D>();
    private List<List<Texture2D>> texturesForCubes = new List<List<Texture2D>>();
    private List<Texture2D> modelPictures = new List<Texture2D>();

    [Header("containers")]
    [SerializeField] private GameObject containerPrefab;
    /// <summary>location of the grid</summary>
    public GameObject containersHolder;
    [SerializeField] private GameObject center;//center of the table
    /// <summary>list of all containers in the grid</summary>
    public List<TileContainer> ContainerList { get; private set; }

    [Header("tiles")]
    [SerializeField] private GameObject tilePrafab;
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();//list of manually created spawn points
    [SerializeField] private GameObject tileHolder;
    /// <summary>size of one tile (one cube)</summary>
    public float TileSize { get; private set; }
    /// <summary>list of all tiles (cubes)</summary>
    public List<GameObject> TileList { get; private set; }
    /// <summary>says on which side of the cube is the part of the image</summary>
    /// <remarks>based on the texture layout (usually 0 has image, 1-5 are blank):
    /// <table>
    /// <tr><td>x<td>x<td>x
    /// <tr><td>3<td>4<td>5<
    /// <tr><td>0<td>1<td>2<
    /// </table>
    /// </remarks>
    public int ModelPictureNumber { get; private set; }

    [Header("start&tut phase stuff")]
    [SerializeField] private Texture2D startCubeTexture;
    [SerializeField] private GameObject startPanelPrefab;
    [SerializeField] private GameObject tutPanelPrefab;
    [SerializeField] private Texture2D tutInputPicture;
    [SerializeField] private Transform startContainerSpot;

    [Header("for generating spawnpoints")]
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;
    private List<Vector3> spawnPositions = new List<Vector3>();//list of all spawnpoints (created in GenerateSpawnPoints) 

    [Header("FileBrowser")]
    public GUISkin customSkin;
    protected FileBrowser m_fileBrowser;
    [SerializeField]
    protected Texture2D m_directoryImage,
                        m_fileImage;

    [Header("stuff needed for menu")]
    private GameObject activePanel;
    private int activePanelNumber;
    [SerializeField] private GameObject configMenuBlockingPanel;
    private List<string> texturePaths = new List<string>();
    [SerializeField] private GameObject table;
    private List<string> widths = new List<string>();
    private List<string> heigths = new List<string>();

    [SerializeField] private Sprite missingImage;

    //for cube puzzle log
    private PuzzleTile leftHeld = null;
    private PuzzleTile rightHeld = null;
    [SerializeField] private Transform replayCubePoint;

    //containers numbered from down left corner, see example (for size 3x5):
    // ---------------------
    // |10 |11 |12 |13 |14 |
    // ---------------------
    // | 5 | 6 | 7 | 8 | 9 |
    // ---------------------
    // | 0 | 1 | 2 | 3 | 4 |
    // ---------------------
    //texture for one cube (0=selected image, 1-5 = "grey parts")
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


    private void Awake()
    {
        ModelPictureNumber = 0;
        ContainerList = new List<TileContainer>();
        TileList = new List<GameObject>();
        TileSize = 0.15f;
        TypeName = "CubePuzzle";

        //create folder for images
        try
        {
            if (!Directory.Exists(Application.dataPath + "\\ImageCopies"))
            {
                Directory.CreateDirectory(Application.dataPath + "\\ImageCopies");
            }
        }
        catch
        { ErrorCatcher.instance.Show("Problem with "+ Application.dataPath + "\\ImageCopies"); }
    }

    private void Start()
    {
        //find out how many pipes can fit on the table
        float tableWidth = table.transform.lossyScale.x;
        float tableHeigth = table.transform.lossyScale.z;
        int maxWidth = (int)((tableWidth / 3) / TileSize);
        int maxHeigth = (int)(tableHeigth / TileSize);

        //set the content of dropdowns in interactibleInfoPanelPrefab accordingly 
        SetNumberOfPuzzlesDropdownContent(maxWidth, maxHeigth);
    }
    

    //------------------------------------------------------------------------------------------
    //for preparation of the puzzle in menu
    //------------------------------------------------------------------------------------------

    public override PuzzleData CreatePuzzle(GameObject panel, int i)
    {
        PuzzleData p = new PuzzleData();
        List<Dropdown> droplist = new List<Dropdown>();
        panel.GetComponentsInChildren<Dropdown>(droplist);
        p.heigthpx = droplist[1].value + 1;
        p.widthpx = droplist[0].value + 1;
        //assign path to image
        if (texturePaths[i] != null)
        {
            p.pathToImage = texturePaths[i];
        }
        //check cubes under table
        p.allowCubesUnderTable = panel.GetComponentInChildren<Toggle>().isOn;
        //generate fixed random spawn position order
        p.spawnPointMix = new List<int>();
        int nnnn = 0;
        if(p.allowCubesUnderTable)
            nnnn = spawnPoints.Count + (p.heigthpx * p.widthpx);//use spawnpoints under table plus generated spawnpoints
        else
            nnnn = (p.heigthpx * p.widthpx);//use only generated spawnpoints
        for (int e = 0; e < nnnn; e++)
        {
            p.spawnPointMix.Add(e);
        }
        ShuffleList(p.spawnPointMix);
        return p;
    }
    private void ShuffleList<T>(List<T> list)
    {
        int listCount = list.Count;
        for (int i = 0; i < listCount; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, listCount);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    public override bool CheckFillingCorrect(GameObject panel, int i)
    {
        Button selectImageButton = panel.GetComponentInChildren<Button>();
        if (texturePaths[i] == null)//not valid cube puzzle image
        {
            selectImageButton.image.color = Color.red;
            return false;
        }
        else
        {
            selectImageButton.image.color = Color.white;
            return true;
        }
    }
    public override string FillTheInfoPanel(GameObject panel, PuzzleData puzzle)
    {
        panel.GetComponentInChildren<Text>().text = puzzle.widthpx + " x " + puzzle.heigthpx;
        List<Image> images = new List<Image>();
        panel.GetComponentsInChildren<Image>(images);
        images[1].sprite = MenuLogic.instance.LoadNewSprite(Application.dataPath + "\\ImageCopies" + puzzle.pathToImage);
        if (images[1].sprite == null)//aka if picture loadig failed
        {
            images[1].sprite = missingImage;
            return "missing file "+ Application.dataPath + "\\ImageCopies" + puzzle.pathToImage;
        }
        return null;
    }

    public override string CheckForMissingThings(Configuration c)
    {
        string missingStuff = "";
        foreach (PuzzleData item in c.puzzles)
        {
            if (!File.Exists(Application.dataPath + "\\ImageCopies" + item.pathToImage))
            {
                missingStuff += "missing file " + Application.dataPath + "\\ImageCopies" + item.pathToImage +"\n";
            }
        }
        if (missingStuff == "")
            return null;
        return missingStuff;
    }

    public override void PrepareInteractibleInfoPanel(GameObject panel, int i)
    {
        List<Dropdown> droplist = new List<Dropdown>();
        panel.GetComponentsInChildren<Dropdown>(droplist);
        droplist[0].ClearOptions();
        droplist[0].AddOptions(widths);
        droplist[1].ClearOptions();
        droplist[1].AddOptions(heigths);
        panel.GetComponentInChildren<Button>().onClick.AddListener(delegate { OnSelectPuzzleImageClick(panel, i); });
        panel.GetComponentInChildren<InputField>().onValueChanged.AddListener(delegate { panel.GetComponentInChildren<InputField>().image.color = Color.white; });
        texturePaths.Add(null);
    }
    public override InputField GetPuzzleName(GameObject panel)
    {
        InputField puzzleNameField = panel.GetComponentInChildren<InputField>();
        return puzzleNameField;
    }

    /// <summary>
    /// draws file brouser
    /// </summary>
    protected void OnGUI()
    {
        GUI.skin = customSkin;
        if (m_fileBrowser != null)
        {
            m_fileBrowser.OnGUI();
        }
    }
    /// <summary>
    /// brings up the file browser to choose a .jpg or .png picture for the puzzle
    /// </summary>
    /// <param name="panel">interactible panel representing the puzzle</param>
    /// <param name="i">the order of the panel in configuration menu list of puzzles</param>
    public void OnSelectPuzzleImageClick(GameObject panel, int i)
    {
        configMenuBlockingPanel.SetActive(true);
        activePanel = panel;
        activePanelNumber = i;
        m_fileBrowser = new FileBrowser(
            new Rect(100, 100, Screen.width - 200, Screen.height - 200),
            "Choose PNG or JPG File",
            FileSelectedCallback
        );

        m_fileBrowser.SelectionPattern = "*.png|*.jpg";//various patterns separated by | (see my changes in file browser)
        m_fileBrowser.DirectoryImage = m_directoryImage;
        m_fileBrowser.FileImage = m_fileImage;
    }
    /// <summary>
    /// copies the selected image to ImageCopies folder, shows the selected image in puzzle panel and stores the path to the image
    /// </summary>
    /// <param name="path">path to the selected image chosen in file browser</param>
    protected void FileSelectedCallback(string path)
    {
        m_fileBrowser = null;

        if (path != null)
        {
            //copy selected image to ImageCopies folder
            string fileName = path.Substring(path.LastIndexOf('\\'), path.LastIndexOf('.') - path.LastIndexOf('\\'));
            string fileType = path.Substring(path.LastIndexOf('.'));

            string newPath = "";
            try
            {
                if (File.Exists(Application.dataPath.ToString() + "\\ImageCopies" + fileName + fileType))//file already exists in ImageCopies...
                {
                    if (FileCompare(path, Application.dataPath + "\\ImageCopies" + fileName + fileType))//...and is the same
                    {
                        newPath = fileName + fileType;
                        //and do not copy anything
                    }
                    else//...but is different
                    {
                        int i = 0;
                        //create unique file name
                        while (File.Exists(Application.dataPath + "\\ImageCopies" + fileName + "(" + i + ")" + fileType) && !FileCompare(path, Application.dataPath + "\\ImageCopies" + fileName + "(" + i + ")" + fileType))
                        {
                            i++;
                        }
                        newPath = fileName + "(" + i + ")" + fileType;
                        //actually copy the file to ImageCopies folder
                        File.Copy(path, Application.dataPath + "\\ImageCopies" + newPath, true);
                    }
                }
                else//file does not exist in ImageCopies
                {
                    newPath = fileName + fileType;
                    //actually copy the file to ImageCopies folder
                    File.Copy(path, Application.dataPath + "\\ImageCopies" + newPath, true);
                }

            }
            catch
            {
                ErrorCatcher.instance.Show("Problem with file " + path + " or folder " + Application.dataPath + "\\ImageCopies");
            }
            //save path to the image
            texturePaths[activePanelNumber] = newPath;

            //show image in the panel
            activePanel.GetComponentInChildren<Button>().image.sprite = MenuLogic.instance.LoadNewSprite(Application.dataPath + "\\ImageCopies" + newPath);
            if (activePanel.GetComponentInChildren<Button>().image.sprite == null)//if picture loadig failed
            {   
                activePanel.GetComponentInChildren<Button>().image.sprite = missingImage;
            }
        }
        else
        {
            activePanel.GetComponentInChildren<Button>().image.sprite = null;
        }
        configMenuBlockingPanel.SetActive(false);
        activePanel.GetComponentInChildren<Button>().image.color = Color.white;//cancel possible error highlight
    }

    /// <summary>
    /// helper function used to compare two files
    /// </summary>
    /// <param name="file1">path to file1</param>
    /// <param name="file2">path ti file2</param>
    /// <returns>true = the files are the same</returns>
    private bool FileCompare(string file1, string file2)
    {
        int file1byte;
        int file2byte;
        FileStream fs1;
        FileStream fs2;
        string path1 = Path.GetFullPath(file1);
        string path2 = Path.GetFullPath(file2);

        //if the same file was referenced two times
        if (path1 == path2)
        {
            return true;
        }

        fs1 = new FileStream(file1, FileMode.Open);
        fs2 = new FileStream(file2, FileMode.Open);

        // Check the file sizes. If they are not the same, the files 
        // are not the same.
        if (fs1.Length != fs2.Length)
        {
            fs1.Close();
            fs2.Close();
            return false;
        }

        // Read and compare a byte from each file until either a
        // non-matching set of bytes is found or until the end of
        // file1 is reached.
        do
        {
            file1byte = fs1.ReadByte();
            file2byte = fs2.ReadByte();
        }
        while ((file1byte == file2byte) && (file1byte != -1));

        fs1.Close();
        fs2.Close();

        return ((file1byte - file2byte) == 0);
    }

    /// <summary>
    /// sets the content od dropdowns in interactibleInfoPanelPrefab which contain options for the width and higth of the puzzle
    /// </summary>
    /// <param name="maxWidth"></param>
    /// <param name="maxHeigth"></param>
    private void SetNumberOfPuzzlesDropdownContent(int maxWidth, int maxHeigth)
    {
        for (int i = 1; i <= maxWidth; i++)
        {
            widths.Add(i.ToString());
        }
        for (int i = 1; i <= maxHeigth; i++)
        {
            heigths.Add(i.ToString());
        }
    }


    
    //------------------------------------------------------------------------------------------------
    // implementation of AbstractPuzzle abstract methods - things needed for running the puzzle
    //------------------------------------------------------------------------------------------------

    public override void OnTimerStop()
    {
        Animator agent = NewManager.instance.TheNpc.GetComponent<Animator>();
        if (NewManager.instance.HasParameter("CubeTimeRanOut",agent))
        {
            agent.SetTrigger("CubeTimeRanOut");
        }
    }

    public override void EndPhase()
    {
        BasicFinish();
    }

    public override void EndStart()
    {
        BasicFinish();
    }

    public override void EndTut()
    {
        BasicFinish();
    }

    /// <summary>
    /// deletes all cubes (tiles) and all containers
    /// </summary>
    private void BasicFinish()
    {
        //destroy tiles
        foreach (GameObject tile in TileList)
        {
            tile.GetComponent<PuzzleTile>().DestroyYourself();
        }
        TileList.Clear();
        //destroy containers
        foreach (var container in ContainerList)
        {
            Destroy(container.gameObject);
        }
        ContainerList.Clear();
    }

    public override void FinishConfig()
    {
        texturesForCubes.Clear();
        modelPictures.Clear();
        tutTexturesForCubes.Clear();
    }

    public override void StartConfig(Configuration c)
    {
        //create textures for cubes in tutorial
        if (c.withTutorial)
        {
            tutTexturesForCubes = CreateTexturesForCubes(2, 2, tutInputPicture);
        }
        //create textures for puzzles
        for (int i = 0; i < c.puzzles.Count; i++)
        {
            PuzzleData puzzle = c.puzzles[i];
            Texture2D tt;//texture of the whole model picture
            if ((tt = MenuLogic.instance.LoadTexture(Application.dataPath + "\\ImageCopies" + puzzle.pathToImage)) == null)//if loading failed load "missing file" picture
            {
                texturesForCubes.Add(CreateTexturesForCubes(puzzle.heigthpx, puzzle.widthpx, missingImage.texture));
                tt = missingImage.texture;
            }
            else//load correct picture
            {
                texturesForCubes.Add(CreateTexturesForCubes(puzzle.heigthpx, puzzle.widthpx, tt));
            }
            //and create textures for model picture(s)
            int sideWidth = Mathf.Min(tt.height / puzzle.heigthpx, tt.width / puzzle.widthpx);
            Texture2D t = new Texture2D(sideWidth * puzzle.widthpx, sideWidth * puzzle.heigthpx);//texture of cropped model picture
            Color[] block = tt.GetPixels(0, 0, sideWidth * puzzle.widthpx, sideWidth * puzzle.heigthpx);
            t.SetPixels(0, 0, sideWidth * puzzle.widthpx, sideWidth * puzzle.heigthpx, block);
            t.Apply();
            modelPictures.Add(t);
        }
    }

    public override void StartPhase()
    {
        int puzzleIndex = NewManager.instance.ActivePuzzleIndex;
        PuzzleData puzzle = NewManager.instance.ActiveConfig.puzzles[puzzleIndex];

        //scaling of the model picture
        float x = puzzle.widthpx * TileSize * 2.5f;//=>model picture 2.5x bigger than the grid
        float y = puzzle.heigthpx * TileSize * 2.5f;
        NewManager.instance.MultiplyWallpictureScale(x, y);

        //set model picture
        NewManager.instance.SetWallPicturePanelBasic(modelPictures[puzzleIndex]);

        //create cubes and containers
        GeneratePuzzleTiles(puzzle.heigthpx, puzzle.widthpx, texturesForCubes[puzzleIndex]);
    }

    public override void StartStart()
    {
        //set model picture
        float x = 5 * TileSize * 2.5f;
        float y = 3 * TileSize * 2.5f;
        NewManager.instance.MultiplyWallpictureScale(x, y);

        NewManager.instance.SetWallPicturePanel(startPanelPrefab);

        //create the start cube
        List<Texture2D> list = new List<Texture2D>();
        for (int i = 0; i < 6; i++)
        {
            list.Add(startCubeTexture);
        }
        GeneratePuzzleTiles(1, 1, list);
    }

    public override void OnTableHeigthChange()
    {
        //this will be called only during start phase, so it adjusts only the position of the start cube and the one container
        if (TileList.Count > 0)
        {
            TileList[0].transform.position = spawnPoints[0].position;
        }
        if (ContainerList.Count > 0)
            ContainerList[0].transform.position = startContainerSpot.position;
    }

    public override void StartTut()
    {
        //set model picture
        float x = 5 * TileSize * 2.5f;
        float y = 3 * TileSize * 2.5f;
        NewManager.instance.MultiplyWallpictureScale(x, y);

        NewManager.instance.SetWallPicturePanel(tutPanelPrefab);

        //create cubes and containers
        GeneratePuzzleTiles(2, 2, tutTexturesForCubes);
    }

    /// <summary>
    /// checks if puzzle is solved
    /// </summary>
    /// <returns>true = puzzle is solved</returns>
    private bool CheckIfComplete()
    {
        bool finished = true;
        foreach (TileContainer item in ContainerList)
        {
            if (item.Matches == false)
            {
                finished = false;
                break;
            }
        }
        return finished;
    }


    

    //------------------------------------------------------------------------------------------------
    // events called by PuzzleTile
    //------------------------------------------------------------------------------------------------

    /// <summary>
    /// PuzzleTile (a script on the cube) calls this when the cube is picked up from the grid, can cause decrease of score
    /// </summary>
    /// <param name="correctlyPlaced">true = the picked up cube was picked up from correct position in grid</param>
    public void OnCubeRemoved(bool correctlyPlaced)
    {
        if (correctlyPlaced)
        {
            NewManager.instance.DecreaseScore();
        }
    }
    /// <summary>
    /// PuzzleTile (a script on the cube) calls this when the cube is placed to the grid, can cause increase of score or can finish the puzzle
    /// </summary>
    /// <param name="correctly">true = the cube is placed to the correct position in grid</param>
    public void OnCubePlaced(bool correctly)
    {
        if (NewManager.instance.InStart)
        {
            NewManager.instance.SetPhaseComplete();
        }
        else
        {
            //update score and check if paseFinished
            if (correctly)
            {
                NewManager.instance.IncreaseScore();
            }
            if (CheckIfComplete())
            {
                NewManager.instance.SetPhaseComplete();
            }
        }
    }



    //------------------------------------------------------------------------------------------------
    // creating textures, container, tiles and spawnpoints
    //------------------------------------------------------------------------------------------------

    /// <summary>
    /// cuts the input texture to create h*w textures for the tiles (the cubes)
    /// </summary>
    /// <param name="h">heigth</param>
    /// <param name="w">width</param>
    /// <param name="input">model picture texture</param>
    /// <returns>set of textures for cubes</returns>
    private List<Texture2D> CreateTexturesForCubes(int h, int w, Texture2D input)
    {
        int sideWidth = Mathf.Min(input.height / h, input.width / w);//if the ratio of input picture is not h:w, it will be cropped
        List<Texture2D> result = new List<Texture2D>();

        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                Texture2D newTexture = new Texture2D(3 * sideWidth, 3 * sideWidth, TextureFormat.RGB565,false);  
                //read block of data (one square of puzzle) from image
                Color[] block = input.GetPixels(sideWidth * j, sideWidth * i, sideWidth, sideWidth);
                //write it to new texture to the right position        
                newTexture.SetPixels(0, 0, sideWidth, sideWidth, block);
                //apply changes to texture
                newTexture.Apply();
                //texturesForCubes.Add(newTexture);
                result.Add(newTexture);
            }
        }
        return result;
    }

    /// <summary>
    /// cretes the cubes, adds texture to them and places them on the table,
    /// also creates the containers and spawnpoints
    /// </summary>
    /// <param name="h">heigth</param>
    /// <param name="w">width</param>
    /// <param name="textureSource">set of textures for the cubes</param>
    private void GeneratePuzzleTiles(int h, int w, List<Texture2D> textureSource)
    {
        //place containersHolder on the table
        Vector3 ctp = center.transform.position;
        containersHolder.transform.position = new Vector3(ctp.x - ((w * TileSize) / 2) + (TileSize / 2), ctp.y, ctp.z - ((h * TileSize) / 2) + (TileSize / 2));
        //generate containers
        int k = 0;
        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                GameObject c = Instantiate(containerPrefab);
                c.transform.SetParent(containersHolder.transform);
                c.transform.localScale = new Vector3(TileSize, 0.02f, TileSize);
                c.transform.localPosition = new Vector3(-j * TileSize, 0, -i * TileSize);

                TileContainer tc;
                if ((tc = c.GetComponent<TileContainer>()) == null)
                    c.gameObject.AddComponent<TileContainer>();
                tc.Initialize(k);
                ContainerList.Add(tc);
                k++;
            }
        }
        
        //generate spawnpoints and shuffle them according to Puzzle.spawnPointMix list
        if (NewManager.instance.InStart)
        {
            //generate spawnpoints
            GenerateSpawnPositions(h, w,true);
            //start needs only one spawnpoint
            spawnPositions = new List<Vector3> { spawnPositions[0] };
        }
        else if (NewManager.instance.InTut)
        {
            //generate spawnpoints
            GenerateSpawnPositions(h, w, false);
            //tutorial needs only 4 spawnpoints
            spawnPositions = new List<Vector3> { spawnPositions[0], spawnPositions[1], spawnPositions[2], spawnPositions[3] };
        }
        else
        {
            //generate spawnpoints
            GenerateSpawnPositions(h, w, NewManager.instance.ActiveConfig.puzzles[NewManager.instance.ActivePuzzleIndex].allowCubesUnderTable);
            spawnPositions = SpecialShuffle(NewManager.instance.ActiveConfig.puzzles[NewManager.instance.ActivePuzzleIndex].spawnPointMix, spawnPositions);
        }

        //generate tiles (cubes)
        k = 0;
        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                //get random rotation (but keep all cubes face up)
                Vector3[] axies = new Vector3[] { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };
                int randomIndex = Random.Range(0, 4);

                //create the cube
                GameObject t = Instantiate(tilePrafab, spawnPositions[k], Quaternion.LookRotation(axies[randomIndex], Vector3.down));
                t.transform.SetParent(tileHolder.transform);
                t.transform.localScale = new Vector3(TileSize * 100, TileSize * 100, TileSize * 100);// *100 beacase my model of cube is 100x smaller than unity cube

                //add the texture
                MeshRenderer mr;
                if ((mr = t.GetComponent<MeshRenderer>()) == null)
                {
                    mr = t.gameObject.AddComponent<MeshRenderer>();
                }
                mr.material = new Material(Shader.Find("Standard"));
                mr.material.mainTexture = textureSource[k];

                //initialize the tile
                PuzzleTile pt;
                if ((pt = t.GetComponent<PuzzleTile>()) == null)
                {
                    pt = t.gameObject.AddComponent<PuzzleTile>();
                }
                pt.Initialize(k, spawnPositions[k]);

                TileList.Add(t);
                k++;
            }
        }

    }

    /// <summary>
    /// generates spawnpoints
    /// </summary>
    /// <param name="h">heigth</param>
    /// <param name="w">width</param>
    private void GenerateSpawnPositions(int h, int w, bool underTable)
    {
        spawnPositions.Clear();

        if (underTable)
        {
            //first add fixed spawnpoints created by hand in editor (the ones under the table)
            foreach (Transform point in spawnPoints)
            {
                spawnPositions.Add(point.position);
            }
        }

        //create more spawnpoint so that there is at least h*w of them
        float offset = TileSize / 2;
        float jOffset = TileSize;

        Vector3 lp = leftPoint.position;
        for (int i = 0; i < Mathf.Ceil((float)w / 2); i++)//half of then on the left wing of the table
        {
            for (int j = 0; j < h; j++)
            {
                float rand = Random.Range(0, offset - 0.05f);
                float jRand = Random.Range(0, jOffset - 0.05f);
                spawnPositions.Add(new Vector3(lp.x - (TileSize + offset) * i - (offset + (TileSize / 2) + rand), lp.y, lp.z - (TileSize + jOffset) * j + jRand)); 
            }
        }

        Vector3 rp = rightPoint.position;
        for (int i = 0; i < w / 2; i++)//other half to the right wing of the table
        {
            for (int j = 0; j < h; j++)
            {
                float rand = Random.Range(0, offset - 0.05f);
                float jRand = Random.Range(0, jOffset - 0.05f);
                spawnPositions.Add(new Vector3(offset + (TileSize / 2) + rp.x + (TileSize + offset) * i - rand, rp.y, rp.z - (TileSize + jOffset) * j + jRand));
            }
        }

    }

    /// <summary>
    /// <para>shuffles list accordingly to nums</para>
    /// <para>for example for {2,0,1,3} and {a,b,c,d} returns {c,a,b,d}</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="nums">specifies the shuffle style, must contain all numbers from 0 to nums.Count-1</param>
    /// <param name="list">the list to be shuffled, must be as long as nums</param>
    /// <returns>shuffled list (see example)</returns>
    private List<T> SpecialShuffle<T>(List<int> nums,List<T> list)
    {
        List<T> newList = new List<T>();
        for (int i = 0; i < nums.Count; i++)
        {
            newList.Add(list[nums[i]]);
        }
        return newList;
    }



    

    //------------------------------------------------------------------------------------------------
    // methods for logging and replaying the logfile
    //------------------------------------------------------------------------------------------------

    public override void Simulate(string[] atoms)
    {
        int i = 0;
        float x = 0; float y = 0; float z = 0; float w = 0;
        switch (atoms[1])//there should be the name of the action (it is already checked in Logger that atoms.Count>1)
        {
            case "Grab":
                if (atoms.Length == 4 && int.TryParse(atoms[3], out i) && (atoms[2] == "left" || atoms[2] == "right"))
                {
                    Grab(i, atoms[2]);
                    break;
                }
                else
                {
                    ErrorCatcher.instance.Show("Wrong format of this logfile: " + Logger.instance.PathToLogFile + " (line " + atoms[0] + " has wrong Grab arguments)");
                    return;
                };
            case "Drop":
                if (atoms.Length == 3 && (atoms[2] == "left" || atoms[2] == "right"))
                {
                    Drop(i, atoms[2]);
                    break;
                }
                else
                {
                    ErrorCatcher.instance.Show("Wrong format of this logfile: " + Logger.instance.PathToLogFile + " (line " + atoms[0] + " has wrong Drop arguments)");
                    return;
                };
            case "Place":
                if (atoms.Length == 8 && int.TryParse(atoms[3], out i) && (atoms[2] == "left" || atoms[2] == "right") && float.TryParse(atoms[4], out x) && float.TryParse(atoms[5], out y) && float.TryParse(atoms[6], out z) && float.TryParse(atoms[7], out w))
                {
                    Place(i, atoms[2], new Quaternion(x, y, z, w));
                    break;
                }
                else
                {
                    ErrorCatcher.instance.Show("Wrong format of this logfile: " + Logger.instance.PathToLogFile + " (line " + atoms[0] + " has wrong Place arguments)");
                    return;
                }
            case "Respawn":
                if (atoms.Length == 4 && int.TryParse(atoms[3], out i) && (atoms[2] == "left" || atoms[2] == "right"))
                {
                    Respawn(i, atoms[2]);
                    break;
                }
                else
                {
                    ErrorCatcher.instance.Show("Wrong format of this logfile: " + Logger.instance.PathToLogFile + " (line " + atoms[0] + " has wrong Respawn arguments)");
                    return;
                }
            default:
                {
                    ErrorCatcher.instance.Show("Wrong format of this logfile: " + Logger.instance.PathToLogFile + " (line " + atoms[0] + " is not a valid action)");
                    break;
                }
        }
    }

    /// <summary>
    /// simulates grabbing tile <paramref name="n"/> by hand <paramref name="hand"/>
    /// </summary>
    /// <param name="n">tile index</param>
    /// <param name="hand">"left" or "right" hand</param>
    private void Grab(int n, string hand)
    {
        foreach (GameObject cube in TileList)
        {
            //find the cube with index n and simulate grab on it
            if (cube.GetComponent<PuzzleTile>().IfItIsYouSimulateGrab(n))
            {
                //mark that the cube is now in the left or right hand, and visualize it by moving it up to the replayCubePoint
                if (hand == "left")
                {
                    if (rightHeld == cube.GetComponent<PuzzleTile>()) { rightHeld = null; }//for the case of grabing a cube from one hand to the other
                    if (leftHeld != null) { ErrorCatcher.instance.Show("Wrong format of this logfile: " + Logger.instance.PathToLogFile + " (illegal Grab action)"); return; }
                    leftHeld = cube.GetComponent<PuzzleTile>();
                    cube.transform.position = replayCubePoint.position + new Vector3(-0.2f, 0, 0);
                }
                else
                {
                    if (leftHeld == cube.GetComponent<PuzzleTile>()) { leftHeld = null; }//for the case of grabing a cube from one hand to the other
                    if (rightHeld != null) { ErrorCatcher.instance.Show("Wrong format of this logfile: " + Logger.instance.PathToLogFile + " (illegal Grab action)"); return; }
                    rightHeld = cube.GetComponent<PuzzleTile>();
                    cube.transform.position = replayCubePoint.position + new Vector3(0.2f, 0, 0);
                }
                break;
            }
        }
    }

    /// <summary>
    /// simulates dropping tile <paramref name="n"/> from hand <paramref name="hand"/>
    /// </summary>
    /// <param name="n">tile index</param>
    /// <param name="hand">"left" or "right" hand</param>
    private void Drop(int n, string hand)
    {
        if (hand == "left")
        {
            if (leftHeld == null) { ErrorCatcher.instance.Show("Wrong format of this logfile: " + Logger.instance.PathToLogFile + " (illegal Drop action)"); return; }
            leftHeld.SimulateDrop();
            leftHeld = null;
        }
        else
        {
            if (rightHeld == null) { ErrorCatcher.instance.Show("Wrong format of this logfile: " + Logger.instance.PathToLogFile + " (illegal Drop action)"); return; }
            rightHeld.SimulateDrop();
            rightHeld = null;
        }
    }

    /// <summary>
    /// simulate placing cube from hand <paramref name="hand"/> to container <paramref name="m"/>
    /// </summary>
    /// <param name="m">container index</param>
    /// <param name="hand">"left" or "right" hand</param>
    /// <param name="rot">cube rotation</param>
    private void Place(int m, string hand, Quaternion rot)
    {
        if (hand == "left")
        {
            if (leftHeld == null) { ErrorCatcher.instance.Show("Wrong format of this logfile: " + Logger.instance.PathToLogFile + " (illegal Place action)"); return; }
            leftHeld.SimulatePlaceTo(m, rot);
            leftHeld = null;
        }
        else
        {
            if (rightHeld == null) { ErrorCatcher.instance.Show("Wrong format of this logfile: " + Logger.instance.PathToLogFile + " (illegal Place action)"); return; }
            rightHeld.SimulatePlaceTo(m, rot);
            rightHeld = null;
        }
    }
    /// <summary>
    /// simulate respawning cube <paramref name="n"/> from hand <paramref name="hand"/>
    /// </summary>
    /// <param name="n">cube index</param>
    /// <param name="hand">"left" or "right" hand</param>
    private void Respawn(int n, string hand)
    {
        Drop(n, hand);
    }

}

