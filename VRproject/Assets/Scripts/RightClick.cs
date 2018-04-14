using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/// <summary>
/// allows to distinguish right and left mouse button click on a button
/// </summary>
public class RightClick : MonoBehaviour, IPointerClickHandler 
{
    /// <summary>event for left mouse button click</summary>
    public UnityEvent leftClick;
    /// <summary>event for right mouse button click</summary>
    public UnityEvent rightClick;

    /// <summary>
    /// checks which mouse button was used to click on this and invokes apropriate event (leftClick or rightClick)
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            leftClick.Invoke();
        else if (eventData.button == PointerEventData.InputButton.Right)
            rightClick.Invoke();
    }
}
