﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//skript pro krychle
//krychle je sebratelna (dedi od GrabableObject), navic pri polozeni do mrizky specialni chovani (snap to grid +zjistuje jestli pasuje)

public class PuzzleTile : GragableObject {

    //reference
    private Transform grid;//mrizka - vzhledem k ni se urcuji smery pri snapovani, referenci ziska od Manageru
    //stav
    private TileContainer placedAt = null;
    private GameObject collidingContainer = null;
    //characteristiky
    private int tileIndex;
    private Animator animator;
    private Transform spawnedAt;//misto kde byl spawnovan - aby se tam pripane zas mohl respawnovat
    //zjistovani kolidujicich policek mrizky (= colliding containers)
    List<TileContainer> listOfContainers;

    public override void Awake()//or start?
    {
        base.Awake();

        grid = ManagerScript.instance.containersHolder.transform;
        listOfContainers = ManagerScript.instance.containerList;

        animator = GetComponent<Animator>();//prefab ho musi mit!
    }

    public override void OnTriggerReleased(ControllerScript controller,bool applyVelocity)
    {
        if (collidingContainer == null || collidingContainer.GetComponent<TileContainer>().isEmpty == false)
        {//mimo mrizku => jen normalne upustit
            //throw
            base.OnTriggerReleased(controller,true);
        }
        else
        {//nad mrizkou => snap to grid
            //just drop
            base.OnTriggerReleased(controller,false);
            //snap to grid
            SnapRotation();
            SnapPosition();
            //zpamatuje si na ktere policko je polozen
            placedAt = collidingContainer.GetComponent<TileContainer>();//containerovy prafab to musi mit!
            //zjisti jestli tam pasuje (funkce Fits pak posle tuto informaci Manageru, dilek se o to dal nezajima a tak jako tak tam zustane sedet)
            placedAt.SetMatches(transform, tileIndex);
            //stop applying forces to the cube
            Freez();//public metoda tridy GrabableObject
            //oznaci policko ze uz neni volne
            placedAt.isEmpty = false;
            //stop highlighting (uz nekoliduje, uz je placedAt)
            collidingContainer.GetComponent<TileContainer>().CancelHighlight();
            collidingContainer = null;

            ManagerScript.instance.OnCubePlaced(placedAt.Matches);//Mananger zkountroluje, jestli se timto nahodou nedokoncila cela skladacka
        }
    }
    public override void OnTriggerPressed(ControllerScript controller)
    {
        base.OnTriggerPressed(controller);
        if (placedAt != null)//pokud beru dilek z mrizky, nastav ze policko na kterem byl je nyni prazdne
        {
            ManagerScript.instance.OnCubeRemoved(placedAt.Matches);
            ClearInfoAboutPlacing();
        }
        Unfreez();//pokud beru dilek z mrizky...a nebo projistotu vzdy
    }

    public void Initialize(int index,Transform spawnPoint)//tahle funkce se vola pri vytvareni dilku (aby se nepsalo primo tileIndex = k;)
    {
        tileIndex = index;
        spawnedAt = spawnPoint;
        listOfContainers = ManagerScript.instance.containerList;
    }

    public void RespawnYourself()//respawnuje sam sebe na misto, kde byl a zacatku vytvoren
    {
        OnRespawn();
        StartCoroutine(PuzzleTileFadeOut(false));
    }

    public void DestroyYourself()
    {
        ////tohle se ale stejne resi OnDstroy, tak to tu mozna ani neni treba...
        //if (placedAt != null)//pokud je umisteny v mrizce, je treba rict, ze po respawnu uz policko obsazene neni!
        //{
        //    placedAt.isEmpty = true;
        //    placedAt.matches = false;
        //    ManagerScript.instance.matchingPieces[tileIndex] = false;
        //}
        StartCoroutine(PuzzleTileFadeOut(true));
    }

    private void ClearInfoAboutPlacing()
    {
        placedAt.isEmpty = true;
        placedAt.Matches = false;
        placedAt = null;
        collidingContainer = null;
    }

    private void OnRespawn()
    {
        if (placedAt != null)//pokud je umisteny v mrizce, je treba rict, ze po respawnu uz policko obsazene neni!
        {
            Unfreez();
            ClearInfoAboutPlacing();
        }
        if (!IsFree())//pokud je v ruce, pred respawnem by jeste mel byt upusten - ne, vlastne nemel, jen se musi nastavit ze ovladac uz nic nedrzi
        {
            //CurrentController.StopHoldingIt();//to se ted pouziva misto currentController.interactingObject = null;
            //OnTriggerReleased(CurrentController);
            OnSnatched();
        }
        collidingContainer = null;
    }

    IEnumerator PuzzleTileFadeOut(bool destroy)
    {
        animator.SetTrigger("Disappear");//play animation
        yield return new WaitForSeconds(1);
        //maybe summon particle system, or light effect or something...

        if (destroy)
        {
            Destroy(this.gameObject);
        }
        else
        {
            transform.position = spawnedAt.position;
        }

    }

    //zjistuje colliding container
    private void FixedUpdate()
    {
        if (!IsFree())//je to kvuli umistovani dilku a ty se daji umistit jedine rukou, takze me to zajima jedine kdyz je krychle drzena
        {
            //spocita ke ktremu policku to ma nejbliz (a ne dal nez na 2x hranu kostky)
            float minDist = ManagerScript.instance.tileSize*2;
            GameObject closest = null;
            foreach (TileContainer container in listOfContainers)
            {
                if (container.gameObject.GetComponent<TileContainer>().isEmpty)//musi mit TileContainer, jinak to spadne.....}ale on ho ma, to zarucuje ten manager)
                {
                    float dist = Vector3.Distance(container.gameObject.transform.position, this.transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closest = container.gameObject;
                    }
                    //minule nejblizsi policko deaktivuje
                    if (collidingContainer != null)
                    {
                        collidingContainer.GetComponent<TileContainer>().CancelHighlight();
                        collidingContainer = null;
                    }
                    //nove nejblizsi policko aktivuje
                    if (closest != null)
                    {
                        collidingContainer = closest;
                        collidingContainer.GetComponent<TileContainer>().Highlight();
                    }
                }
            }
        }
    }

    private void SnapRotation()//natoci krychli aby pasovala do mrizky a zaroven se s ni co nejmene muselo tocit
    {
        Vector3 cubeUp = GetDirectionOf(transform.up);//zjisti kam ma mirit up vektor krychle
        Vector3 cubeFw = GetDirectionOf(transform.forward);//zjisti kam ma mirit forward vektor krychle
        transform.rotation = Quaternion.LookRotation(cubeFw, cubeUp);//podle nich se otoci
    }
    private void SnapPosition()//posune krychli aby pasovala do mrizky
    {
        float offset = ManagerScript.instance.tileSize / 2;//co traba jeste +0.1, aby to neblikalo na hrane s kontainerem...?
        Vector3 newPos = new Vector3(collidingContainer.transform.position.x, collidingContainer.transform.position.y + offset, collidingContainer.transform.position.z);
        transform.position = newPos;
    }

    private Vector3 GetDirectionOf(Vector3 vect)//pouzivano ve SnapRotation, zjisti kam ma smarovat (ke kteremu svetovemu smeru ma nejbliz) vektor vect aby to pasovalo na mrizku
    {
        //porovnavam to vzhledem k mrizce
        //Vector3 worldUp = grid.up;
        //Vector3 worldFw = grid.forward;
        //Vector3 worldR = grid.right;
        //pocitam uhly, vysvetleni viz obrazek v modrem sesite...
        float alpha = Vector3.Angle(grid.up, vect);
        if (alpha < 45)
        {
            return (grid.up);
        }
        else if (alpha <= 135)
        {
            float beta = Vector3.Angle(grid.forward, new Vector3(vect.x, 0, vect.z));//cosi jako projekce do roviny xz
            if (beta < 45)
            {
                return (grid.forward);
            }
            else if (beta > 135)
            {
                return (-grid.forward);
            }
            else
            {
                float gama = Vector3.Angle(grid.right, new Vector3(vect.x, 0, vect.z));
                if (gama < 45)
                {
                    return (grid.right);
                }
                else if (gama > 135)
                {
                    return (-grid.right);
                }
            }
        }
        else
        {
            return (-grid.up);
        }

        return new Vector3();//to by se stejne stat nemelo
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        if (placedAt != null)//pokud je umisteny v mrizce, je treba rict, ze po destroyi uz policko obsazene neni
        {
            ClearInfoAboutPlacing();
        }
    }
}
