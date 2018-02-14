using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

//umi vyrobit log udalosti
//a pak ho zpetne prehravat

public class Logger : MonoBehaviour {

    //sigleton stuff
    public static Logger instance;
    private void Awake()
    {
        instance = this;
    }

    //for replaying from logs
    private float lastTime;
    public Text logText;
    private List<string> actionTexts;

    //ui refs
    public Text text1;
    public Text text2;
    public Text text3;

    //for creating logs
    private string pathToLogFile;
    private bool logAllowed = false;

    //for cube puzzle log
    private PuzzleTile leftHeld = null;//*****
    private PuzzleTile rightHeld = null;//*****
    public Transform replayCubePoint;//**********************

    //to enable/disable player hands//************
    public ControllerScript leftHand;
    public ControllerScript rightHand;

    public void SetLoggerPath(string s)
    {
        pathToLogFile = s;
        if (s == null)
            logAllowed = false;
        else
            logAllowed = true;
    }

    void Start()
    {
        //deaktivuj prvky viditelne pouze v replay modu
        text1.gameObject.SetActive(false);
        text2.gameObject.SetActive(false);
        text3.gameObject.SetActive(false);
    }

    public void StopLogger()
    {
        StopAllCoroutines();
        pathToLogFile = null;
        logAllowed = false;
        text1.gameObject.SetActive(false);
        text2.gameObject.SetActive(false);
        text3.gameObject.SetActive(false);
        leftHand.enabled = true;//****************
        rightHand.enabled = true;
    }

    public void Log(string s)//pokud se muze logovat, zaloguj string s do aktualniho logfilu
    {
        if (logAllowed)
        {
            try
            {
                if (!File.Exists(pathToLogFile))
                {
                    //Debug.Log("wrong file!!!");
                    ErrorCatcher.instance.Show("Wanted to write a line to logfile but file " + pathToLogFile + " does not exist.");
                    return;
                }
                using (StreamWriter sw = new StreamWriter(pathToLogFile, true))
                {
                    sw.WriteLine(s);
                    sw.Close();
                }
            }
            catch(System.Exception e)//kvuli vecem jinym nez ze file neexistuje (pristupova prava nebo tak neco?)
            {
                ErrorCatcher.instance.Show("Wanted to write a line to logfile " + pathToLogFile + " but it threw error: " + e.ToString());
            }
        }
    }

    public void ReadLog()//start replaying the steps from current logfile
    {
        text1.gameObject.SetActive(true);
        text2.gameObject.SetActive(true);
        leftHand.enabled = false;//**************************
        rightHand.enabled = false;//je to totez jako kdyby v ControllerScriptu v Update bylo if(inReplay)return;, ale takhle contrl nemusi vedet o existenci loggeru

        try
        {
            if (!File.Exists(pathToLogFile))
            {
                //Debug.Log("wrong file!!!");
                ErrorCatcher.instance.Show("Wanted to read a logfile but file " + pathToLogFile + " does not exist");
                return;
            }
            string s;
            using (StreamReader sr = new StreamReader(pathToLogFile))
            {
                sr.ReadLine(); sr.ReadLine(); //prvni 2 radky nepotrebuju
                s = sr.ReadLine();
                sr.Close();
            }
            if (!float.TryParse(s, out lastTime))
            {
                //Debug.Log("3 radka neni float cas");
                ErrorCatcher.instance.Show("Wrong format of this logfile: " + pathToLogFile + " (3rd line is not float time)");
                return;
            }

            StartCoroutine(Reading());
        }
        catch (System.Exception e)//kvuli vecem jinym nez ze file neexistuje (pristupova prava nebo tak neco?)
        {
            ErrorCatcher.instance.Show("Wanted to read logfile " + pathToLogFile + " but it threw error: " + e.ToString());
        }
    }

    IEnumerator Reading()//postupne cte radky logu, vola na ne simulate a pocita dobu cekani mezi dvema akcemi
    {
        actionTexts = new List<string> {"","",""};
        logText.text = "";

        if(!File.Exists(pathToLogFile))
        {
            //Debug.Log("wrong file!!!");
            ErrorCatcher.instance.Show("Wanted to read a logfile but file " + pathToLogFile + " does not exist");
            yield break;
        }

        using (StreamReader sr = new StreamReader(pathToLogFile))
        {
            for (int i = 0; i < 3; i++)//prvni 3 radky nepotrebuju
            {
                if(sr.ReadLine()==null)
                {
                    //Debug.Log("less than 3 line in the log file!!!");
                    ErrorCatcher.instance.Show("Wrong format of this logfile: " + pathToLogFile + " (has less than 3 lines)");
                    yield break;
                }
            }
            string action;
            while ((action = sr.ReadLine()) != null)
            {
                //vypocet casu
                string[] atoms = action.Split(' ');
                float thisTime;
                if (!float.TryParse(atoms[0], out thisTime))
                {
                    //Debug.Log("prvni atom neni float cas");
                    ErrorCatcher.instance.Show("Wrong format of this logfile: " + pathToLogFile + " (1st word is not float time)");
                    yield break;
                }
                //jen zobrazeni - vzdy vidim tri nejnovejsi akce
                actionTexts.RemoveAt(0);
                actionTexts.Add(action.Substring(action.IndexOf(' ') + 1) + "\n");
                logText.text = actionTexts[2] + actionTexts[1] + actionTexts[0];
                //cekani
                yield return new WaitForSeconds(thisTime - lastTime);
                //provedeni akce
                lastTime = thisTime;
                Simulate(atoms);
            }

            sr.Close();
            text3.gameObject.SetActive(true);//dojelo se na konec, zviditelni label "DONE"
        }

    }

    private void Simulate(string[] atoms)//dekoduje radku z logu
    {
        if (atoms.Length < 2) 
        {
            //Debug.Log("spatny tvar akce " + Time.time);
            ErrorCatcher.instance.Show("Wrong format of this logfile: " + pathToLogFile + " (line "+atoms[0]+" is too short)");
            return;
        }
        int i = 0; int j = 0;
        float x = 0; float y = 0; float z = 0; float w = 0;
        switch (atoms[1])//na pozici 1 je nazev akce
        {
            case "Rotate": 
                if (atoms.Length==4 && int.TryParse(atoms[2], out i) && int.TryParse(atoms[3], out j))
                {
                    Rotace(i, j);
                    break;
                }
                else
                {
                    //Debug.Log("rotate nema spravne argumenty " + Time.time);
                    ErrorCatcher.instance.Show("Wrong format of this logfile: " + pathToLogFile + " (line " + atoms[0] + " has wrong Rotate arguments)");
                    return;
                }
            case "Press": Press(); break;
            case "Next": Next();break;
            case "Grab": 
                if (atoms.Length == 4 && int.TryParse(atoms[3], out i) && (atoms[2]=="left"||atoms[2]=="right"))
                {
                    Grab(i, atoms[2]);
                    break;
                }
                else
                {
                    //Debug.Log("grab nema spravne argumenty " + Time.time);
                    ErrorCatcher.instance.Show("Wrong format of this logfile: " + pathToLogFile + " (line " + atoms[0] + " has wrong Grab arguments)");
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
                    //Debug.Log("drop nema spravne argumenty " + Time.time);
                    ErrorCatcher.instance.Show("Wrong format of this logfile: " + pathToLogFile + " (line " + atoms[0] + " has wrong Drop arguments)");
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
                    //Debug.Log("place nema spravne argumenty " + Time.time);
                    ErrorCatcher.instance.Show("Wrong format of this logfile: " + pathToLogFile + " (line " + atoms[0] + " has wrong Place arguments)");
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
                    //Debug.Log("respawn nema spravne argumenty " + Time.time);
                    ErrorCatcher.instance.Show("Wrong format of this logfile: " + pathToLogFile + " (line " + atoms[0] + " has wrong Respawn arguments)");
                    return;
                }
            default:
                {
                    //Debug.Log("neexistujici akce " + Time.time);
                    ErrorCatcher.instance.Show("Wrong format of this logfile: " + pathToLogFile + " (line " + atoms[0] + " is not a valid action)");
                    break;
                }
        }
    }

    //general
    private void Next()//simuluje koordinatorovo kliknuti na "next phase"
    {
        NewManager.instance.TrySwitchPhase(true);
    }

    //pipe puzzle
    private void Rotace(int i, int j)//simuluje rotaci dilku (v PipePuzzle)
    {
        PipePuzzle pp = NewManager.instance.CurrentPuzzle as PipePuzzle;
        if(pp==null)//kontrola kdyby napr nekdo napsal Rotate do logu pro CubePuzzle...
        {
            ErrorCatcher.instance.Show("Wrong format of this logfile: " + pathToLogFile + " (Rotate is not valid action for this type of puzzle)");
            return;
        }

        if(i>=pp.PipeList.Count || j>=pp.PipeList[i].Count)        //kontrola mezi pro i a j
        {  ErrorCatcher.instance.Show("Wrong format of this logfile: " + pathToLogFile + " (illegal Rotate action)"); return; } 

        pp.PipeList[i][j].OnTriggerPressed(null);
    }
    private void Press()//simuluje stisknuti tlacitka na stole (v PipePuzzle)
    {
        PipePuzzle pp = NewManager.instance.CurrentPuzzle as PipePuzzle;
        if (pp == null)
        {
            ErrorCatcher.instance.Show("Wrong format of this logfile: " + pathToLogFile + " (Press is not valid action for this type of puzzle)");
            return;
        }
        pp.Button.GetComponentInChildren<ButtonScript>().ClickEffect();
    }

    //cube puzzle
    private void Grab(int n, string hand)
    {
        CubePuzzle cp = NewManager.instance.CurrentPuzzle as CubePuzzle;
        if (cp == null)
        {
            ErrorCatcher.instance.Show("Wrong format of this logfile: " + pathToLogFile + " (Grab is not valid action for this type of puzzle)");
            return;
        }
        foreach (GameObject cube in cp.TileList)
        {
            //najdi kostku n
            //pokud je umistena, zavolej OnCubeRemoved a tak a odumisti ji
            //posun ji o 0.8 y nahoru
            if (cube.GetComponent<PuzzleTile>().IfItIsYouSimulateGrab(n))
            {
                //prirad ji do promenne left/rightHeld
                if (hand == "left")
                {
                    if (leftHeld != null) { ErrorCatcher.instance.Show("Wrong format of this logfile: " + pathToLogFile + " (illegal Grab action)"); return; }
                    leftHeld = cube.GetComponent<PuzzleTile>();
                    cube.transform.position = replayCubePoint.position+new Vector3(-0.2f,0,0);
                }
                else
                {
                    if (rightHeld != null) { ErrorCatcher.instance.Show("Wrong format of this logfile: " + pathToLogFile + " (illegal Grab action)"); return; }
                    rightHeld = cube.GetComponent<PuzzleTile>();
                    cube.transform.position = replayCubePoint.position + new Vector3(0.2f, 0, 0);
                }
                break;
            }
        }
    }
    private void Drop(int n, string hand)//tu asi staci hand...
    {
        CubePuzzle cp = NewManager.instance.CurrentPuzzle as CubePuzzle;
        if (cp == null)
        {
            ErrorCatcher.instance.Show("Wrong format of this logfile: " + pathToLogFile + " (Drop is not valid action for this type of puzzle)");
            return;
        }
        //posun ji o 0.8 dolu
        //a odeber ji z promenne left/rightHeld
        if (hand=="left")
        {
            if (leftHeld == null) { ErrorCatcher.instance.Show("Wrong format of this logfile: " + pathToLogFile + " (illegal Drop action)"); return; }
            leftHeld.SimulateDrop();
            leftHeld = null;
        }
        else
        {
            if (rightHeld == null) { ErrorCatcher.instance.Show("Wrong format of this logfile: " + pathToLogFile + " (illegal Drop action)"); return; }
            rightHeld.SimulateDrop();
            rightHeld = null;
        }
    }
    private void Place(int m, string hand, Quaternion rot)
    {
        CubePuzzle cp = NewManager.instance.CurrentPuzzle as CubePuzzle;
        if (cp == null)
        {
            ErrorCatcher.instance.Show("Wrong format of this logfile: " + pathToLogFile + " (Place is not valid action for this type of puzzle)");
            return;
        }
        if (hand == "left")
        {
            if (leftHeld == null) { ErrorCatcher.instance.Show("Wrong format of this logfile: " + pathToLogFile + " (illegal Place action)");return; }
            leftHeld.SimulatePlaceTo(m, rot);
            leftHeld = null;
        }
        else
        {
            if (rightHeld == null) { ErrorCatcher.instance.Show("Wrong format of this logfile: " + pathToLogFile + " (illegal Place action)"); return; }
            rightHeld.SimulatePlaceTo(m, rot);
            rightHeld = null;
        }
    }
    private void Respawn(int n, string hand)
    {
        CubePuzzle cp = NewManager.instance.CurrentPuzzle as CubePuzzle;
        if (cp == null)
        {
            ErrorCatcher.instance.Show("Wrong format of this logfile: " + pathToLogFile + " (Respawn is not valid action for this type of puzzle)");
            return;
        }
        Drop(n, hand);
    }
}
