using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// holds data and functions neccessary for multiple menu pages
/// </summary>
public class MenuLogic: MonoBehaviour {

    //references to other menu pages
    public GameObject expMenuCanvas;
    public GameObject confMenuCanvas;
    public GameObject mainMenuCanvas;
    public GameObject chooseMenuCanvas;
    public GameObject spectatorCanvas;
    
    //available things
    public ListOfConfigurations availableConfigs = new ListOfConfigurations();
    public ListOfExperiments availableExperiments = new ListOfExperiments();

    public static MenuLogic instance;

    private void Awake()
    {
        instance = this;

        //if there is a exp list file
        //"load" it
        if (File.Exists(Application.dataPath + "/exps.xml"))
        {
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(ListOfExperiments));
                using (var stream = new FileStream(Application.dataPath + "/exps.xml", FileMode.Open))
                {
                    availableExperiments = new ListOfExperiments();
                    availableExperiments = ser.Deserialize(stream) as ListOfExperiments;
                    stream.Close();
                }
            }
            catch(System.Exception exc)
            {
                ErrorCatcher.instance.Show("Wanted to deserialize " + Application.dataPath + "/exps.xml" + " but it threw error " + exc.ToString());
                return;
            }
        }
    }

    private void OnApplicationQuit()//before quitting save the list of available experiments
    {
        if (!ErrorCatcher.instance.CatchedError)//if quitting because of an error, do not save anything
        {
            try
            {
                var ser = new XmlSerializer(typeof(ListOfExperiments));
                using (var stream = new FileStream(Application.dataPath + "/exps.xml", FileMode.Create))
                {
                    ser.Serialize(stream, availableExperiments);
                    stream.Close();
                }
            }
            catch (System.Exception exc)
            {
                ErrorCatcher.instance.Show("Wanted to serialize " + Application.dataPath + "/exps.xml" + " but it threw error " + exc.ToString());
                Application.CancelQuit();
            }
        }
    }

    /// <summary>
    /// Checks if <paramref name="s"/> is max 100 chracters long and if it contains only english letters, numbers, hyphens and underscore characters
    /// </summary>
    /// <remarks>also does not allow names forbidden as filenames in Windows: CON, PRN, AUX, NUL, COM1, COM2, COM3, COM4, COM5, COM6, COM7, COM8, COM9, LPT1, LPT2, LPT3, LPT4, LPT5, LPT6, LPT7, LPT8, LPT9</remarks>
    /// <param name="s"></param>
    /// <returns></returns>
    public bool IsValidName(string s)
    {
        if (s.Length >= 100)
            return false;

        if (s == "CON" || s == "PRN" || s == "AUX" || s == "NUL" ||
            s == "COM1" || s == "COM2" || s == "COM3" || s == "COM4" || s == "COM5" || s == "COM6" || s == "COM7" || s == "COM8" || s == "COM9" ||
            s == "LPT1" || s == "LPT2" || s == "LPT3" || s == "LPT4" || s == "LPT5" || s == "LPT6" || s == "LPT7" || s == "LPT8" || s == "LPT9")
            return false;

        return Regex.IsMatch(s,@"^[a-zA-Z0-9_\-]+$");
    }


    /// <summary>
    /// <para>loads a PNG or JPG image from disk to a Texture2D, assigns this texture to a new sprite and return its reference</para>
    /// <para>copied form https://forum.unity.com/threads/generating-sprites-dynamically-from-png-or-jpeg-files-in-c.343735/ </para>
    /// </summary>
    /// <param name="FilePath">path to image</param>
    /// <param name="PixelsPerUnit"></param>
    /// <returns>loaded sprite</returns>
    public Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 100.0f)
    {
        // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference

        Sprite NewSprite = new Sprite();
        Texture2D SpriteTexture = LoadTexture(FilePath);
        if (SpriteTexture == null)//aka if loading failed
        {
            return null;
        }
        NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), PixelsPerUnit);

        return NewSprite; 
    }

    /// <summary>
    /// <para>loads a PNG or JPG file from disk to a Texture2D, returns null if load fails</para>
    /// <para>copied form https://forum.unity.com/threads/generating-sprites-dynamically-from-png-or-jpeg-files-in-c.343735/ </para>
    /// </summary>
    /// <param name="FilePath">path to image</param>
    /// <returns>loaded texture</returns>
    public Texture2D LoadTexture(string FilePath)
    {
        if (FilePath.EndsWith(".png") || FilePath.EndsWith(".jpg"))
        {
            // Load a PNG or JPG file from disk to a Texture2D
            // Returns null if load fails

            Texture2D Tex2D;
            byte[] FileData;

            if (File.Exists(FilePath))
            {
                FileData = File.ReadAllBytes(FilePath);
                Tex2D = new Texture2D(2, 2);           // Create new "empty" texture
                if (Tex2D.LoadImage(FileData))           // Load the imagedata into the texture (size is set automatically)
                    return Tex2D;                 // If data = readable -> return texture
            }
            return null;                     // Return null if load failed
        }
        else
        {
            return null;
        }
    }
}

