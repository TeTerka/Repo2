using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A type of puzzle
/// </summary>
/// <remarks>
/// Puzzle description: The goal of the puzzle is to connect the start and the end pipe by rotating pipes in the grid
/// </remarks>
public class PipePuzzle : AbstractPuzzle
{
    [Header("generating pipes")]
    [SerializeField] private GameObject cross4Prefab;
    [SerializeField] private GameObject cross3Prefab;
    [SerializeField] private GameObject curvePrefab;
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private GameObject startTubePrefab;
    [SerializeField] private GameObject endTubePrefab;
    [SerializeField] private GameObject waterTapPrefab;
    [SerializeField] private GameObject pipesHolder;
    [SerializeField] private GameObject center;//center of the table
    private float pipeSize;

    [SerializeField] private Transform startSpot;//place for the pipe in start phase
    private List<GameObject> helpPipes = new List<GameObject>();//start and end pipes (the ones that do not rotate)

    private List<List<PipeTile>> pipeList;
    private bool pathFound = false;

    [Header("button on the table")]
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform buttonSpot;
    private GameObject button;

    [Header("model pictures")]
    [SerializeField] private GameObject startPanelPrefab;
    [SerializeField] private GameObject tutPanelPrefab;
    [SerializeField] private GameObject phasePanelPrefab;

    [Header("stuff for menu settings")]
    [SerializeField] private GameObject table;//to be able to get table size
    [SerializeField] private Sprite pipeImage;
    private List<string> widths = new List<string>();
    private List<string> heigths = new List<string>();


    private void Start()
    {
        pipeSize = 0.2f;
        pipeList = new List<List<PipeTile>>();
        TypeName = "PipePuzzle";

        //find out how many pipes can fit on the table
        float tableWidth = table.transform.lossyScale.x;
        float tableHeigth = table.transform.lossyScale.z;
        int maxWidth = (int)((tableWidth / 1.5f) / pipeSize);
        int maxHeigth = (int)(tableHeigth / pipeSize)+1;

        //set the content of dropdowns in interactibleInfoPanelPrefab accordingly 
        SetNumberOfPuzzlesDropdownContent(maxWidth, maxHeigth);
    }


    //--------------------------------------------------
    //for preparation of the puzzle in menu
    //--------------------------------------------------

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

    public override string FillTheInfoPanel(GameObject panel, PuzzleData puzzle)
    {
        panel.GetComponentInChildren<Text>().text = puzzle.widthpx + " x " + puzzle.heigthpx;
        List<Image> images = new List<Image>();
        panel.GetComponentsInChildren<Image>(images);
        images[1].sprite = pipeImage;
        return null;
    }

    public override string CheckForMissingThings(Configuration c)
    {
        return null;
    }

    public override void PrepareInteractibleInfoPanel(GameObject panel, int i)
    {
        List<Dropdown> droplist = new List<Dropdown>();
        panel.GetComponentsInChildren<Dropdown>(droplist);
        droplist[0].ClearOptions();
        droplist[0].AddOptions(widths);
        droplist[1].ClearOptions();
        droplist[1].AddOptions(heigths);
        panel.GetComponentInChildren<Button>().image.sprite = pipeImage;
        panel.GetComponentInChildren<InputField>().onValueChanged.AddListener(delegate { panel.GetComponentInChildren<InputField>().image.color = Color.white; });
    }

    public override bool CheckFillingCorrect(GameObject panel, int i)
    {
        panel.GetComponentInChildren<Button>().image.color = Color.white;
        return true;
    }
    public override PuzzleData CreatePuzzle(GameObject panel, int i)
    {
        PuzzleData p = new PuzzleData();

        List<Dropdown> droplist = new List<Dropdown>();
        panel.GetComponentsInChildren<Dropdown>(droplist);
        p.heigthpx = droplist[1].value + 1;
        p.widthpx = droplist[0].value + 1;
        p.chosenList = ChoosePath(p.heigthpx, p.widthpx);

        return p;
    }

    public override InputField GetPuzzleName(GameObject panel)
    {
        InputField puzzleNameField = panel.GetComponentInChildren<InputField>();
        return puzzleNameField;
    }


    //-------------------------------------------------------------------------
    //for creating content of the puzzle so that it has a solution
    //-------------------------------------------------------------------------

    /// <summary>
    /// <para>generates a grid of size <paramref name="h"/> x <paramref name="w"/> full of chars representing types of pipes</para>
    /// <para>instead of "List of List of char" uses "List of Wrapper" because of Unity serialization</para>
    /// </summary>
    /// <param name="h">heigth</param>
    /// <param name="w">width</param>
    /// <returns>2D list of chars reprezenting pipe types in the grid</returns>
    private List<Wrapper> ChoosePath(int h, int w)
    {
        List<Wrapper> chosenList = new List<Wrapper>();
        //create empty dfs state field (full of 'f'="fresh" nodes)
        for (int i = 0; i < h; i++)
        {
            chosenList.Add(new Wrapper());
            chosenList[i].row = new List<char>();
            for (int j = 0; j < w; j++)
            {
                chosenList[i].row.Add('f');
            }
        }
        pathFound = false;
        //choose field of the grid through which the path will lead
        DFS(0, 0, h, w, chosenList);
        //choose correct pipe types for these field, for other fields choose random pipe types
        return ChooseAllPipes(h, w, chosenList);
    }

    /// <summary>
    /// finds a path from start to end pipe and marks in 'o' in <paramref name="chosenList"/>
    /// </summary>
    /// <param name="i">heigth coordinate of current position in grid</param>
    /// <param name="j">width coordinate of current position in grid</param>
    /// <param name="h">heigth of the puzzle grid</param>
    /// <param name="w">width of the puzzle grid</param>
    /// <param name="chosenList">2D list of char, used to mark the path</param>
    private void DFS(int i, int j, int h, int w, List<Wrapper> chosenList)
    {   //('f'="fresh", 'o'="open", 'c'="closed")

        chosenList[i].row[j] = 'o';

        //end condition (reached the end pipe)
        if (i == h - 1 && j == w - 1)
        {
            pathFound = true;
            //now the fields chosen for the path are the ones marked as "open"
        }

        //take directions in random order
        List<int> directions = new List<int> { 0, 1, 2, 3 };
        ShuffleList(directions);

        foreach (int x in directions)
        {
            if (!pathFound)
            {
                switch (x)
                {
                    case 0: if (i + 1 < h && chosenList[i + 1].row[j] == 'f') DFS(i + 1, j, h, w, chosenList); break;//go up
                    case 1: if (j + 1 < w && chosenList[i].row[j + 1] == 'f') DFS(i, j + 1, h, w, chosenList); break;//go right
                    case 2: if (i - 1 >= 0 && chosenList[i - 1].row[j] == 'f') DFS(i - 1, j, h, w, chosenList); break;//go down
                    case 3: if (j - 1 >= 0 && chosenList[i].row[j - 1] == 'f') DFS(i, j - 1, h, w, chosenList); break;//go left
                }
            }
        }

        //dead end condition
        if (!pathFound)
            chosenList[i].row[j] = 'c';
    }

    /// <summary>
    /// goes through <paramref name="chosenList"/> and calls ChoosePipe on each field
    /// </summary>
    /// <param name="h">puzzle heigth</param>
    /// <param name="w">puzzle width</param>
    /// <param name="chosenList">2D list of char with 'o' marking the selected path</param>
    /// <returns>2D list of char, each char representing a pipe type</returns>
    private List<Wrapper> ChooseAllPipes(int h, int w, List<Wrapper> chosenList)
    {
        List<Wrapper> otherList = new List<Wrapper>();
        for (int i = 0; i < h; i++)
        {
            otherList.Add(new Wrapper());
            otherList[i].row = new List<char>();
            for (int j = 0; j < w; j++)
            {
                otherList[i].row.Add(ChoosePipe(i, j, h, w, chosenList));
            }
        }

        return otherList;
    }

    /// <summary>
    /// <para>decides which pipe type to put at position [i,j] in the grid</para>
    /// <para>puts correct pipe types on 'o' places and random types on other places</para>
    /// <para>'t' = cross3Prefab, 'x' = cross4Prefab, 'c' = curvePrefab, 's' = linePrefab</para>
    /// </summary>
    /// <param name="i">heigth coordinate of current position in grid</param>
    /// <param name="j">width coordinate of current position in grid</param>
    /// <param name="h">puzzle heigth</param>
    /// <param name="w">puzzle width</param>
    /// <param name="chosenList">2D list of char with 'o' marking the selected path</param>
    /// <returns>char representing pipe type chosen for this position</returns>
    private char ChoosePipe(int i, int j, int h, int w, List<Wrapper> chosenList)
    {
        if (chosenList[i].row[j] == 'o')
        {
            bool u, r, d, l;//has up/right/down/left neighbour which is also marked as 'o'
            int n = 0;//number of 'o' neighbours
            if (i - 1 >= 0) d = chosenList[i - 1].row[j] == 'o'; else d = false;
            if (i + 1 < h) u = chosenList[i + 1].row[j] == 'o'; else u = false;
            if (j + 1 < w) r = chosenList[i].row[j + 1] == 'o'; else r = false;
            if (j - 1 >= 0) l = chosenList[i].row[j - 1] == 'o'; else l = false;

            //special for first and last pipe
            if (i == 0 && j == 0)
                l = true;
            if (i == h - 1 && j == w - 1)
                r = true;

            if (d) n++;
            if (u) n++;
            if (r) n++;
            if (l) n++;


            //choose according to the number of 'o' neighbours
            if (n == 4)
                return 'x';//cross4Prefab;
            else if (n == 3)
                return 't';// cross3Prefab;
            else if ((u && d) || (r && l))
                return 's';// linePrefab;
            else if (n == 2)
                return 'c';// curvePrefab;
        }
        else//others chosen randomly
        {
            int r = Random.Range(0, 4);
            switch (r)
            {
                case 0: return 't';//cross3Prefab;
                case 1: return 'x';//cross4Prefab;
                case 2: return 'c';//curvePrefab;
                case 3: return 's';//linePrefab;
            }
        }
        return 'a';//this should not happen
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





    //------------------------------------------------------------------------------------------------
    // implementation of AbstractPuzzle abstract methods - things needed for running the puzzle
    //------------------------------------------------------------------------------------------------

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
    /// destroys all pipes
    /// </summary>
    private void BasicFinish()
    {
        //destroy pipes
        foreach(List<PipeTile> row in pipeList)
        {
            foreach (PipeTile pipe in row)
            {
                Destroy(pipe.gameObject);
            }
        }
        pipeList.Clear();

        //destroy "help" pipes (=start tube, end tube atd.)
        foreach (GameObject p in helpPipes)
        {
            Destroy(p);
        }
        helpPipes.Clear();
    }

    public override void FinishConfig()
    {
        //destroy the button
        if(button!=null)
            Destroy(button);
        button = null;
    }
    
    public override void StartConfig(Configuration c)
    {
        //instantiate the button
        button = Instantiate(buttonPrefab, buttonSpot.position,buttonPrefab.transform.rotation);
    }

    public override void StartPhase()
    {
        //create the puzzle
        NewManager nm = NewManager.instance;
        GeneratePipes(nm.ActiveConfig.puzzles[nm.ActivePuzzleIndex].heigthpx, nm.ActiveConfig.puzzles[nm.ActivePuzzleIndex].widthpx);

        //set up the wall picture
        float x = 5 * pipeSize * 2f;
        float y = 3 * pipeSize * 2f;
        nm.MultiplyWallpictureScale(x, y);
        NewManager.instance.SetWallPicturePanel(phasePanelPrefab);
    }

    public override void StartStart()
    {
        //create the start tube
        GameObject cc = Instantiate(startTubePrefab,startSpot.position,startTubePrefab.transform.rotation);
        cc.transform.localScale = new Vector3(pipeSize, pipeSize, pipeSize);
        helpPipes.Add(cc);

        //set up the wall picture
        float x = 5 * pipeSize * 2f;
        float y = 3 * pipeSize * 2f;
        NewManager.instance.MultiplyWallpictureScale(x, y);
        NewManager.instance.SetWallPicturePanel(startPanelPrefab);
    }

    public override void OnTableHeigthChange()
    {
        //this will be called only during start phase, so it adjusts only the position of the start pipe and the button
        if (helpPipes.Count > 0)
            helpPipes[0].transform.position = startSpot.position;
        if (button != null)
            button.transform.position = buttonSpot.position;
    }

    public override void StartTut()
    {
        //create tutorial puzzle
        GeneratePipes(2, 2);

        //set up the wall picture
        float x = 5 * pipeSize * 2f;
        float y = 3 * pipeSize * 2f;
        NewManager.instance.MultiplyWallpictureScale(x, y);
        NewManager.instance.SetWallPicturePanel(tutPanelPrefab);
    }

    public override void OnTimerStop()
    {
        //if the puzzle is solved increase score
        NewManager nm = NewManager.instance;
        if (!nm.PhaseFinished)
        {
            if (CheckComplete(nm.ActiveConfig.puzzles[nm.ActivePuzzleIndex].heigthpx, nm.ActiveConfig.puzzles[nm.ActivePuzzleIndex].widthpx))
                nm.IncreaseScore();

            nm.SetPhaseComplete();
        }
    }





    //------------------------------------------------------------------------------------------------
    // other methods - for creating the puzzle and for checking if it is solved
    //------------------------------------------------------------------------------------------------

    /// <summary>
    /// checks if there is a closed path from strat to end pipe (aka puzzle is solved), plays particle systems for "leaking water"
    /// </summary>
    /// <param name="h">heigth</param>
    /// <param name="w">width</param>
    /// <returns>true = puzzle solved</returns>
    private bool CheckComplete(int h, int w)
    {
        //setup
        List<PipeTile> bluePipes = new List<PipeTile>();
        List<List<bool>> seen = new List<List<bool>>();
        for (int i = 0; i < pipeList.Count; i++)
        {
            seen.Add(new List<bool>());
            for (int j = 0; j< pipeList[i].Count;j++)
            {
                seen[i].Add(false);
            }
        }
        bool ret = true;

        //special tiles
        if (pipeList[h - 1][w - 1].right == false)//if the first pipe does not fit the start pipe there is no need to continue checking
        {
            helpPipes[helpPipes.Count - 1].gameObject.GetComponentInChildren<ParticleSystem>().Play();
            return false;
        }
        if(pipeList[0][0].left == false)//check if final pipe fits the end pipe
        {
            ret = false;
        }

        //flooding algorithm
        Queue<PipeTile> qu = new Queue<PipeTile>();
        qu.Enqueue(pipeList[h - 1][w - 1]);
        bluePipes.Add(pipeList[h - 1][w - 1]);
        seen[h - 1][w - 1] = true;
        while (qu.Count != 0)
        {
            PipeTile p = qu.Dequeue();
            if (p.up)//if pipe p has an up end
            {
                if (p.I + 1 < h)//and is not in the topmost position
                {
                    if (pipeList[p.I + 1][p.J].down)//if the pipe above it has a down end
                    {
                        if (seen[p.I + 1][p.J] == false)//... and we have not seen it yet
                        {
                            seen[p.I + 1][p.J] = true;//visit it
                            qu.Enqueue(pipeList[p.I + 1][p.J]);//and remember to check its neighbours too
                            bluePipes.Add(pipeList[p.I + 1][p.J]);
                        }
                    }
                    else
                    {
                        //this pipe does not fit
                        if (p.particleUp!=null)
                            p.particleUp.Play();
                        ret = false; 
                    }
                }
                else
                {
                    //this pipe leaks out of the grid
                    if (p.particleUp != null)
                        p.particleUp.Play();
                    ret = false;
                }
            }
            if (p.right && !(p.I == h - 1 && p.J == w - 1))//first pipe has start pipe on the right
            {
                if (p.J + 1 < w)
                {
                    if (pipeList[p.I][p.J + 1].left)//the same for right neighbour
                    {
                        if (seen[p.I][p.J + 1] == false)
                        {
                            seen[p.I][p.J + 1] = true;
                            qu.Enqueue(pipeList[p.I][p.J + 1]);
                            bluePipes.Add(pipeList[p.I][p.J + 1]);
                        }
                    }
                    else
                    {
                        //this pipe does not fit
                        if (p.particleRight != null)
                            p.particleRight.Play();
                        ret = false; 
                    }
                }
                else
                {
                    //this pipe leaks out of the grid
                    if (p.particleRight != null)
                        p.particleRight.Play();
                    ret = false;
                }
            }
            if (p.down)
            {
                if (p.I - 1 >= 0)
                {
                    if (pipeList[p.I - 1][p.J].up)//the same for down neighbour
                    {
                        if (seen[p.I - 1][p.J] == false)
                        {
                            seen[p.I - 1][p.J] = true;
                            qu.Enqueue(pipeList[p.I - 1][p.J]);
                            bluePipes.Add(pipeList[p.I - 1][p.J]);
                        }
                    }
                    else
                    {
                        //this pipe does not fit
                        if (p.particleDown != null)
                            p.particleDown.Play();
                        ret = false;
                    }
                }
                else
                {
                    //this pipe leaks out of the grid
                    if (p.particleDown != null)
                        p.particleDown.Play();
                    ret = false;
                }
            }
            if (p.left && !(p.I == 0 && p.J == 0))//last pipe has end pipe on the left
            {
                if (p.J - 1 >= 0)
                {
                    if (pipeList[p.I][p.J - 1].right)//the same for left neighbour
                    {
                        if (seen[p.I][p.J - 1] == false)
                        {
                            seen[p.I][p.J - 1] = true;
                            qu.Enqueue(pipeList[p.I][p.J - 1]);
                            bluePipes.Add(pipeList[p.I][p.J - 1]);
                        }
                    }
                    else
                    {
                        //this pipe does not fit
                        if (p.particleLeft != null)
                            p.particleLeft.Play();
                        ret = false;
                    }
                }
                else
                {
                    //this pipe leaks out of the grid
                    if (p.particleLeft != null)
                        p.particleLeft.Play();
                    ret = false;
                }
            }
        }

        //if the solution is correct, this colors the path (from start tube to end tube) blue
        if(ret==true)
        {
            foreach(PipeTile p in bluePipes)
            {
                foreach (var m in p.gameObject.GetComponentsInChildren<MeshRenderer>())
                {
                    m.material.color = new Color(0.2f,1,1);
                    p.blue = true;
                }
            }
        }

        return ret;
    }

    /// <summary>
    /// generates the actual puzzle on the table
    /// </summary>
    /// <param name="h">heigth</param>
    /// <param name="w">width</param>
    private void GeneratePipes(int h, int w)
    {
        float o = 0.05f;//offset

        //place the pipesHolder on the table
        Vector3 ctp = center.transform.position;
        pipesHolder.transform.position = new Vector3(ctp.x - ((w * (pipeSize - o)) / 2) + ((pipeSize - o) / 2), ctp.y+o, ctp.z - ((h * (pipeSize - o)) / 2) + ((pipeSize - o) / 2));
        
        //create end tube
        GameObject cc = Instantiate(endTubePrefab);
        cc.transform.SetParent(pipesHolder.transform);
        cc.transform.localScale = new Vector3(pipeSize, pipeSize, pipeSize);
        cc.transform.localPosition = new Vector3(1 * (pipeSize - o), -0.022f, 0 * (pipeSize - o));
        helpPipes.Add(cc);

        //create other tubes
        for (int i = 0; i < h; i++)
        {
            pipeList.Add(new List<PipeTile>());
            for (int j = 0; j < w; j++)
            {
                //decide which pipe prefab to create
                int k;
                GameObject pipePrefab;
                if (NewManager.instance.InTut)
                {
                    pipePrefab = LoadTutPipe(i, j);
                }
                else
                {
                    k = NewManager.instance.ActivePuzzleIndex;
                    pipePrefab = LoadPipe(i, j, k);
                }

                //create it
                GameObject c = Instantiate(pipePrefab);
                c.transform.SetParent(pipesHolder.transform);
                c.transform.localScale = new Vector3(pipeSize, pipeSize, pipeSize);
                c.transform.localPosition = new Vector3(-j * (pipeSize-o), 0, -i * (pipeSize - o));

                //store info abou it
                PipeTile pt;
                if ((pt = c.GetComponent<PipeTile>()) == null)
                    c.gameObject.AddComponent<PipeTile>();
                pipeList[i].Add(pt);
                pt.Initialize(i, j);
            }
        }

        //create start tube
        cc = Instantiate(startTubePrefab);
        cc.transform.SetParent(pipesHolder.transform);
        cc.transform.localScale = new Vector3(pipeSize, pipeSize, pipeSize);
        cc.transform.localPosition = new Vector3(-w * (pipeSize - o), -0.022f, -(h-1)* (pipeSize - o));
        helpPipes.Add(cc);
    }

    /// <summary>
    /// choose which pipe prefab tu put at [i,j] in tutorial
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    /// <returns>pipe prefab</returns>
    private GameObject LoadTutPipe(int i, int j)
    {
        if (i == 0 && j == 0) return cross3Prefab;
        if (i == 1 && j == 1) return cross3Prefab;
        if (i == 0 && j == 1) return curvePrefab;
        if (i == 1 && j == 0) return curvePrefab;

        return null;
    }
    /// <summary>
    /// choose which pipe prefab tu put at [i,j] in k-th puzzle
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    /// <param name="k"></param>
    /// <returns>pipe prefab</returns>
    private GameObject LoadPipe(int i, int j, int k)
    {
        Configuration c = NewManager.instance.ActiveConfig;
        if (c.puzzles[k].chosenList[i].row[j] == 'c') return curvePrefab;
        if (c.puzzles[k].chosenList[i].row[j] == 's') return linePrefab;
        if (c.puzzles[k].chosenList[i].row[j] == 't') return cross3Prefab;
        if (c.puzzles[k].chosenList[i].row[j] == 'x') return cross4Prefab;
        return null;
    }

    /// <summary>
    /// check if puzzle solved (is invoked by the button on the table)
    /// </summary>
    public void Check()
    {
        NewManager nm = NewManager.instance;
        if (nm.InStart)//in start phase just play water animation
        {
            nm.SetPhaseComplete();
            helpPipes[helpPipes.Count - 1].gameObject.GetComponentInChildren<ParticleSystem>().Play();
        }
        else if (nm.InTut)//in tutorial check, but only 2x2 grid
        {
            PipePuzzle pppp = (PipePuzzle)nm.CurrentPuzzle;
            if (pppp.CheckComplete(2, 2))
                nm.IncreaseScore();
            nm.SetPhaseComplete();
        }
        else//in regular phase check heigth x width grid
        {
            PipePuzzle pppp = (PipePuzzle)nm.CurrentPuzzle;

            if (pppp.CheckComplete(nm.ActiveConfig.puzzles[nm.ActivePuzzleIndex].heigthpx, nm.ActiveConfig.puzzles[nm.ActivePuzzleIndex].widthpx))
                nm.IncreaseScore();

            nm.SetPhaseComplete();
        }
    }




    //------------------------------------------------------------------------------------------------
    // methods for logging and replaying the logfile
    //------------------------------------------------------------------------------------------------

    public override void Simulate(string[] atoms)
    {
        int i = 0; int j = 0;
        switch (atoms[1])//there should be the name of the action (it is already checked in Logger that atoms.Count>1)
        {
            case "Rotate":
                if (atoms.Length == 4 && int.TryParse(atoms[2], out i) && int.TryParse(atoms[3], out j))
                {
                    Rotace(i, j);
                    break;
                }
                else
                {
                    ErrorCatcher.instance.Show("Wrong format of this logfile: " + Logger.instance.PathToLogFile + " (line " + atoms[0] + " has wrong Rotate arguments)");
                    return;
                }
            case "Press": Press(); break;
            default:
                {
                    ErrorCatcher.instance.Show("Wrong format of this logfile: " + Logger.instance.PathToLogFile + " (line " + atoms[0] + " is not a valid action)");
                    break;
                }
        }
    }

    /// <summary>
    /// simulate rotation of pipe [i,j]
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    private void Rotace(int i, int j)
    {
        if (i >= pipeList.Count || j >= pipeList[i].Count) 
            { ErrorCatcher.instance.Show("Wrong format of this logfile: " + Logger.instance.PathToLogFile + " (illegal Rotate action)"); return; }

        pipeList[i][j].OnTriggerPressed(null);
    }
    /// <summary>
    /// simulate pressing of the button on the table
    /// </summary>
    private void Press()
    {
        button.GetComponentInChildren<ButtonScript>().ClickEffect();
    }

}
