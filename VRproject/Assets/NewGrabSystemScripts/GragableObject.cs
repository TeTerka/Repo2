using UnityEngine;

//objekt s timto skriptem muze byt sebran, upusten a zahozen
//(musi mit take rigidbody a tag GrabableObject)

public class GragableObject:MonoBehaviour{

    public ControllerScript CurrentController { get; private set; }//to asi ani nemusi byt public...
    public bool IsFree()
    { return CurrentController == null; }

    Rigidbody rb;
    FixedJoint fj;

    public virtual void Awake()//or start?
    {
        //kdyby se tag zapomnel priradit v inspektoru...
        if (!gameObject.CompareTag("GrabableObject"))
        {
            gameObject.tag = "GrabableObject";
        }
        //kdyby se rigidbody zapomnelo priradit v inspektoru...
        if ((rb=GetComponent<Rigidbody>())==null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        fj = null;
    }

    public virtual void OnTriggerPressed(ControllerScript controller)
    {
        //vyres moznost prendavani z ruky do ruky
        if (!IsFree())//pokud tento sebratelny predmet zrovna drzi druha ruka
        {
            CurrentController.StopHoldingIt();//nastavi ze druha ruka nedrzi nic
            //jinak se nic nestane, nebere se to tedy jako release predmetu (zadny efekt se nevola, jen se to prepoji do druhe ruky)
            //kdybych nejaky efekt chtela, bude tu OnSwapHands()....zatim nepotrebuju...
        }

        //prilep objekt k ovladaci
        if (fj != null)//pokud uz objekt ma komponentu fixedJoint, jen presmeruj jeji cil na tento ovladac (napr davam to z ruky do ruky)
        {
            fj.connectedBody = controller.GetComponent<Rigidbody>();//controler ho musi mit!
        }
        else //pokud dilek nema fixedJoint, je treba vyrobit novy a pripojit k nemu tento ovladac
        {
            fj = this.gameObject.AddComponent<FixedJoint>();
            fj.breakForce = 20000;
            fj.breakTorque = 20000;
            fj.connectedBody = controller.GetComponent<Rigidbody>();
        }

        CurrentController = controller;
    }

    public virtual void OnTriggerReleased(ControllerScript controller, bool applyVelocity)
    {
        CurrentController = null;
        //disconnect
        DisconnectFromController();
        //throw - aplikuje silu hodu pokud to objekt vyzaduje (defaultni zpusob odhozeni predmetu)
        if (!controller.isFake && applyVelocity)//pokud mame vive controllers  a chceme, prida predmetu rychlost (jako silu a smer hodu)
        {
            rb.velocity = controller.Velocity;
            rb.angularVelocity = controller.AngularVelocity;
        }
        else //just drop
        {
           //nothing
        }
    }

    public virtual void OnSnatched()//player did not want to drop it, the game snatched the item out of his hands (example: the held item was respawned)
    {
        CurrentController.StopHoldingIt();
        CurrentController = null;
        DisconnectFromController();
    }

    private void DisconnectFromController()
    {
        //disconnect
        if (fj != null)
        {
          fj.connectedBody = null;
          Destroy(fj);//je to treba nebo ne??? asi ano, jinak totiz nefunguje hand to hand...ale nevchapu proc?!??????????????????????????????????
        }
    }

    public bool IsFrozen { get; private set; }
    public void Freez()
    {
        rb.isKinematic = true;
        IsFrozen = true;
    }
    public void Unfreez()
    {
        rb.isKinematic = false;
        IsFrozen = false;
    }
    public virtual void OnDestroy()//eh? kdyz se to jmenuje presne takhle, tak se to asi vola samo kdy je treba...
    {
        if (!IsFree())
        {
            CurrentController.StopHoldingIt();
        }
    }
}
