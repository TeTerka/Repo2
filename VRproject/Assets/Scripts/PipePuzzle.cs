using System.Collections.Generic;
using UnityEngine;

public class PipePuzzle : AbstractPuzzle
{
    [Header("generating pipes")]
    public GameObject cross4Prefab;
    public GameObject cross3Prefab;
    public GameObject curvePrefab;
    public GameObject linePrefab;
    public GameObject startTubePrefab;
    public GameObject endTubePrefab;
    public GameObject waterTapPrefab;
    public GameObject pipesHolder;
    public GameObject center;//center of the table
    public float PipeSize { get; private set; }

    public Transform startSpot;//misto pro samostatnou trubku s fazi startu
    private List<GameObject> helpPipes = new List<GameObject>();//koncove a pocatecni trubky (neboli ty co se neotaci)

    public List<List<PipeTile>> pipeList = new List<List<PipeTile>>();//public jen kvuli loggeru....to se musi zmenit!!!!!!!!staci get
    private bool pathFound = false;

    [Header("button on the table")]
    public GameObject buttonPrefab;
    public Transform buttonSpot;
    public GameObject button;//public jen kvuli loggeru!!!!!!!!!!!!!!!!!!!!!!!!staci get

    [Header("model pictures")]
    public Texture2D startPicture;
    public Texture2D tutPicture;
    public Texture2D phasePicture;

    private void Start()
    {
        PipeSize = 0.2f;
    }


    public override void CustomUpdate()
    {
        //nic
    }

    public override void OnTimerStop()//po vyprseni casu je treba provest totez, co pri stisknuti tlacitka na stole
        //jen tu neni treba resit inStart a inTut, protoze ti timer nemaji
    {
        if (!NewManager.instance.PhaseFinished)
        {
            if (CheckComplete(NewManager.instance.ActiveConfig.puzzles[NewManager.instance.ActivePuzzleIndex].heigthpx, NewManager.instance.ActiveConfig.puzzles[NewManager.instance.ActivePuzzleIndex].widthpx))
                NewManager.instance.IncreaseScore();

            NewManager.instance.SetPhaseComplete();
        }
    }

    public void Check()//kontrola volana (obvykle) stiskem tlacitka na stole
    {
        if (NewManager.instance.InStart)//ve startu tlacitko spusti animaci vody a to je vse
        {
            NewManager.instance.SetPhaseComplete();
            helpPipes[helpPipes.Count - 1].gameObject.GetComponentInChildren<ParticleSystem>().Play();
        }
        else if (NewManager.instance.InTut)//v tutorialu normalne zkontroluje existenci uzavrene cesty, jen to bude na poli 2x2
        {
            PipePuzzle pppp = (PipePuzzle)NewManager.instance.CurrentPuzzle;
            if (pppp.CheckComplete(2, 2))
                NewManager.instance.IncreaseScore();
            NewManager.instance.SetPhaseComplete();
        }
        else//(tedy v normalni fazi) zkontroluje existenci uzavrene cesty na poli heigth x width
        {
            PipePuzzle pppp = (PipePuzzle)NewManager.instance.CurrentPuzzle;

            if (pppp.CheckComplete(NewManager.instance.ActiveConfig.puzzles[NewManager.instance.ActivePuzzleIndex].heigthpx, NewManager.instance.ActiveConfig.puzzles[NewManager.instance.ActivePuzzleIndex].widthpx))
                NewManager.instance.IncreaseScore();

            NewManager.instance.SetPhaseComplete();
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

    public void BasicFinish()
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

        //pozn.: netreba zastavovat particle systemy, protoze jsou na trubkach, ktere jsou stejne zniceny
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
        button = Instantiate(buttonPrefab, buttonSpot);
    }
    
    public override void StartPhase()
    {
        GeneratePipes(NewManager.instance.ActiveConfig.puzzles[NewManager.instance.ActivePuzzleIndex].heigthpx, NewManager.instance.ActiveConfig.puzzles[NewManager.instance.ActivePuzzleIndex].widthpx);

        //nastav velikost a obsah platna
        float x = 5 * PipeSize * 2f;
        float y = 3 * PipeSize * 2f;
        NewManager.instance.MultiplyWallpictureScale(x, y);
        NewManager.instance.SetWallPicture(phasePicture);
    }

    public override void StartStart()
    {
        //start tube
        GameObject cc = Instantiate(startTubePrefab);
        cc.transform.SetParent(startSpot);
        cc.transform.localScale = new Vector3(PipeSize, PipeSize, PipeSize);
        cc.transform.localPosition = new Vector3(0, 0, 0 );
        helpPipes.Add(cc);

        //nastav velikost a obsah platna
        float x = 5 * PipeSize * 2f;
        float y = 3 * PipeSize * 2f;
        NewManager.instance.MultiplyWallpictureScale(x, y);
        NewManager.instance.SetWallPicture(startPicture);
    }
    
    public override void StartTut()
    {
        GeneratePipes(2, 2);

        //nastav velikost a obsah platna
        float x = 5 * PipeSize * 2f;
        float y = 3 * PipeSize * 2f;
        NewManager.instance.MultiplyWallpictureScale(x, y);
        NewManager.instance.SetWallPicture(tutPicture);
    }

    public List<Wrapper> ChoosePath(int h, int w)//hloupe nahodne vygeneruje mapu velikosti (h x w) tak, aby mela aspon jedno reseni
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
        //vyber pole mrizky, pres ktera povede cesta
        DFS(0, 0, h, w,chosenList);
        //vyber konkretni trubky na tato pole a nahodne trubky na ostatni mista
        return ChooseAllPipes(h, w, chosenList);
    }

    private List<Wrapper> ChooseAllPipes(int h, int w, List<Wrapper> chosenList)//kde je 'o', tam chci dat konkretni trubku viz ChoosePipe(i,j), jinak nahodnou viz totez...
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

    private void DFS(int i, int j, int h, int w, List<Wrapper> chosenList)
    {   //('f'="fresh", 'o'="open", 'c'="closed")

        chosenList[i].row[j] = 'o';

        //ukoncovaci podminka
        if (i == h - 1 && j == w - 1)
        {
            pathFound = true;//aby se uz nepokracovalo v rekurzi
            //pozn.: vybrane jsou ty policka, ktera jsou v tuto chvili "open"
        }

        //ber smery v nahodnem poradi
        List<int> directions = new List<int> { 0, 1, 2, 3 };
        ShuffleList(directions);
        foreach (int x in directions)
        {
            if (!pathFound)
            {
                switch (x)
                {
                    case 0: if (i + 1 < h && chosenList[i + 1].row[j] == 'f') DFS(i + 1, j, h, w,chosenList); break;//up
                    case 1: if (j + 1 < w && chosenList[i].row[j + 1] == 'f') DFS(i, j + 1, h, w,chosenList); break;//right
                    case 2: if (i - 1 >= 0 && chosenList[i - 1].row[j] == 'f') DFS(i - 1, j, h, w,chosenList); break;//down
                    case 3: if (j - 1 >= 0 && chosenList[i].row[j - 1] == 'f') DFS(i, j - 1, h, w,chosenList); break;//left
                }
            }
        }

        if (!pathFound)
            chosenList[i].row[j] = 'c';
    }

    private char ChoosePipe(int i, int j, int h, int w, List<Wrapper> chosenList)//po dfs (jehoz vystupem je chosenList) vyplni "open" policka trubkami aby navazovaly
                                                                                 // na ne "open" da nahodne trubky
    {        
        //vypln vhodne tato policka, ostatni vypln klidne nahodne
        if (chosenList[i].row[j] == 'o')
        {
            bool u, r, d, l;//budou znacit, jestli ma up/right/down/left souseda, ktery je taky 'o'
            int n = 0;
            if (i - 1 >= 0) d = chosenList[i - 1].row[j] == 'o'; else d = false;
            if (i + 1 < h) u = chosenList[i + 1].row[j] == 'o'; else u = false;
            if (j + 1 < w) r = chosenList[i].row[j + 1] == 'o'; else r = false;
            if (j - 1 >= 0) l = chosenList[i].row[j - 1] == 'o'; else l = false;

            //specialne pro pocatecni a koncovou trubku
            if (i == 0 && j == 0)
                l = true;
            if (i == h - 1 && j == w - 1)
                r = true;

            if (d) n++;
            if (u) n++;
            if (r) n++;
            if (l) n++;


            //podle poctu 'o' sousedu
            if (n == 4)
                return 'x';//cross4Prefab;
            else if (n == 3)
                return 't';// cross3Prefab;
            else if ((u && d) || (r && l))
                return 's';// linePrefab;
            else if (n == 2)
                return 'c';// curvePrefab;
        }
        else//vyber nahodne
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
        return 'a';//to by se nemelo stat...
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

    private bool CheckComplete(int h, int w)//zjisti jestli existuje cesta ze startu do cile a jestli v ni nejsou diry (pokud jsou, spusti tam particle systemy(vodu))
    {
        //setup
        foreach (List<PipeTile> item in pipeList)
        {
            foreach (PipeTile p in item)
            {
                p.seen = false;
            }
        }

        bool ret = true;

        //special tiles
        if (pipeList[h - 1][w - 1].right == false)//sedi pocatecni dilek
        {
            helpPipes[helpPipes.Count - 1].gameObject.GetComponentInChildren<ParticleSystem>().Play();
            return false;//pokud nesedi prvni dilek, neni treba nic vic kontrolovat
        }
        if(pipeList[0][0].left == false)//sedi koncovy dilek
        {
            ret = false;
        }

        //provede upravene bfs
        Queue<PipeTile> qu = new Queue<PipeTile>();
        qu.Enqueue(pipeList[h - 1][w - 1]);
        pipeList[h - 1][w - 1].seen = true;
        while (qu.Count != 0)
        {
            PipeTile p = qu.Dequeue();
            if (p.up)
            {
                if (p.I + 1 < h)
                {
                    if (pipeList[p.I + 1][p.J].down)//tedy pokud ma korektniho horniho souseda...
                    {
                        if (pipeList[p.I + 1][p.J].seen == false)//... ktereho jsme jeste nenavstivili
                        {
                            pipeList[p.I + 1][p.J].seen = true;//navstivme ho
                            qu.Enqueue(pipeList[p.I + 1][p.J]);//a poznamenejme si, ze chceme pak prozkoumat jeste i jeho sousedy
                        }
                    }
                    else
                    {
                        if(p.particleUp!=null)
                            p.particleUp.Play();
                        ret = false; //trubky nenavazuji
                    }
                }
                else
                {
                    if (p.particleUp != null)
                        p.particleUp.Play();
                    ret = false;//trubka ven z planku
                }
            }
            if (p.right && !(p.I == h - 1 && p.J == w - 1))//u prvniho dilku nekontroluju co je v pravo
            {
                if (p.J + 1 < w)
                {
                    if (pipeList[p.I][p.J + 1].left)//tedy pokud ma korektniho praveho souseda
                    {
                        if (pipeList[p.I][p.J + 1].seen == false)
                        {
                            pipeList[p.I][p.J + 1].seen = true;
                            qu.Enqueue(pipeList[p.I][p.J + 1]);
                        }
                    }
                    else
                    {
                        if (p.particleRight != null)
                            p.particleRight.Play();
                        ret = false; //trubky nenavazuji
                    }
                }
                else
                {
                    if (p.particleRight != null)
                        p.particleRight.Play();
                    ret = false;//trubka ven z planku
                }
            }
            if (p.down)
            {
                if (p.I - 1 >= 0)
                {
                    if (pipeList[p.I - 1][p.J].up)//tedy pokud ma korektniho dolniho souseda
                    {
                        if (pipeList[p.I - 1][p.J].seen == false)
                        {
                            pipeList[p.I - 1][p.J].seen = true;
                            qu.Enqueue(pipeList[p.I - 1][p.J]);
                        }
                    }
                    else
                    {
                        if (p.particleDown != null)
                            p.particleDown.Play();
                        ret = false; //trubky nenavazuji
                    }
                }
                else
                {
                    if (p.particleDown != null)
                        p.particleDown.Play();
                    ret = false;//trubka ven z planku
                }
            }
            if (p.left && !(p.I == 0 && p.J == 0))//u posledniho dilku nekontroluju co je v levo
            {
                if (p.J - 1 >= 0)
                {
                    if (pipeList[p.I][p.J - 1].right)//tedy pokud ma korektniho leveho souseda
                    {
                        if (pipeList[p.I][p.J - 1].seen == false)
                        {
                            pipeList[p.I][p.J - 1].seen = true;
                            qu.Enqueue(pipeList[p.I][p.J - 1]);
                        }
                    }
                    else
                    {
                        if (p.particleLeft != null)
                            p.particleLeft.Play();
                        ret = false; //trubky nenavazuji
                    }
                }
                else
                {
                    if (p.particleLeft != null)
                        p.particleLeft.Play();
                    ret = false;//trubka ven z planku
                }
            }
        }

        return ret;
    }

    private void GeneratePipes(int h, int w)
    {
        //vsude ti je nejake +-0.5, avsak nevim, je-li to spravne....!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    
        //umisti sparavne containersHolder
        pipesHolder.transform.position = new Vector3(center.transform.position.x - ((w * (PipeSize - 0.05f)) / 2) + ((PipeSize - 0.05f) / 2), center.transform.position.y+0.05f, center.transform.position.z - ((h * (PipeSize - 0.05f)) / 2) + ((PipeSize - 0.05f) / 2));
        
        //generuj end tube
        GameObject cc = Instantiate(endTubePrefab);
        cc.transform.SetParent(pipesHolder.transform);
        cc.transform.localScale = new Vector3(PipeSize, PipeSize, PipeSize);
        cc.transform.localPosition = new Vector3(2 * (PipeSize - 0.05f), -0.022f, 0 * (PipeSize - 0.05f));//fuj -0.022 napevno......!!!!!!!!!!!!!!!!
        helpPipes.Add(cc);

        //fancy end tube (not neccessary but has a nice valve)
        cc = Instantiate(waterTapPrefab);
        cc.transform.SetParent(pipesHolder.transform);
        cc.transform.localScale = new Vector3(PipeSize, PipeSize, PipeSize);
        cc.transform.localPosition = new Vector3(1 * (PipeSize - 0.05f), 0, 0 * (PipeSize - 0.05f));
        helpPipes.Add(cc);

        //other tubes
        for (int i = 0; i < h; i++)
        {
            pipeList.Add(new List<PipeTile>());
            for (int j = 0; j < w; j++)
            {
                //zjisti co vlastne chces instanciovat (lisi se podle toho, jestli jde o tutorial, nebo normalni fazi)
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

                //vyrob to
                GameObject c = Instantiate(pipePrefab);
                c.transform.SetParent(pipesHolder.transform);
                c.transform.localScale = new Vector3(PipeSize, PipeSize, PipeSize);
                c.transform.localPosition = new Vector3(-j * (PipeSize-0.05f), 0, -i * (PipeSize - 0.05f));//(cislovani mrizky stejne jako u cubePuzzlu)

                //uloz si o tom info
                PipeTile pt;
                if ((pt = c.GetComponent<PipeTile>()) == null)
                    c.gameObject.AddComponent<PipeTile>();
                pipeList[i].Add(pt);
                pt.Initialize(i, j);
            }
        }

        //start tube
        cc = Instantiate(startTubePrefab);
        cc.transform.SetParent(pipesHolder.transform);
        cc.transform.localScale = new Vector3(PipeSize, PipeSize, PipeSize);
        cc.transform.localPosition = new Vector3(-w * (PipeSize - 0.05f), -0.022f, -(h-1)* (PipeSize - 0.05f));
        helpPipes.Add(cc);
    }

    private GameObject LoadTutPipe(int i, int j)//podiva se kterou trubku ma vygenerovat do tutorialu na policko [i,j]
    {
        if (i == 0 && j == 0) return cross3Prefab;
        if (i == 1 && j == 1) return cross3Prefab;
        if (i == 0 && j == 1) return curvePrefab;
        if (i == 1 && j == 0) return curvePrefab;

        return null;
    }
    private GameObject LoadPipe(int i, int j, int k)//podiva se kterou trubku ma vygenerovat v k-tem puzzle na policko [i,j]
    {
        Configuration c = NewManager.instance.ActiveConfig;
        if (c.puzzles[k].chosenList[i].row[j] == 'c') return curvePrefab;
        if (c.puzzles[k].chosenList[i].row[j] == 's') return linePrefab;
        if (c.puzzles[k].chosenList[i].row[j] == 't') return cross3Prefab;
        if (c.puzzles[k].chosenList[i].row[j] == 'x') return cross4Prefab;
        return null;
    }

}
