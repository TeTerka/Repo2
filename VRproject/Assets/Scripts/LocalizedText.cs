using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// should be attached on every Text UI visible for the player, changes it's text when language is changed
/// </summary>
public class LocalizedText : MonoBehaviour {

    [SerializeField] private string key;//key for this line of text to the current language dictionary

    private void Start()
    {
        LocalizationManager.instance.currentLocalizedTexts.Add(this);//add this to the list of currently visible texts
        ResetContent();
    }

    /// <summary>
    /// adjusts text on this Text UI accordingly to currently used language
    /// </summary>
    public void ResetContent()
    {
        GetComponent<Text>().text = LocalizationManager.instance.currentLanguage[key];
    }

    private void OnDestroy()
    {
        LocalizationManager.instance.currentLocalizedTexts.Remove(this);//remove this to the list of currently visible texts
    }


}
