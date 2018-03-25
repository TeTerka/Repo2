using UnityEngine;


/// <summary>
/// script for Vive controllers for obtaining input from them, must have a rigidbody component
/// </summary>

public class ControllerScript : MonoBehaviour {

    public bool isLeft;//Logger needs to know which hand is which
    public bool isFake;//for development without VR headset,......does not work anymore+++++++++++++++++++++++
    public Transform fingerTip;//the sphere defining the controller interaction range

    //getting information from Vive controllers
    private SteamVR_TrackedObject trackedObj;
    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    //cashed information from Vive controllers
    public Vector3 Velocity { get; private set; }
    public Vector3 AngularVelocity { get; private set; }

    private GragableObject heldObject;//object held by this controller
    public void StopHoldingIt()//releasing the object without the call of OnTriggerReleased
    {
        heldObject = null;
    }

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    void FixedUpdate()
    {
        //update cashed info
        if (!isFake)
        {
            Velocity = Controller.velocity;
            AngularVelocity = Controller.angularVelocity;
        }
    }

    void Update()
    {
        //check input for trigger up/down
        if (!isFake)
        {
            //try to interact with something only if this controller is not currently holding anything
            if (Controller.GetHairTriggerDown())
            {
                if (heldObject == null)
                {
                    FindInteractibleObject();
                }
            }
            //if this controller is holding something, drop it
            if (Controller.GetHairTriggerUp())
            {
                if (heldObject != null)
                {
                    ReleaseObject();
                }
            }
        }
        else//dont have vive during development+++++++++++++++++++++++++++++++++++++++++++++++
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (heldObject == null)
                {
                    FindInteractibleObject();
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (heldObject != null)
                {
                    if (heldObject != null)
                    {
                        ReleaseObject();
                    }
                }
            }
        }
    }

    /// <summary>
    /// finds an object to interact with, interacts and calls OnTriggerPressed on it
    /// </summary>
    private void FindInteractibleObject()
    {
        Collider[] touchedObjects = Physics.OverlapSphere(fingerTip.position, fingerTip.lossyScale.x / 2f);

        foreach (Collider obj in touchedObjects)
        {
            if (obj.isTrigger && obj.CompareTag("GrabableObject") )
            {
                heldObject = obj.GetComponent<GragableObject>();
                if(heldObject!=null)
                    heldObject.OnTriggerPressed(this);
                return;
            }

            if(obj.CompareTag("InteractibleObject"))
            {
                if(obj.GetComponent<IInteractibleObject>()!=null)
                    obj.GetComponent<IInteractibleObject>().OnTriggerPressed(this);
                return;
            }
        }
    }

    /// <summary>
    /// releases the held object and calls OnTriggerReleased on it
    /// </summary>
    private void ReleaseObject()
    {
        heldObject.OnTriggerReleased(this,true);
        heldObject = null;
    }

    
}
