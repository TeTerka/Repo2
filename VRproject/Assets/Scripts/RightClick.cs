using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

//umoznuje rozlisit leve a prave kliknuti na button

public class RightClick : MonoBehaviour, IPointerClickHandler 
{
    public UnityEvent leftClick;
    public UnityEvent rightClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            leftClick.Invoke();
        else if (eventData.button == PointerEventData.InputButton.Right)
            rightClick.Invoke();
    }
}
