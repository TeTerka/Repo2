using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//allows talking during an animation state
//(just put this script on the state and fill the list of sentences)

public class WelcomeSpeechBehaviour : StateMachineBehaviour {

    private GameObject subtitlesCanvas;
    private Text subtitleText;
    private float time = 0;
    private int i = 0;
    private string npcName;
    private AudioSource mouth;

    //////public bool replaceName;//ehm, to pak bude jinak
    public int timeForASentence;
    public List<Sentence> sentences = new List<Sentence>();
    public bool useSound;
    /////public List<AudioClip> audioClips = new List<AudioClip>();
    //pozn.: oba listy musi mit stejnou delku!!

    NewManager mngr;

    private void Awake()
    {
        mngr = GameObject.Find("NewManager").GetComponent<NewManager>();
        subtitlesCanvas = mngr.subtitlesCanvas;
        subtitleText = subtitlesCanvas.GetComponentInChildren<Text>();

        //if (useSound)//ale tohle je taky jeste null!!!!!!!!!!
        //{
        //    mouth = mngr.theNpc.GetComponent<AudioSource>();
        //    if(mouth==null)
        //    {
        //        useSound = false;
        //        Debug.Log("error, no AudioSpoource found on the NPC");
        //    }
        //}
        //npcName = mngr.npcName;//ehm, touhle dobou je to jeste null...
    }

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //book the subtitles canvas
        mngr.whoWantsSubtitles = stateInfo.shortNameHash;
        subtitlesCanvas.SetActive(true);

        //display first sentence
        npcName = mngr.npcName;
        if (sentences[0].replaceAlex)//******
        {
            subtitleText.text = sentences[0].text.Replace("Alex", npcName);
        }
        else
        {
            subtitleText.text = sentences[0].text;
        }
        //start the countdown
        if (useSound)
        {
            mouth = mngr.theNpc.GetComponent<AudioSource>();
            if (mouth == null)
            {
                useSound = false;
                Debug.Log("error, no AudioSource found on the NPC");
            }
            mouth.clip = sentences[0].audio;
            mouth.Play();
        }
        time = 0;
        i = 1;
    }
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        //update the countdown, display apropriate sentence
        time += Time.deltaTime;
        if(i<sentences.Count && time>=timeForASentence)
        {
            if(useSound)
            {
                mouth = mngr.theNpc.GetComponent<AudioSource>();
                if (mouth == null)
                {
                    useSound = false;
                    Debug.Log("error, no AudioSource found on the NPC");
                }
                mouth.clip = sentences[i].audio;
                mouth.Play();
            }
            if (sentences[i].replaceAlex)//******
            {
                subtitleText.text = sentences[i].text.Replace("Alex", npcName);
            }
            else
            {
                subtitleText.text = sentences[i].text;
            }
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
