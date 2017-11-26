using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartPuzzleBahaviour : StateMachineBehaviour {

    private GameObject subtitlesCanvas;
    private Text subtitleText;

    NewManager mngr;

    private void Awake()
    {
        mngr = GameObject.Find("NewManager").GetComponent<NewManager>();
        subtitlesCanvas = mngr.subtitlesCanvas;
        subtitleText = subtitlesCanvas.GetComponentInChildren<Text>();
    }
    
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        mngr.whoWantsSubtitles = stateInfo.shortNameHash;
        subtitlesCanvas.SetActive(true);
        subtitleText.text = "Ok, začínáme...";
    }

	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //pokud se jeste pred koncem tohoto stavu nekdo jiny zapsal ze bude pouzivat titulky, tak subtitlesCanvas nechci znicit
        if (mngr.whoWantsSubtitles == -1 || mngr.whoWantsSubtitles==stateInfo.shortNameHash)
        {
            subtitlesCanvas.SetActive(false);
            subtitleText.text = "";
            mngr.whoWantsSubtitles = -1;
        }
    }
}
