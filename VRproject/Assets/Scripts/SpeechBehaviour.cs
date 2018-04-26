using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// allows talking of the virtual agent during an animation state
/// </summary>
public class SpeechBehaviour : StateMachineBehaviour {

    private GameObject subtitlesCanvas;
    private Text subtitleText;
    private float time = 0;
    private int i = 0;
    private string npcName;
    private AudioSource mouth;

    /// <summary>one sentence will be dispalyed for this time (in seconds)</summary>
    [SerializeField] private int timeForASentence;
    /// <summary>list of sentences to display</summary>
    [SerializeField] private List<Sentence> sentences = new List<Sentence>();
    /// <summary>true = play also the audio files attached to each sentence</summary>
    [SerializeField] private bool useSound;


    private NewManager mngr;

    private void Awake()
    {
        mngr = GameObject.Find("NewManager").GetComponent<NewManager>();
        subtitlesCanvas = mngr.subtitlesCanvas;
        subtitleText = subtitlesCanvas.GetComponentInChildren<Text>();

        if(useSound)
        {
            foreach (Sentence s in sentences)
            {
                if (s.audio==null)
                {
                    ErrorCatcher.instance.Show("missing audio in the current behaviour");
                    return;
                }
            }
        }

    }

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        //book the subtitles canvas
        mngr.whoWantsSubtitles = stateInfo.shortNameHash;
        subtitlesCanvas.SetActive(true);

        //start the countdown
        time = 0;
        i = 1;

        //display first sentence
        if (sentences.Count > 0)
        {
            npcName = mngr.NpcName;
            if (sentences[0].replaceAlex)
            {
                subtitleText.text = LocalizationManager.instance.currentLanguage[sentences[0].text].Replace("Alex", npcName);
            }
            else
            {
                subtitleText.text = LocalizationManager.instance.currentLanguage[sentences[0].text];
            }
            //manage sound
            if (useSound)
            {
                mouth = mngr.TheNpc.GetComponent<AudioSource>();
                if (mouth == null)
                {
                    useSound = false;
                    return;
                }
                mouth.Stop();

                mouth.clip = sentences[0].audio;
                if (mouth.clip == null)
                {
                    useSound = false;
                    return;
                }
                mouth.Play();
            }
        }
    }


    public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        //update the countdown, display appropriate sentence
        time += Time.deltaTime;
        if(i<sentences.Count && time>=timeForASentence)
        {
            if (sentences[i].replaceAlex)
            {
                subtitleText.text = LocalizationManager.instance.currentLanguage[sentences[i].text].Replace("Alex", npcName);
            }
            else
            {
                subtitleText.text = LocalizationManager.instance.currentLanguage[sentences[i].text]; 
            }
            //manage sound
            if (useSound)
            {
                mouth = mngr.TheNpc.GetComponent<AudioSource>();
                if (mouth == null)
                {
                    useSound = false;
                    return;
                }
                mouth.Stop();

                mouth.clip = sentences[i].audio;
                if (mouth.clip == null)
                {
                    useSound = false;
                    return;
                }
                mouth.Play();
            }

            i++;
            time = 0;
        }
    }


    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //disable the subtitle canvas (but only if nobody declared he needs it - because of state blending, the OnStateExit of old state happens AFTER OnStateEnter of the next state)
        if (mngr.whoWantsSubtitles == -1 || mngr.whoWantsSubtitles == stateInfo.shortNameHash)
        {
            subtitlesCanvas.SetActive(false);
            subtitleText.text = "";
            mngr.whoWantsSubtitles = -1;
        }
        //stop playing the sound
        if (useSound)
        {
            mouth = mngr.TheNpc.GetComponent<AudioSource>();
            if (mouth != null)
            {
                mouth.Stop();
            }
        }

    }

}
