using UnityEngine;


/// <summary>
/// script for Vive controllers for obtaining input from them, must have a rigidbody component
/// </summary>

public class ControllerScript : MonoBehaviour {

    /// <summary>to determin which hand is left and which is right (the Logger needs to know this)</summary>
    public bool isLeft;
    /// <summary>the sphere defining the controller interaction range(the Logger needs to know this)</summary>
    [SerializeField] private Transform fingerTip;

    //getting information from Vive controllers
    private SteamVR_TrackedObject trackedObj;
    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    /// <summary>cashed info about Vive controller velocity</summary>
    public Vector3 Velocity { get; private set; }

    /// <summary>cashed info about Vive controller angular velocity</summary>
    public Vector3 AngularVelocity { get; private set; }


    private GrabableObject heldObject;//object held by this controller

    /// <summary>
    /// releasing the object without the call of OnTriggerReleased
    /// </summary>
    public void StopHoldingIt()
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
        Velocity = Controller.velocity;
        AngularVelocity = Controller.angularVelocity;
    }

    void Update()
    {
        //check input for trigger down
        //try to interact with something only if this controller is not currently holding anything
        if (Controller.GetHairTriggerDown())
        {
            if (heldObject == null)
            {
                FindInteractibleObject();
            }
        }
        //check input for trigger up
        //if this controller is holding something, drop it
        if (Controller.GetHairTriggerUp())
        {
            if (heldObject != null)
            {
                ReleaseObject();
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
                heldObject = obj.GetComponent<GrabableObject>();
                if(heldObject!=null)
                    heldObject.OnTriggerPressed(this);
                return;
            }

            if(obj.CompareTag("InteractibleObject"))
            {
                IInteractibleObject iio = obj.GetComponent<IInteractibleObject>();
                if (iio!=null)
                    iio.OnTriggerPressed(this);
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
