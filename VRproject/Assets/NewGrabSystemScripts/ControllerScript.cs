using UnityEngine;

//skript na controllery - ziskavani inputu

public class ControllerScript : MonoBehaviour {

    public bool isFake;//oznaceni ze tento controller je jen testovaci 
    public Transform fingerTip;//koule kam "dosahne" controller

    //ziskavani informaci z ovladacu vive
    private SteamVR_TrackedObject trackedObj;
    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    //cashovane info z ovladacu
    public Vector3 Velocity { get; private set; }//readonly for other classes
    public Vector3 AngularVelocity { get; private set; }

    //pamatovane objekty
    private GragableObject interactingObject;//objekt ktery je ovladacem drzen
    public void StopHoldingIt()//kvuli respawnu je traba obcas neco hraci "vytrhnout z ruky", proto ostatni tridy mohou nastavit interactingObject = null
                               //just like ReleaseObject but without  interactingObject.OnTriggerReleased
    {
        interactingObject = null;
    }

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    void FixedUpdate()
    {
        if (!isFake)
        {
            Velocity = Controller.velocity;
            AngularVelocity = Controller.angularVelocity;
        }
    }

    void Update()
    {
        if(NewManager.instance.InReplayMode)//****************
        {
            return;
        }

        //check input for trigger up/down
        if (!isFake)
        {
            //pokud s nicim tento ovladac neinteraguje a stiskne se HairTrigger, zkusim sebrat kolidujici predmet
            if (Controller.GetHairTriggerDown())
            {
                if (interactingObject == null)
                {
                    FindInteractibleObject();
                }
            }
            //pokud neco drzim a pusti se HairTrigger, upustim to
            if (Controller.GetHairTriggerUp())
            {
                if (interactingObject != null)
                {
                    ReleaseObject();
                }
            }
        }
        else//dont have vive during development
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (interactingObject == null)
                {
                    FindInteractibleObject();
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (interactingObject != null)
                {
                    if (interactingObject != null)
                    {
                        ReleaseObject();
                    }
                }
            }
        }
    }

    private void FindInteractibleObject()//a zavola na nem OnTriggerPressed
    {
        Collider[] touchedObjects = Physics.OverlapSphere(fingerTip.position, fingerTip.lossyScale.x / 2f); // fingertip je mala koule na konci ovladace

        foreach (Collider obj in touchedObjects)
        {
            if (obj.isTrigger && obj.CompareTag("GrabableObject") ) //pridano isTrigger, protoze ted ma moje krychle 2 ruzne collidery...
            {
                interactingObject = obj.GetComponent<GragableObject>();
                interactingObject.OnTriggerPressed(this);
                return;
            }

            if(obj.CompareTag("PipeTile"))
            {
                obj.GetComponent<PipeTile>().OnTriggerPressed(this);
                return;
            }
        }
    }

    private void ReleaseObject()//pokud ruka neco drzi, pusti to
    {
        interactingObject.OnTriggerReleased(this,true);
        interactingObject = null;
    }

    
}
