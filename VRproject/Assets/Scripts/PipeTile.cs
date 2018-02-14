using UnityEngine;

//represents a pipe in the pipe puzzle
//cat rotzte itself 90 degrees clockwise and has refences to water particle systems

public class PipeTile : MonoBehaviour {

    //public?
    public bool up;
    public bool right;
    public bool down;
    public bool left;

    //public?
    public ParticleSystem particleUp;
    public ParticleSystem particleRight;
    public ParticleSystem particleDown;
    public ParticleSystem particleLeft;

    public int I { get; private set; }
    public int J { get;private set; }

    ///////////////hidden...
    /////////////public bool seen;

    public void Initialize(int i, int j)
    {
        I = i;
        J = j;
        /////////////seen = false;
    }

    public void OnTriggerPressed(ControllerScript controller)
    {
        if (!NewManager.instance.PhaseFinished)
        {
            //*********logging********
            if (!NewManager.instance.InReplayMode)
                Logger.instance.Log(Time.time+" Rotate "+I+" "+J);//"player Rotated the tile at [i,j] position 90 degrees clockwise"
            //************************

            this.gameObject.transform.Rotate(90, 0, 0);

            bool tmp = right;
            right = up;
            up = left;
            left = down;
            down = tmp;

            ParticleSystem tmpp = particleRight;
            particleRight = particleUp;
            particleUp = particleLeft;
            particleLeft = particleDown;
            particleDown = tmpp;
        }
    }

}
