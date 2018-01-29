using System.Collections.Generic;
using UnityEngine;

public class CubePuzzle : AbstractPuzzle
{
    //textures
    private List<Texture2D> tutTexturesForCubes = new List<Texture2D>();
    private List<List<Texture2D>> texturesForCubes = new List<List<Texture2D>>();
    private List<Texture2D> modelPictures = new List<Texture2D>();

    [Header("containers")]
    public GameObject containerPrefab;
    public GameObject containersHolder;//public??????????????????????????????????????????
    public GameObject center;//center of the table
    public List<TileContainer> ContainerList { get; private set; }

    [Header("tiles")]
    public GameObject tilePrafab;
    public List<Transform> spawnPoints = new List<Transform>();//seznam napevno urcenych spawnpointu (ty pod stolem)
    public GameObject tileHolder;
    public float TileSize { get; private set; }
    private List<GameObject> tileList = new List<GameObject>();

    public int ModelPictureNumber { get; private set; }

    [Header("start&tut phase stuff")]
    public Texture welcomePicture;
    public Texture2D startCubeTexture;
    public Texture tutorialPicture;
    public Texture2D tutInputPicture;

    [Header("for generating spawnpoints")]
    public Transform leftPoint;
    public Transform rightPoint;
    private List<Vector3> spawnPositions = new List<Vector3>();//seznam vsech spawnpointu (vznikne v GenerateSpawnPoints) 

    //cislovani krychli/mrizky je od leveho dolniho rohu:
    // ---------------------
    // |10 |11 |12 |13 |14 |
    // ---------------------
    // | 5 | 6 | 7 | 8 | 9 |
    // ---------------------
    // | 0 | 1 | 2 | 3 | 4 |
    // ---------------------
    //textura jedne kostky (0=selected image, 1-5 = "grey parts")
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
        TileSize = 0.15f;
    }

    public override void CustomUpdate()
    {
        //nic
    }

    public override void OnTimerStop()
    {
        //nic
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
        //and create textures for model picture(s)
        for (int i = 0; i < c.puzzles.Count; i++)
        {
            Texture2D tt;//textura celeho vzoroveho obrazku
            if ((tt = MenuLogic.instance.LoadTexture(c.puzzles[i].pathToImage)) == null)//v pripade chyby nahrad obrazek vykricnikem....ale nic neukoncuj, je to jen varovani
            {
                texturesForCubes.Add(CreateTexturesForCubes(c.puzzles[i].heigthpx, c.puzzles[i].widthpx, MenuLogic.instance.missingImage.texture));
                tt = MenuLogic.instance.missingImage.texture;
            }
            else//jinak normalne nahraj obrazky ze souboru
            {
                texturesForCubes.Add(CreateTexturesForCubes(c.puzzles[i].heigthpx, c.puzzles[i].widthpx, tt));
            }
            int sideWidth = Mathf.Min(tt.height / c.puzzles[i].heigthpx, tt.width / c.puzzles[i].widthpx);
            Texture2D t = new Texture2D(sideWidth * c.puzzles[i].widthpx, sideWidth * c.puzzles[i].heigthpx);//textura oriznuteho vzoroveho obrazku
            Color[] block = tt.GetPixels(0, 0, sideWidth * c.puzzles[i].widthpx, sideWidth * c.puzzles[i].heigthpx);
            t.SetPixels(0, 0, sideWidth * c.puzzles[i].widthpx, sideWidth * c.puzzles[i].heigthpx, block);
            t.Apply();
            modelPictures.Add(t);
        }
    }

    public override void StartPhase()
    {
        //scaling of the model picture
        //////imageHolder.localScale = NewManager.instance.originalScale;
        float x = NewManager.instance.ActiveConfig.puzzles[NewManager.instance.ActivePuzzleIndex].widthpx * TileSize * 3f;//=>model picture 3x vestsi nez mrizka na skladani
        float y = NewManager.instance.ActiveConfig.puzzles[NewManager.instance.ActivePuzzleIndex].heigthpx * TileSize * 3f;
        //////imageHolder.localScale = new Vector3(imageHolder.localScale.x * x, imageHolder.localScale.y * y, 0.2f);
        NewManager.instance.MultiplyWallpictureScale(x, y);

        NewManager.instance.SetWallPicture(modelPictures[NewManager.instance.ActivePuzzleIndex]);

        //create cubes and containers
        GeneratePuzzleTiles(NewManager.instance.ActiveConfig.puzzles[NewManager.instance.ActivePuzzleIndex].heigthpx, NewManager.instance.ActiveConfig.puzzles[NewManager.instance.ActivePuzzleIndex].widthpx, texturesForCubes[NewManager.instance.ActivePuzzleIndex]);
    }

    public override void StartStart()
    {
        float x = 5 * TileSize * 3f;
        float y = 3 * TileSize * 3f;
        NewManager.instance.MultiplyWallpictureScale(x, y);

        NewManager.instance.SetWallPicture(welcomePicture);
        /////////////////////////////////////////////modelPictureFrame.material.color = Color.white;

        //create the start cube
        List<Texture2D> list = new List<Texture2D>();
        for (int i = 0; i < 6; i++)
        {
            list.Add(startCubeTexture);
        }
        GeneratePuzzleTiles(1, 1, list);
    }

    public override void StartTut()
    {
        float x = 5 * TileSize * 3f;
        float y = 3 * TileSize * 3f;
        NewManager.instance.MultiplyWallpictureScale(x, y);

        NewManager.instance.SetWallPicture(tutorialPicture);
        ///////////////////////////modelPictureFrame.material.color = Color.white;
        //create cubes and containers
        GeneratePuzzleTiles(2, 2, tutTexturesForCubes);
    }

    public void BasicFinish()
    {
        //destroy tiles
        foreach (GameObject tile in tileList)
        {
            tile.GetComponent<PuzzleTile>().DestroyYourself();
        }
        tileList.Clear();
        //destroy containers
        foreach (var container in ContainerList)
        {
            Destroy(container.gameObject);
        }
        ContainerList.Clear();
    }

    public bool CheckIfComplete()
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

    public void OnCubeRemoved(bool correctlyPlaced)//when player pics cube up from the grid
    {
        if (correctlyPlaced)
        {
            NewManager.instance.DecreaseScore();
        }
    }
    public void OnCubePlaced(bool correctly)//when player places a cube
    {
        if (NewManager.instance.InStart)
        {
            NewManager.instance.SetPhaseComplete();
        }
        else
        {
            //updatovani skore a zjisteni, jestli neni phaseFinished
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


    //////////////////////********************************CREATING TEXTURES, CONTAINERS, TILES...********************************************
    private List<Texture2D> CreateTexturesForCubes(int h, int w, Texture2D input)//z obrazku input vyrobi h*w textur pro jednotlive dilky skladacky
    {
        int sideWidth = Mathf.Min(input.height / h, input.width / w);//=> kdyz to nebude vychazet presne, obrazek bude oriznuty
        List<Texture2D> result = new List<Texture2D>();

        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                Texture2D newTexture = new Texture2D(3 * sideWidth, 3 * sideWidth, TextureFormat.RGB565, false); //je to false potreba?     
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

    private void ShuffleList<T>(List<T> list)//zamicha seznam cehokoliv
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

    //cretes the cubes, adds texture to them and places them on the table
    //also creates the containers
    private void GeneratePuzzleTiles(int h, int w, List<Texture2D> textureSource)
    {
        //umisti sparavne containersHolder
        containersHolder.transform.position = new Vector3(center.transform.position.x - ((w * TileSize) / 2) + (TileSize / 2), center.transform.position.y, center.transform.position.z - ((h * TileSize) / 2) + (TileSize / 2));
        //generuj containers na stul
        int k = 0;
        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                GameObject c = Instantiate(containerPrefab);
                c.transform.SetParent(containersHolder.transform);
                c.transform.localScale = new Vector3(TileSize, 0.02f, TileSize);
                c.transform.localPosition = new Vector3(-j * TileSize, 0, -i * TileSize);//poradi generovani (znamenka) je upraveno aby sedely indexy (viz obr cislovani mrizky)

                TileContainer tc;
                if ((tc = c.GetComponent<TileContainer>()) == null)//kdyby prefab nemel tileContainer script, tak mu ho to doda
                    c.gameObject.AddComponent<TileContainer>();
                tc.Initialize(k);
                ContainerList.Add(tc);
                k++;
            }
        }
        //generuj spawnpoints
        GenerateSpawnPoints(h, w);
        //generuj dilky (krychle)
        ShuffleList(spawnPositions);
        k = 0;
        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                //get random rotation - nakonec ne, protoze je chci pro prehlednost tou plnou stranou nahoru
                Vector3[] axies = new Vector3[] { Vector3.forward, Vector3.down, Vector3.right, Vector3.back, Vector3.up, Vector3.left };
                //int randomIndex1 = Random.Range(0, 6);
                //int randomIndex2 = Random.Range(0, 6);
                //Quaternion randomRotation = Quaternion.LookRotation(axies[randomIndex1], axies[randomIndex2]);

                //create the cube
                //GameObject t = Instantiate(tilePrafab, spawnPoints[k].position,randomRotation);
                GameObject t = Instantiate(tilePrafab, spawnPositions[k], Quaternion.LookRotation(axies[3], axies[1]));
                t.transform.SetParent(tileHolder.transform);
                t.transform.localScale = new Vector3(TileSize * 100, TileSize * 100, TileSize * 100);// *100 protoze moje krychle z blenderu je omylem 100x mensi nez krychle v unity...

                //add the texture
                MeshRenderer mr;
                if ((mr = t.GetComponent<MeshRenderer>()) == null)//kdyby prefab nemel meshRenderer, tak mu ho to doda
                {
                    mr = t.gameObject.AddComponent<MeshRenderer>();
                }
                mr.material = new Material(Shader.Find("Standard"));
                mr.material.mainTexture = textureSource[k];

                //index
                PuzzleTile pt;
                if ((pt = t.GetComponent<PuzzleTile>()) == null)//kdyby prefab nemel PuzzleTile script, tak mu ho to doda
                {
                    pt = t.gameObject.AddComponent<PuzzleTile>();
                }
                pt.Initialize(k, spawnPositions[k]);

                tileList.Add(t);
                k++;
            }
        }

    }

    private void GenerateSpawnPoints(int h, int w)//vyrobi spawnpointy na stole tak, aby se tam vsechno veslo a nebylo to uplne v pravidelne mrizce ani uplne nahodne na hromade
    {
        spawnPositions.Clear();
        //nejprve prida do seznamu rucne naklikane spawnpointy (napr. ty pod stolem)
        foreach (Transform point in spawnPoints)
        {
            spawnPositions.Add(point.position);
        }

        //pak vyrobi dalsi, aby se tam vsechny dilky vesly
        float offset = TileSize / 2;
        float jOffset = TileSize;

        for (int i = 0; i < Mathf.Ceil((float)w / 2); i++)//polovina kostek na leve kridlo stolu
        {
            for (int j = 0; j < h; j++)
            {
                float rand = Random.Range(0, offset - 0.05f);
                float jRand = Random.Range(0, jOffset - 0.05f);
                spawnPositions.Add(new Vector3(leftPoint.position.x - (TileSize + offset) * i - (offset + (TileSize / 2) + rand), leftPoint.position.y, leftPoint.position.z - (TileSize + jOffset) * j + jRand));
            }
        }

        for (int i = 0; i < w / 2; i++)//druha polovina kostek na prave kridlo stolu
        {
            for (int j = 0; j < h; j++)
            {
                float rand = Random.Range(0, offset - 0.05f);
                float jRand = Random.Range(0, jOffset - 0.05f);
                spawnPositions.Add(new Vector3(offset + (TileSize / 2) + rightPoint.position.x + (TileSize + offset) * i - rand, rightPoint.position.y, rightPoint.position.z - (TileSize + jOffset) * j + jRand));
            }
        }

    }
}

