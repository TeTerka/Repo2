using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// can log imporatant events and replay an experiment from the logfile
/// </summary>
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
    public string PathToLogFile { get; private set; }
    private bool logAllowed = false;

    //to enable/disable player hands
    public ControllerScript leftHand;
    public ControllerScript rightHand;


    void Start()
    {
        //deaktivate things visible only in replay mode
        text1.gameObject.SetActive(false);
        text2.gameObject.SetActive(false);
        text3.gameObject.SetActive(false);
    }

    /// <summary>
    /// sets <paramref name="s"/> to be the current logger path
    /// </summary>
    /// <param name="s">path</param>
    public void SetLoggerPath(string s)
    {
        PathToLogFile = s;
        if (s == null)
            logAllowed = false;
        else
            logAllowed = true;
    }

    /// <summary>
    /// immediately stops creating or replaying the logfile
    /// </summary>
    public void StopLogger()
    {
        StopAllCoroutines();
        PathToLogFile = null;
        logAllowed = false;
        text1.gameObject.SetActive(false);
        text2.gameObject.SetActive(false);
        text3.gameObject.SetActive(false);
        leftHand.enabled = true;
        rightHand.enabled = true;
    }

    /// <summary>
    /// adds new line <paramref name="s"/> to the current logfile
    /// </summary>
    /// <param name="s">line</param>
    public void Log(string s)
    {
        if (logAllowed)
        {
            try
            {
                if (!File.Exists(PathToLogFile))
                {
                    ErrorCatcher.instance.Show("Wanted to write a line to logfile but file " + PathToLogFile + " does not exist.");
                    return;
                }
                using (StreamWriter sw = new StreamWriter(PathToLogFile, true))
                {
                    sw.WriteLine(s);
                    sw.Close();
                }
            }
            catch(System.Exception e)
            {
                ErrorCatcher.instance.Show("Wanted to write a line to logfile " + PathToLogFile + " but it threw error: " + e.ToString());
            }
        }
    }

    /// <summary>
    /// starts replaying the steps from current logfile
    /// </summary>
    public void ReadLog()
    {
        //show replay mode ui
        text1.gameObject.SetActive(true);
        text2.gameObject.SetActive(true);
        //disable controllers
        leftHand.enabled = false;
        rightHand.enabled = false;

        try
        {
            if (!File.Exists(PathToLogFile))
            {
                ErrorCatcher.instance.Show("Wanted to read a logfile but file " + PathToLogFile + " does not exist");
                return;
            }
            string s;
            using (StreamReader sr = new StreamReader(PathToLogFile))
            {
                sr.ReadLine(); sr.ReadLine(); //do not need first two lines
                s = sr.ReadLine();//use 3rd line
                sr.Close();
            }
            if (!float.TryParse(s, out lastTime))//the 3rd line should contain time when the logging started
            {
                ErrorCatcher.instance.Show("Wrong format of this logfile: " + PathToLogFile + " (3rd line is not float time)");
                return;
            }

            StartCoroutine(Reading());
        }
        catch (System.Exception e)
        {
            ErrorCatcher.instance.Show("Wanted to read logfile " + PathToLogFile + " but it threw error: " + e.ToString());
        }
    }

    /// <summary>
    /// reads the logfile line by line and simulates the actions
    /// </summary>
    /// <returns></returns>
    IEnumerator Reading()
    {
        actionTexts = new List<string> {"","",""};
        logText.text = "";

        if(!File.Exists(PathToLogFile))
        {
            ErrorCatcher.instance.Show("Wanted to read a logfile but file " + PathToLogFile + " does not exist");
            yield break;
        }

        using (StreamReader sr = new StreamReader(PathToLogFile))
        {
            for (int i = 0; i < 3; i++)//do not need first three lines
            {
                if(sr.ReadLine()==null)
                {
                    ErrorCatcher.instance.Show("Wrong format of this logfile: " + PathToLogFile + " (has less than 3 lines)");
                    yield break;
                }
            }
            string action;
            while ((action = sr.ReadLine()) != null)
            {
                //time calculation
                string[] atoms = action.Split(' ');
                float thisTime;
                if (!float.TryParse(atoms[0], out thisTime))
                {
                    ErrorCatcher.instance.Show("Wrong format of this logfile: " + PathToLogFile + " (1st word is not float time)");
                    yield break;
                }
                //show some info about logging (top three actions in queue)
                actionTexts.RemoveAt(0);
                actionTexts.Add(action.Substring(action.IndexOf(' ') + 1) + "\n");
                logText.text = actionTexts[2] + actionTexts[1] + actionTexts[0];
                //waits
                yield return new WaitForSeconds(thisTime - lastTime);
                //actualy simulates the action
                lastTime = thisTime;
                Simulate(atoms);
            }

            sr.Close();
            text3.gameObject.SetActive(true);//end of logfile, make label "DONE" visible
        }

    }

    /// <summary>
    /// decodes one line from logfile
    /// </summary>
    /// <param name="atoms">parsed line from logfile</param>
    private void Simulate(string[] atoms)
    {
        if (atoms.Length < 2) 
        {
            ErrorCatcher.instance.Show("Wrong format of this logfile: " + PathToLogFile + " (line "+atoms[0]+" is too short)");
            return;
        }
        switch (atoms[1])//there should be the name of the action
        {
            case "Next": Next();break;
            default:
                {
                    NewManager.instance.CurrentPuzzle.Simulate(atoms);
                    break;
                }
        }
    }

   /// <summary>
   /// simulate click on "Next" button
   /// </summary>
    private void Next()
    {
        NewManager.instance.TrySwitchPhase(true);
    }
}
