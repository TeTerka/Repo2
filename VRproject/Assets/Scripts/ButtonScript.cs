using System.Collections;
using UnityEngine;

/// <summary>
/// script for the button on the table in PipePuzzle, can end phase on click
/// </summary>
public class ButtonScript : MonoBehaviour
{
    private bool isClicking = false;
    private Animator animator;
    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    private void OnTriggerEnter(Collider other)
    {
        //clicks when touched by a contorller
        if (other.CompareTag("Controller")&&(!NewManager.instance.InReplayMode))
        {
            if ((!isClicking) && (!NewManager.instance.Switching))
            {
                StartCoroutine(Click());
            }
        }
    }

    /// <summary>
    /// starts click animation and calls click effect
    /// </summary>
    /// <returns></returns>
    IEnumerator Click()
    {
        isClicking = true;
        animator.SetTrigger("Click");
        ClickEffect();
        yield return new WaitForSeconds(1f);
        isClicking = false;
    }

    /// <summary>
    /// prematurely ends the phase and checks correctness
    /// </summary>
    public void ClickEffect()
    {
        if (!NewManager.instance.PhaseFinished)
        {
            //logging
            if(!NewManager.instance.InReplayMode)
                Logger.instance.Log(Time.time + " Press");//"player Pressed the button"

            PipePuzzle p = (PipePuzzle)NewManager.instance.CurrentPuzzle;//the casting is ok because the button is used only in PipePuzzle
            p.Check();
        }
    }
}