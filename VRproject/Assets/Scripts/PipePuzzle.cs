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

    private List<List<PipeTile>> pipeList = new List<List<PipeTile>>();
    private List<List<List<char>>> chosenList = new List<List<List<char>>>();//f = fresh, o = open (=chosen), c = closed.....pro DFS
    private bool pathFound = false;

    [Header("button on the table")]
    public GameObject buttonPrefab;
    public Transform buttonSpot;
    private GameObject button;

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

    public void Check()//kontrola volana (obvzkle) stiskem tlacitka na stole
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
        chosenList.Clear();//staci tohle jako smazani obsahu?

        //destroy the button
        if(button!=null)
            Destroy(button);
        button = null;
    }
    
    public override void StartConfig(Configuration c)
    {
        //instantiate the button
        button = Instantiate(buttonPrefab, buttonSpot);

        //prepare each puzzle
        for (int k = 0; k < c.puzzles.Count; k++)
        {
            //create empty dfs state field (full of "fresh" nodes), one 2D field for each puzzle = > List<List<List<char>>>
            chosenList.Add(new List<List<char>>());
            for (int i = 0; i < c.puzzles[k].heigthpx; i++)
            {
                chosenList[k].Add(new List<char>());
                for (int j = 0; j < c.puzzles[k].widthpx; j++)
                {
                    chosenList[k][i].Add('f');
                }
            }
            //vymysli mapu...bude ulozena prave v tom poli, chosen = "open" nodes
            ChoosePath(k,c.puzzles[k].heigthpx, c.puzzles[k].widthpx);
        }

        //prepare tutorial - za posledni puzzle pridej jeste zaznam pro grid pro tutorial
        if (c.withTutorial)
        {
            chosenList.Add(new List<List<char>>());
            for (int i = 0; i < 2; i++)
            {
                chosenList[c.puzzles.Count].Add(new List<char>());
                for (int j = 0; j < 2; j++)
                {
                    chosenList[c.puzzles.Count][i].Add('f');
                }
            }
            //vymysli mapu...bude ulozena prave v tom poli, chosen = "open" nodes
            ChoosePath(c.puzzles.Count, 2, 2);
        }
    }
    
    public override void StartPhase()
    {
        GeneratePipes(NewManager.instance.ActiveConfig.puzzles[NewManager.instance.ActivePuzzleIndex].heigthpx, NewManager.instance.ActiveConfig.puzzles[NewManager.instance.ActivePuzzleIndex].widthpx);

        //nastav velikost a obsah platna
        float x = 5 * PipeSize * 3f;
        float y = 3 * PipeSize * 3f;
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
        float x = 5 * PipeSize * 3f;
        float y = 3 * PipeSize * 3f;
        NewManager.instance.MultiplyWallpictureScale(x, y);
        NewManager.instance.SetWallPicture(startPicture);
    }
    
    public override void StartTut()
    {
        GeneratePipes(2, 2);

        //nastav velikost a obsah platna
        float x = 5 * PipeSize * 3f;
        float y = 3 * PipeSize * 3f;
        NewManager.instance.MultiplyWallpictureScale(x, y);
        NewManager.instance.SetWallPicture(tutPicture);
    }

    private void ChoosePath(int k, int h, int w)//hloupe nahodne vygeneruje mapu pro k-ty puzzle tak, aby mela aspon jedno reseni
    {
        //setup - nastav vse na "fresh" ()tedy jeste nenavstiveno
        foreach (List<char> item in chosenList[k])
        {
            for (int i = 0; i < item.Count; i++)
            {
                item[i] = 'f';
            }
        }
        pathFound = false;
        //vyber pole mrizky, pres ktera povede cesta
        DFS(0, 0, h, w, k);
    }

    private void DFS(int i, int j, int h, int w, int k)
    {

        chosenList[k][i][j] = 'o';

        //ukoncovaci podminka
        if (i == h - 1 && j == w - 1)
        {
            pathFound = true;//aby se uz nepokracovalo v rekurzi
            //pozn.: vybrane jsou ty policka, ktera jsou v tuto chvili "open"
        }

        List<int> directions = new List<int> { 0, 1, 2, 3 };
        ShuffleList(directions);
        foreach (int x in directions)//ber smery v nahodnem poradi
        {
            if (!pathFound)
            {
                switch (x)
                {
                    case 0: if (i + 1 < h && chosenList[k][i + 1][j] == 'f') DFS(i + 1, j, h, w, k); break;//up
                    case 1: if (j + 1 < w && chosenList[k][i][j + 1] == 'f') DFS(i, j + 1, h, w, k); break;//right
                    case 2: if (i - 1 >= 0 && chosenList[k][i - 1][j] == 'f') DFS(i - 1, j, h, w, k); break;//down
                    case 3: if (j - 1 >= 0 && chosenList[k][i][j - 1] == 'f') DFS(i, j - 1, h, w, k); break;//left
                }
            }
        }

        if (!pathFound)
            chosenList[k][i][j] = 'c';
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

    private bool CheckComplete(int h, int w)//zjisti jestli existuje cesta ze startu do cile a jestli v ni nejsou diry (pokud jsou, spusti tam partcle systemy(vodu))
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
                if (p.i + 1 < h)
                {
                    if (pipeList[p.i + 1][p.j].down)//tedy pokud ma korektniho horniho souseda...
                    {
                        if (pipeList[p.i + 1][p.j].seen == false)//... ktereho jsme jeste nenavstivili
                        {
                            pipeList[p.i + 1][p.j].seen = true;
                            qu.Enqueue(pipeList[p.i + 1][p.j]);
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
            if (p.right && !(p.i == h - 1 && p.j == w - 1))//u prvniho dilku nekontroluju co je v pravo
            {
                if (p.j + 1 < w)
                {
                    if (pipeList[p.i][p.j + 1].left)//tedy pokud ma korektniho praveho souseda
                    {
                        if (pipeList[p.i][p.j + 1].seen == false)
                        {
                            pipeList[p.i][p.j + 1].seen = true;
                            qu.Enqueue(pipeList[p.i][p.j + 1]);
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
                if (p.i - 1 >= 0)
                {
                    if (pipeList[p.i - 1][p.j].up)//tedy pokud ma korektniho dolniho souseda
                    {
                        if (pipeList[p.i - 1][p.j].seen == false)
                        {
                            pipeList[p.i - 1][p.j].seen = true;
                            qu.Enqueue(pipeList[p.i - 1][p.j]);
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
            if (p.left && !(p.i == 0 && p.j == 0))//u posledniho dilku nekontroluju co je v levo
            {
                if (p.j - 1 >= 0)
                {
                    if (pipeList[p.i][p.j - 1].right)//tedy pokud ma korektniho leveho souseda
                    {
                        if (pipeList[p.i][p.j - 1].seen == false)
                        {
                            pipeList[p.i][p.j - 1].seen = true;
                            qu.Enqueue(pipeList[p.i][p.j - 1]);
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
        //generuj containers na stul
        //end tube
        GameObject cc = Instantiate(endTubePrefab);
        cc.transform.SetParent(pipesHolder.transform);
        cc.transform.localScale = new Vector3(PipeSize, PipeSize, PipeSize);
        cc.transform.localPosition = new Vector3(2 * (PipeSize - 0.05f), -0.022f, 0 * (PipeSize - 0.05f));//fuj -0.022 napevno......!!!!!!!!!!!!!!!!
        helpPipes.Add(cc);
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
                //kvuli tomu tutorialu...
                int k;
                if (NewManager.instance.InTut)
                    k = NewManager.instance.ActiveConfig.puzzles.Count;
                else
                    k = NewManager.instance.ActivePuzzleIndex;

                GameObject pipePrefab = ChoosePipe(i,j,h,w, k);

                GameObject c = Instantiate(pipePrefab);
                c.transform.SetParent(pipesHolder.transform);
                c.transform.localScale = new Vector3(PipeSize, PipeSize, PipeSize);
                c.transform.localPosition = new Vector3(-j * (PipeSize-0.05f), 0, -i * (PipeSize - 0.05f));//poradi generovani (znamenka) je upraveno aby sedely indexy (viz obr cislovani mrizky)

                PipeTile pt;
                if ((pt = c.GetComponent<PipeTile>()) == null)
                    c.gameObject.AddComponent<PipeTile>();
                pipeList[i].Add(pt);
                pt.i = i;
                pt.j = j;
                pt.seen = false;
            }
        }
        //start tube
        cc = Instantiate(startTubePrefab);
        cc.transform.SetParent(pipesHolder.transform);
        cc.transform.localScale = new Vector3(PipeSize, PipeSize, PipeSize);
        cc.transform.localPosition = new Vector3(-w * (PipeSize - 0.05f), -0.022f, -(h-1)* (PipeSize - 0.05f));
        helpPipes.Add(cc);
    }

    private GameObject ChoosePipe(int i, int j, int h, int w, int k)//podiva se do chosenListu, aby zjistil, co za trubku patri na policko [i,j]
    {
        //vypln vhodne tato policka, ostatni vypln klidne nahodne
        if (chosenList[k][i][j] == 'o')
        {
            bool u, r, d, l;
            int n = 0;
            if (i - 1 >= 0) d = chosenList[k][i - 1][j] == 'o'; else d = false; ;
            if (i + 1 < h) u = chosenList[k][i + 1][j] == 'o'; else u = false;
            if (j + 1 < w) r = chosenList[k][i][j + 1] == 'o'; else r = false;
            if (j - 1 >= 0) l = chosenList[k][i][j - 1] == 'o'; else l = false;

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
                return cross4Prefab;
            else if (n == 3)
                return cross3Prefab;
            else if ((u && d) || (r && l))
                return linePrefab;
            else if (n == 2)
                return curvePrefab;
        }
        else//vyber nahodne
        {
            int r = Random.Range(0, 4);
            switch (r)
            {
                case 0: return cross3Prefab;
                case 1: return cross4Prefab;
                case 2: return curvePrefab;
                case 3: return linePrefab;
            }
        }
        return null;//to by se nemelo stat...
    }


}
