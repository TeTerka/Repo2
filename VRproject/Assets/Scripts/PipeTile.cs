using UnityEngine;


/// <summary>
/// represents a pipe in the pipe puzzle
/// </summary>
/// <remarks>
/// <para>can rotate itself 90 degrees clockwise and has refences to water particle systems</para>
/// <para>objects with this script on mus have "InteractibleObject" tag</para>
/// </remarks>
public class PipeTile : MonoBehaviour,IInteractibleObject
{
    //description of this pipe (does it have up/right/down/left tube?)
    public bool up;
    public bool right;
    public bool down;
    public bool left;

    //references to particle systems
    public ParticleSystem particleUp;
    public ParticleSystem particleRight;
    public ParticleSystem particleDown;
    public ParticleSystem particleLeft;

    //describes position of this pipe
    public int I { get; private set; }
    public int J { get;private set; }

    /// <summary>
    /// initialize this pipe, set its position to [i,j] 
    /// </summary>
    /// <param name="i">heigth position</param>
    /// <param name="j">width position</param>
    public void Initialize(int i, int j)
    {
        I = i;
        J = j;
    }

    public void OnTriggerPressed(ControllerScript controller)
    {
        if (!NewManager.instance.PhaseFinished)
        {
            //logging
            if (!NewManager.instance.InReplayMode)
                Logger.instance.Log(Time.time+" Rotate "+I+" "+J);//"player Rotated the tile at [i,j] position 90 degrees clockwise"

            //rotate model
            this.gameObject.transform.Rotate(90, 0, 0);

            //rotate info
            bool tmp = right;
            right = up;
            up = left;
            left = down;
            down = tmp;

            //rotate perticle systems
            ParticleSystem tmpp = particleRight;
            particleRight = particleUp;
            particleUp = particleLeft;
            particleLeft = particleDown;
            particleDown = tmpp;
        }
    }

}
