using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabScript : MonoBehaviour {

    public bool isFake;

    private SteamVR_TrackedObject trackedObj;
    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }
    private GameObject collidingObject;
    private GameObject objectInHand;

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    void Update()
    {
        if (!isFake)
        {
            if (Controller.GetHairTriggerDown())
            {
                if (collidingObject != null)
                {
                    GrabObject();
                }
            }

            if (Controller.GetHairTriggerUp())
            {
                if (objectInHand != null)
                {
                    ReleaseObject();
                }
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (collidingObject != null)
                {
                    GrabObject();
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (objectInHand != null)
                {
                    ReleaseObject();
                }
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        SetCollidingObject(other);
    }

    public void OnTriggerStay(Collider other)
    {
        SetCollidingObject(other);
    }

    public void OnTriggerExit(Collider other)
    {
        if (collidingObject==null)
            return;
        collidingObject = null;
    }
    private void SetCollidingObject(Collider col)
    {
        if (collidingObject != null || col.GetComponent<Collider>() == null)//pokud uz neco drzi, nebo pokud dana vec neni sebratelna
            return;
        collidingObject = col.gameObject;
    }
    private void GrabObject()
    {
        objectInHand = collidingObject;
        collidingObject = null;
        var joint = AddFixedJoint();
        joint.connectedBody = objectInHand.GetComponent<Rigidbody>();
    }

    private FixedJoint AddFixedJoint()
    {
        FixedJoint fx = gameObject.AddComponent<FixedJoint>();
        fx.breakForce = 20000;
        fx.breakTorque = 20000;
        return fx;
    }
    private void ReleaseObject()
    {
        if (GetComponent<Joint>())
        {
            GetComponent<Joint>().connectedBody = null;
            Destroy(GetComponent<Joint>());
            if (!isFake)
            {
                objectInHand.GetComponent<Rigidbody>().velocity = Controller.velocity;
                objectInHand.GetComponent<Rigidbody>().angularVelocity = Controller.angularVelocity;
            }
            else
            {

            }
        }
        objectInHand = null;
    }
}
