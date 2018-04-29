using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// manager class for handling multiple languages for texts in minigames
/// </summary>
public class LocalizationManager : MonoBehaviour {

    /// <summary>currently used language</summary>
    public Dictionary<string, string> currentLanguage = new Dictionary<string, string>();
    /// <summary>all available languages</summary>
    private Dictionary<string, Dictionary<string, string>> allLanguages = new Dictionary<string, Dictionary<string, string>>();
    /// <summary>list of currently visible texts that shoul be localized</summary>
    public List<LocalizedText> currentLocalizedTexts = new List<LocalizedText>();

    public static LocalizationManager instance;
    [SerializeField] private Dropdown expMenuDropdown;
    [SerializeField] private Dropdown spectatorMenuDropdown;

    /// <summary>reference to english file, which is the default (allwas present)</summary>
    [SerializeField] private TextAsset english;

    private void Awake()
    {
        instance = this;
    }

    void Start () {

        //load english from the text asset
        allLanguages.Add("en", new Dictionary<string, string>());
        string[] lines = english.text.Split('\n');
        foreach(string line in lines)
        {
            if (line.Contains("="))
            {
                int eq = line.IndexOf('=');
                string key = line.Substring(0, eq);
                string value = line.Substring(eq + 1);
                if (!allLanguages.ContainsKey(key))
                    allLanguages["en"].Add(key, value);
            }
        }

        try
        {
            //find other language files in StreamingAssets\Languages folder
            foreach (string file in Directory.GetFiles(Path.Combine(Application.streamingAssetsPath,"Languages")))
            {
                if (file.EndsWith(".txt"))
                {
                    //create a dictionary for the language
                    string name = file.Substring(file.LastIndexOf('\\') + 1, file.LastIndexOf('.')-(file.LastIndexOf('\\') + 1));
                    LoadLanguageFile(name);
                    expMenuDropdown.options.Add(new Dropdown.OptionData(name));
                    spectatorMenuDropdown.options.Add(new Dropdown.OptionData(name));
                }
            }
       }
       catch(System.Exception e)
       {
           ErrorCatcher.instance.Show(e.Message);
       }

        //english is allways present, so set is as starting language
        currentLanguage = allLanguages["en"];
    }

    /// <summary>
    /// loads a new language pack from file
    /// </summary>
    /// <param name="name">language name</param>
    private void LoadLanguageFile(string name)
    {
        allLanguages.Add(name, new Dictionary<string, string>());
        if (File.Exists(Path.Combine(Path.Combine(Application.streamingAssetsPath,"Languages"),name+".txt")))
        {
            string line;
            StreamReader file = new StreamReader(Path.Combine(Path.Combine(Application.streamingAssetsPath,"Languages"), name+".txt"));
            while ((line = file.ReadLine()) != null)
            {
                if (line.Contains("="))
                {
                    int eq = line.IndexOf('=');
                    string key = line.Substring(0, eq);
                    string value = line.Substring(eq + 1);
                    if (!allLanguages.ContainsKey(key))
                        allLanguages[name].Add(key, value);
                }
            }
            file.Close();
        }                
    }

    /// <summary>
    /// action for the dropdown that changes language
    /// </summary>
    /// <param name="d"></param>
    public void ChangeLanguage(Dropdown d)
    {
        if (allLanguages.ContainsKey(d.options[d.value].text))
        {
            currentLanguage = allLanguages[d.options[d.value].text];
            foreach (LocalizedText item in currentLocalizedTexts)
            {
                item.ResetContent();
            }
        }
    }

}
