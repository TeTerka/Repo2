using System.Collections;
using UnityEngine;

//tlacitko na stole pro PipePuzzle
//jeho stisknuti ukonci fazi a zkontroluje, jestli byla uspesne vyresena

public class ButtonScript : MonoBehaviour
{
    private bool isClicking = false;
    Animator animator;
    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    private void OnTriggerEnter(Collider other)
    {
        //klikne pokud se ho dotkne collider ovladace
        if (other.CompareTag("Controller")&&(!NewManager.instance.InReplayMode))
        {
            if ((!isClicking) && (!NewManager.instance.Switching))//!switching aby se na tlacitko nedalo klikat v dobe mezi dvema fazemi
            {
                StartCoroutine(Click());
            }
        }
    }

    IEnumerator Click()
    {
        isClicking = true;
        animator.SetTrigger("Click");
        //yield return new WaitForSeconds(0.5f);//+mozna play sound...
        ClickEffect();
        yield return new WaitForSeconds(1f);
        isClicking = false;
    }

    public void ClickEffect()//predacasne ukonci skladani -> konec faze + kontrola spravnosti......public jen kvuli loggeru!!!!!!!!!!!!!!
    {
        if (!NewManager.instance.PhaseFinished)
        {
            //*********logging********
            if(!NewManager.instance.InReplayMode)
                Logger.instance.Log(Time.time + " Press");//"player Pressed the button"
            //************************
            PipePuzzle p = (PipePuzzle)NewManager.instance.CurrentPuzzle;//(tohle je ok, protoze tlacitko se popuziva pouze v PipePuzzle)
            p.Check();
        }
    }
}