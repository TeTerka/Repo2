using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//pokus o tlacito...ale asi ho tam stejne nepouziju

public class ButtonScript : MonoBehaviour {

    private bool isClicking;
    Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Controller"))//also dont click if user has something in hand...or yes, but the anim may cause trouble...
        {
            if (!isClicking)
            {
                StartCoroutine(Click());
            }
        }
    }

    IEnumerator Click()
    {
        isClicking = true;
        animator.SetTrigger("click");
        yield return new WaitForSeconds(0.5f);//+mozna play sound...
        isClicking = false;
        ClickEffect();
    }

    private void ClickEffect()
    {
        //do stuff
    }
}
