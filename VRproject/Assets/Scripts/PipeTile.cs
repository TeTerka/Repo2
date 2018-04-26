﻿using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// represents a pipe in the PipePuzzle
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

    /// <summary>describes position of this pipe</summary>
    public int I { get; private set; }
    /// <summary>describes position of this pipe</summary>
    public int J { get;private set; }

    private List<Renderer> myRndr = new List<Renderer>();
    public bool blue = false;

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

    private void Start()
    {
        myRndr.AddRange(gameObject.GetComponentsInChildren<Renderer>());
    }

    public void OnTriggerPressed(ControllerScript controller)
    {
        if (!NewManager.instance.PhaseFinished)
        {
            //logging
            if (!NewManager.instance.InReplayMode)
                Logger.instance.Log(Time.time.ToString(System.Globalization.CultureInfo.InvariantCulture)+" Rotate "+I+" "+J);//"player Rotated the tile at [i,j] position 90 degrees clockwise"

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

    public void OnHoverStart()
    {
        foreach (Renderer r in myRndr)
            r.material.color = new Color(0.7f,1,0.6f);
    }
    public void OnHoverEnd()
    {
        if (blue)//aka if it is part of the highlited path
        {
            foreach (Renderer r in myRndr)
                r.material.color = new Color(0.2f, 1, 1);
        }
        else
        {
            foreach (Renderer r in myRndr)
                r.material.color = Color.white;
        }
    }

}
