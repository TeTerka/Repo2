using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;

//nosic dat a funkci sdilenych mezi vsemi strankami menu

public class MenuLogic: MonoBehaviour {
   public GameObject expMenuCanvas;
   public GameObject confMenuCanvas;
   public GameObject mainMenuCanvas;
   public GameObject chooseMenuCanvas;
    public GameObject spectatorCanvas;

   public ListOfConfigurations availableConfigs = new ListOfConfigurations();
   public ListOfExperiments availableExperiments = new ListOfExperiments();

    public Sprite missingImage;


    //sigleton stuff
    public static MenuLogic instance;
    private void Awake()
    {
        if(instance!=null)
        {
            Debug.Log("Multiple MenuLogics in one scene!");
        }
        instance = this;

        //if there is a exp list file ----- musim kontrolovat aby to nespadlo atak....staci File.Exists?....asi zatim jo
        //"load" it
        if (File.Exists(Application.dataPath + "/eee.xml"))
        {
            var ser = new XmlSerializer(typeof(ListOfExperiments));
            using (var stream = new FileStream(Application.dataPath + "/eee.xml", FileMode.Open))
            {
                availableExperiments = new ListOfExperiments();
                availableExperiments = ser.Deserialize(stream) as ListOfExperiments;
            }
        }
    }

    private void OnApplicationQuit()//pri vypnuti se provede ulozeni seznamu sablon experimentu
    {
        var ser = new XmlSerializer(typeof(ListOfExperiments));
        var stream = new System.IO.FileStream(Application.dataPath+"/eee.xml", System.IO.FileMode.Create);
        ser.Serialize(stream, availableExperiments);
        stream.Close();
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////
    //https://forum.unity.com/threads/generating-sprites-dynamically-from-png-or-jpeg-files-in-c.343735/
    public Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 100.0f)
    {

        // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference

        Sprite NewSprite = new Sprite();
        Texture2D SpriteTexture = LoadTexture(FilePath);
        if (SpriteTexture == null)
        {
            return null;//kdyz load selze, vrati to null
        }
        NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), PixelsPerUnit);

        return NewSprite; 
    }

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
    ////////////////////////////////////////////////////////////////////////////////////////////////////////

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

}

//serializovane tridy.....
public class Puzzle
{
    public string pathToImage;
    public int widthpx;
    public int heigthpx;
    public string name;
}

public class Configuration
{
    public string name;
    public bool withNPC;
    public bool withTutorial;
    //nova cast
    public int timeLimit;//time limit for each puzzle, in seconds
    public string modelName;
    public string behaviourName;
    //
    [XmlArray("Puzzles")]
    [XmlArrayItem("Puzzle")]
    public List<Puzzle> puzzles = new List<Puzzle>();
}

[XmlRoot("ListOfConfigurations")]
public class ListOfConfigurations
{
    [XmlArray("Confiurations")]
    [XmlArrayItem("Configuration")]
    public List<Configuration> configs = new List<Configuration>();
}

public class Experiment
{
    public string name;
    public string resultsFile;
    [XmlArray("Confiurations")]
    [XmlArrayItem("Configuration")]
    public List<Configuration> configs = new List<Configuration>();
    [XmlArray("IDs")]
    [XmlArrayItem("id")]
    public List<string> ids = new List<string>();
}

[XmlRoot("ListOfExperiments")]
public class ListOfExperiments
{
    [XmlArray("Experiments")]
    [XmlArrayItem("Experiment")]
    public List<Experiment> experiments = new List<Experiment>();
}