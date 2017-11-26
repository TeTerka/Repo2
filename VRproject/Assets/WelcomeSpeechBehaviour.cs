using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WelcomeSpeechBehaviour : StateMachineBehaviour {

    private GameObject subtitlesCanvas;
    private Text subtitleText;
    private float time = 0;
    private int i = 0;
    private string npcName;
    private List<string> sentences;

    NewManager mngr;

    private void Awake()
    {
        mngr = GameObject.Find("NewManager").GetComponent<NewManager>();
        subtitlesCanvas = mngr.subtitlesCanvas;
        subtitleText = subtitlesCanvas.GetComponentInChildren<Text>();
        npcName = mngr.npcName;//ehm, touhle dobou je to jeste null...
        sentences = new List<string>{
        "Dobrý den, vítám vás na dnešním experimentu.",
        "Mé jméno je "+ npcName +" a budu vás provázet jednotlivými částmi.",
        "Na pultu před sebou máte sadu krychlí. ",
        "Vašim úkolem je sestavit z nich fotografii, ",
        "jejíž finální podobu můžete vidět na obrazovce za mnou.",
        "Po úspěšném složení se obrazovka vždy změní a zobrazí další fotografii ke složení."
        };
}

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        mngr.whoWantsSubtitles = stateInfo.shortNameHash;
        npcName = mngr.npcName;
        sentences[1] = "Mé jméno je " + npcName + " a budu vás provázet jednotlivými částmi.";
        subtitlesCanvas.SetActive(true);
        subtitleText.text = sentences[0];
        time = 0;
        i = 1;
    }
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        //ale co kdyz bude prerusen???? tedy co kdyz se prepne state "v pulce vety"?
        time += Time.deltaTime;
        if(i<sentences.Count && time>=5f)
        {
            subtitleText.text = sentences[i];
            i++;
            time = 0;
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //pokud se jeste pred koncem tohoto stavu nekdo jiny zapsal ze bude pouzivat titulky, tak subtitlesCanvas nechci znicit
        if (mngr.whoWantsSubtitles == -1 || mngr.whoWantsSubtitles == stateInfo.shortNameHash)
        {
            subtitlesCanvas.SetActive(false);
            subtitleText.text = "";
            mngr.whoWantsSubtitles = -1;
        }
    }
}
