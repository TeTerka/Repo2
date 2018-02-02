using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

//umi vyrobit log udalosti
//a pak ho zpetne prehravat
//**zatim jen pro PipePuzzle**

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
    }

    public void Log(string s)//pokud se muze logovat, zaloguj string s do aktualniho logfilu
    {
        if (logAllowed)
        {
            if (!File.Exists(pathToLogFile))
            {
                Debug.Log("wrong file!!!");
                return;
            }
            using (StreamWriter sw = new StreamWriter(pathToLogFile, true))
            {
                sw.WriteLine(s);
                sw.Close();
            }
        }
    }

    public void ReadLog()//start replaying the steps from current logfile
    {
        text1.gameObject.SetActive(true);
        text2.gameObject.SetActive(true);

        if (!File.Exists(pathToLogFile))
        {
            Debug.Log("wrong file!!!");
            return;
        }
        string s;
        using (StreamReader sr = new StreamReader(pathToLogFile))
        {
            sr.ReadLine(); sr.ReadLine(); //prvni 2 radky nepotrebuju
            s = sr.ReadLine();
            sr.Close();
        }
        if (!float.TryParse(s, out lastTime)) { Debug.Log("3 radka neni float cas"); return; }

        StartCoroutine(Reading());       
    }

    IEnumerator Reading()//postupne cte radky logu, vola na ne simulate a pocita dobu cekani mezi dvema akcemi
    {
        actionTexts = new List<string> {"","",""};
        logText.text = "";

        if(!File.Exists(pathToLogFile))
        {
            Debug.Log("wrong file!!!");
            yield break;
        }

        using (StreamReader sr = new StreamReader(pathToLogFile))
        {
            for (int i = 0; i < 3; i++)//prvni 3 radky nepotrebuju
            {
                if(sr.ReadLine()==null)
                {
                    Debug.Log("less than 3 line in the log file!!!");
                    yield break;
                }
            }
            string action;
            while ((action = sr.ReadLine()) != null)
            {
                //vypocet casu
                string[] atoms = action.Split(' ');
                float thisTime;
                if (!float.TryParse(atoms[0], out thisTime)) { Debug.Log("prvni atom neni float cas"); yield break; }
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
            Debug.Log("spatny tvar akce " + Time.time);
            return;
        }
        switch (atoms[1])//na pozici 1 je nazev akce
        {
            case "Rotate": int i=0; int j=0;
                if (atoms.Length==4 && int.TryParse(atoms[2], out i) && int.TryParse(atoms[3], out j))
                {
                    Rotace(i, j);
                    break;
                }
                else
                {
                    Debug.Log("rotate nema spravne argumenty " + Time.time);
                    return;
                }
            case "Press": Press(); break;
            case "Next": Next();break;
            default: Debug.Log("neexistujici akce " + Time.time);break;
        }
    }

    private void Rotace(int i, int j)//simuluje rotaci dilku (v PipePuzzle)
    {
        PipePuzzle pp = (PipePuzzle)NewManager.instance.CurrentPuzzle;
        pp.pipeList[i][j].OnTriggerPressed(null);
    }
    private void Press()//simuluje stisknuti tlacitka na stole (v PipePuzzle)
    {
        PipePuzzle pp = (PipePuzzle)NewManager.instance.CurrentPuzzle;
        pp.button.GetComponentInChildren<ButtonScript>().ClickEffect();
    }
    private void Next()//simuluje koordinatorovo kliknuti na "next phase"
    {
        StartCoroutine(NewManager.instance.SwitchPhase());
    }
}
