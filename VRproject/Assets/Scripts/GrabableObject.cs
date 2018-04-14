using UnityEngine;


/// <summary>
/// object with this script on can be grabbed, dopped and thrown away
/// </summary>
/// <remarks>
/// object must have a rigidbody component and tag "GrabableObject" and two collider components (one has IsTrigger=true, other one false)
/// </remarks>
public class GrabableObject:MonoBehaviour{

    /// <summary>controller currently holding this object</summary>
    public ControllerScript CurrentController { get; private set; }

    /// <summary>true = this object is not held by any controller</summary>
    public bool IsFree()
    { return CurrentController == null; }

    private Rigidbody rb;
    private FixedJoint fj;

    /// <summary>the collider for collision computations</summary>
    public BoxCollider PhysicsCollider { get; private set; }

    /// <summary>the collider for detecting atemps to grab this object (IsTrigger should be set to true)</summary>
    public BoxCollider GrabCollider { get; private set; }

    /// <summary>true = this object currently cannot move (IsKinematic on its Rigidbody is set to true)</summary>
    public bool IsFrozen { get; private set; }

    public virtual void Awake()
    {
        if (!gameObject.CompareTag("GrabableObject"))
        {
            gameObject.tag = "GrabableObject";
        }
        if ((rb=GetComponent<Rigidbody>())==null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        fj = null;

        //finds the two colliders, the trigger one will be GrabCollider, the other one PhysicsCollider
        BoxCollider[] c = GetComponents<BoxCollider>();
        foreach (BoxCollider collider in c)
        {
            if (collider.isTrigger)
                GrabCollider = collider;
            else
                PhysicsCollider = collider;
        }
    }

    /// <summary>
    /// sticks this object to <paramref name="controller"/>
    /// </summary>
    /// <param name="controller">controller that pressed the trigger</param>
    public virtual void OnTriggerPressed(ControllerScript controller)
    {
        //takes care of hand to hand grab
        if (!IsFree())//aka this object is held by the other hand (the other controller)
        {
            CurrentController.StopHoldingIt();
        }

        //sticks this object to the controller
        if (fj != null)
        {
            fj.connectedBody = controller.GetComponent<Rigidbody>();
        }
        else
        {
            fj = this.gameObject.AddComponent<FixedJoint>();
            fj.breakForce = 20000;
            fj.breakTorque = 20000;
            fj.connectedBody = controller.GetComponent<Rigidbody>();
        }

        CurrentController = controller;
    }

    /// <summary>
    /// disconnects this object from <paramref name="controller"/>, either drops it or throws it
    /// </summary>
    /// <param name="controller">controller that preleased the trigger</param>
    /// <param name="applyVelocity">true = throw, flase = drop</param>
    public virtual void OnTriggerReleased(ControllerScript controller, bool applyVelocity)
    {
        CurrentController = null;
        //disconnect
        DisconnectFromController();
        //throw
        if (applyVelocity)
        {
            rb.velocity = controller.Velocity;
            rb.angularVelocity = controller.AngularVelocity;
        }
        else //drop
        {
           //nothing
        }
    }

    /// <summary>
    /// simply disconnects this object from controller, does not call OnTriggerReleased
    /// </summary>
    /// <remarks>called when player did not want to drop it, the game snatched the item out of his hands (example: the held item was respawned)</remarks>
    public virtual void OnSnatched()
    {
        CurrentController.StopHoldingIt();
        CurrentController = null;
        DisconnectFromController();
    }

    /// <summary>
    /// disconnects this from current controller
    /// </summary>
    private void DisconnectFromController()
    {
        if (fj != null)
        {
          fj.connectedBody = null;
          Destroy(fj);
        }
    }

    /// <summary>
    /// makes the object unmoveable
    /// </summary>
    public void Freez()
    {
        rb.isKinematic = true;
        IsFrozen = true;
    }

    /// <summary>
    /// makes the object moveable again
    /// </summary>
    public void Unfreez()
    {
        rb.isKinematic = false;
        IsFrozen = false;
    }

    public virtual void OnDestroy()
    {
        if (!IsFree())
        {
            CurrentController.StopHoldingIt();
        }
    }
}
