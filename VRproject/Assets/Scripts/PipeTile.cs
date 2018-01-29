using UnityEngine;

public class PipeTile : MonoBehaviour {

    //public?
    public bool up;
    public bool right;
    public bool down;
    public bool left;

    public ParticleSystem particleUp;
    public ParticleSystem particleRight;
    public ParticleSystem particleDown;
    public ParticleSystem particleLeft;


    //hidden...ale nestaci get private set....!!!!!
    public int i;
    public int j;
    public bool seen;

    public void OnTriggerPressed(ControllerScript controller)
    {
        if (!NewManager.instance.PhaseFinished)
        {
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
