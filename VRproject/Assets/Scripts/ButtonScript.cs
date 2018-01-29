using System.Collections;
using UnityEngine;

//tlacitko na stole pro PipePuzzle
//jeho stisknuti ukonci fazi a zkontroluje, jestli byla uspesne vyresena

public class ButtonScript : MonoBehaviour
{
    private bool isClicking = false;
    Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Controller"))
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
        animator.SetTrigger("Click");
        yield return new WaitForSeconds(0.5f);//+mozna play sound...
        ClickEffect();
        yield return new WaitForSeconds(1f);
        isClicking = false;
    }

    private void ClickEffect()//predacasne ukonci skladani -> konec faze + kontrola spravnosti
    {
        if (!NewManager.instance.PhaseFinished)
        {
            PipePuzzle p = (PipePuzzle)NewManager.instance.CurrentPuzzle;//(tohle je ok, protoze tlacitko se popuziva pouze v PipePuzzle)
            p.Check();
        }
    }
}