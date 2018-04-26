using System.Collections;
using UnityEngine;

/// <summary>
/// script for the button on the table in PipePuzzle, can end phase on click
/// </summary>
/// <remarks>object must have animator component and a renderer</remarks>
public class ButtonScript : MonoBehaviour,IInteractibleObject
{
    private bool isClicking = false;
    private Animator animator;
    private Renderer myRndr;
    private void Start()
    {
        animator = GetComponent<Animator>();
        myRndr = GetComponentInChildren<Renderer>();
    }

    public void OnTriggerPressed(ControllerScript controller)
    {
        if (!NewManager.instance.InReplayMode)
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
                Logger.instance.Log(Time.time.ToString(System.Globalization.CultureInfo.InvariantCulture) + " Press");//"player Pressed the button"

            PipePuzzle p = (PipePuzzle)NewManager.instance.CurrentPuzzle;//the casting is ok because the button is used only in PipePuzzle
            p.Check();
        }
    }

    public void OnHoverStart()
    {
        myRndr.material.color = new Color(0.7f, 1, 0.6f);
    }
    public void OnHoverEnd()
    {
        myRndr.material.color = Color.blue;
    }
}