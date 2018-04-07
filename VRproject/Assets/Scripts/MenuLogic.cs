using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;


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

    public Sprite missingImage;

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
    /// checks if <paramref name="s"/> contains whitespace characters only
    /// </summary>
    /// <param name="s">checked string</param>
    /// <returns>true = contains only whitespace characters</returns>
    public bool ContainsWhitespaceOnly(string s)
    {
        foreach (char c in s)
        {
            if (!char.IsWhiteSpace(c))
            {
                return false;
            }
        }
        return true;
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

