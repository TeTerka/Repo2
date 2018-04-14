using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// script for a piece of CubePuzzle (the cube)
/// </summary>
/// <remarks>
/// must have an Animator component (for the respawn animation)
/// </remarks>
public class PuzzleTile : GrabableObject {

    //references
    private Transform grid;
    private CubePuzzle cp;
    //state
    private TileContainer placedAt = null;
    private TileContainer collidingContainer = null;
    //characteristics
    private int tileIndex;
    private Animator animator;
    private Vector3 spawnedAt;//spawnPoint belonging to this tile (so that it can be respawned there)
    //for looking for colliding containers
    private List<TileContainer> listOfContainers;


    public override void Awake()
    {
        base.Awake();

        foreach (AbstractPuzzle puzzleType in NewManager.instance.puzzleTypes)
        {
            if(puzzleType.TypeName=="CubePuzzle")
                cp = (CubePuzzle)puzzleType;
        }

        grid = cp.containersHolder.transform;
        listOfContainers = cp.ContainerList;

        animator = GetComponent<Animator>();
    }

    public override void OnTriggerReleased(ControllerScript controller,bool applyVelocity)
    {
        if (collidingContainer == null || collidingContainer.isEmpty == false)
        {//out of grid => throw it
         //throw
            //logging
            string hand; if (controller.isLeft) hand = "left"; else hand = "right";
            Logger.instance.Log(Time.time.ToString(System.Globalization.CultureInfo.InvariantCulture) + " Drop " + hand);

            base.OnTriggerReleased(controller,true);
        }
        else
        {//above grid => snap to grid
            //just drop
            base.OnTriggerReleased(controller,false);
            //snap to grid
            SnapRotation();
            SnapPosition();
            //place it
            placedAt = collidingContainer;
            placedAt.SetMatches(transform, tileIndex);
            placedAt.isEmpty = false;
            //stop applying forces to the cube
            Freez();
            //stop highlighting
            collidingContainer.CancelHighlight();
            collidingContainer = null;

            //logging
            string hand; if (controller.isLeft) hand = "left"; else hand = "right";
            Quaternion gtr = gameObject.transform.rotation;
            Logger.instance.Log(Time.time.ToString(System.Globalization.CultureInfo.InvariantCulture) + " Place " + hand + " " + placedAt.ContainerIndex + " " + gtr.x + " " + gtr.y + " " + gtr.z + " " + gtr.w);

            //CubePuzzle zkontroluje, jestli se timto nahodou nedokoncila cela skladacka
            cp.OnCubePlaced(placedAt.Matches);
        }
        PhysicsCollider.size = new Vector3(0.01f, 0.01f, 0.01f);//returns the physics collider to normal size
    }


    public override void OnTriggerPressed(ControllerScript controller)
    {
        //logging
        string hand; if (controller.isLeft) hand = "left"; else hand = "right";
        Logger.instance.Log(Time.time.ToString(System.Globalization.CultureInfo.InvariantCulture) + " Grab "+ hand + " " +tileIndex);

        base.OnTriggerPressed(controller);
        PhysicsCollider.size = new Vector3(0.005f, 0.005f, 0.005f);//shrink the physics collider for easier manipulation with the held cube
        if (placedAt != null)
        {
            ClearInfoAboutPlacing();
        }
        Unfreez();
    }

    /// <summary>
    /// inicialize info about a newly created tile
    /// </summary>
    /// <param name="index"></param>
    /// <param name="spawnPoint"></param>
    public void Initialize(int index,Vector3 spawnPoint)
    {
        tileIndex = index;
        spawnedAt = spawnPoint;
        listOfContainers = cp.ContainerList;
    }



    //-----------------------------------------------------------------
    // for respawning and destroying the tile
    //-----------------------------------------------------------------

    /// <summary>
    /// rspawns this cube to its original position
    /// </summary>
    public void RespawnYourself()
    {
        //logging
        if (CurrentController != null)//if in hand, log this as a drop, otherwise we do not need to log that
        {
            string hand; if (CurrentController.isLeft) hand = "left"; else hand = "right";
            Logger.instance.Log(Time.time.ToString(System.Globalization.CultureInfo.InvariantCulture) + " Respawn " + hand + " " + tileIndex);
        }

        OnRespawn();
        StartCoroutine(PuzzleTileFadeOut(false));
    }

    /// <summary>
    /// takes care of all special situation that should be considered before respawning
    /// </summary>
    private void OnRespawn()
    {
        if (placedAt != null)//the tile is in grid
        {
            Unfreez();
            ClearInfoAboutPlacing();
        }
        if (!IsFree())//the tile is in hand
        {
            OnSnatched();
        }
        if (collidingContainer != null)//the tile is causing highlight
        {
            collidingContainer.CancelHighlight();
            collidingContainer = null;
        }
        PhysicsCollider.size = new Vector3(0.01f, 0.01f, 0.01f);
    }

    /// <summary>
    /// playes the disappear animation, waits a bit and based on <paramref name="destroy"/> destroys or respawns the tile
    /// </summary>
    /// <param name="destroy">true = destroy, false = respawn</param>
    /// <returns></returns>
    IEnumerator PuzzleTileFadeOut(bool destroy)
    {
        animator.SetTrigger("Disappear");

        yield return new WaitForSeconds(1);

        if (destroy)
        {
            Destroy(this.gameObject);
        }
        else
        {
            transform.position = spawnedAt;
        }

    }

    /// <summary>
    /// destroy this tile
    /// </summary>
    public void DestroyYourself()
    {
        StartCoroutine(PuzzleTileFadeOut(true));
    }

    /// <summary>
    /// clears info about placement of this tile and calls OnCubeRemoved
    /// </summary>
    private void ClearInfoAboutPlacing()
    {
        placedAt.isEmpty = true;

        CubePuzzle cp = (CubePuzzle)NewManager.instance.CurrentPuzzle;
        cp.OnCubeRemoved(placedAt.Matches);

        placedAt.Matches = false;
        placedAt = null;
        collidingContainer = null;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (placedAt != null)
        {
            ClearInfoAboutPlacing();
        }
    }



    //-----------------------------------------------------------------
    // snapping to grid
    //-----------------------------------------------------------------


    private void FixedUpdate()
    {
        if (!IsFree())//look for the nearest container only for the tile in hand
        {
            //finds the nearest container, but max distance is TileSize*2
            float minDist = cp.TileSize*2;
            TileContainer closest = null;
            foreach (TileContainer container in listOfContainers)
            {
                if (container.isEmpty)
                {
                    float dist = Vector3.Distance(container.gameObject.transform.position, this.transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closest = container;
                    }
                }
            }
            //deactivate previous neares container
            if (collidingContainer != null)
            {
                collidingContainer.CancelHighlight();
                collidingContainer = null;
            }
            //activate new nearest container
            collidingContainer = closest;
            if (closest != null)
            {
                collidingContainer.Highlight();
            }
        }
    }

    /// <summary>
    /// rotates the cube so that it fits in the grid
    /// </summary>
    private void SnapRotation()
    {
        Vector3 cubeUp = GetDirectionOf(transform.up);//where should the up vector of the cube be
        Vector3 cubeFw = GetDirectionOf(transform.forward);//where should the forward vector of the cube be
        transform.rotation = Quaternion.LookRotation(cubeFw, cubeUp);//rotate it so they really are
    }
    /// <summary>
    /// moves te cube so that it fits the grid
    /// </summary>
    private void SnapPosition()
    {
        float offset = (cp.TileSize / 2)+0.01f;//+0.01f because of the container heigth
        Vector3 pos = collidingContainer.gameObject.transform.position;
        Vector3 newPos = new Vector3(pos.x, pos.y + offset, pos.z);
        transform.position = newPos;
    }

    /// <summary>
    /// chooses from main directions of grid (up, forward,...) the one closest to <paramref name="vect"/>
    /// </summary>
    /// <param name="vect"></param>
    /// <returns></returns>
    private Vector3 GetDirectionOf(Vector3 vect)
    {
        float alpha = Vector3.Angle(grid.up, vect);
        if (alpha < 45)
        {
            return (grid.up);
        }
        else if (alpha <= 135)
        {
            float beta = Vector3.Angle(grid.forward, new Vector3(vect.x, 0, vect.z));
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

        return new Vector3();//this will not happen
    }




    //-----------------------------------------------------------------
    // for simulations
    //-----------------------------------------------------------------


    /// <summary>
    /// simulates dropping this tile during logfile replay
    /// (moves the cube back to its spawn position)
    /// </summary>
    public void SimulateDrop()
    {
        this.gameObject.transform.position = spawnedAt;
        this.Unfreez();
    }


    /// <summary>
    /// simulates grabbing this tile during logfile replay if this tile has index <paramref name="index"/>
    /// </summary>
    /// <remarks>just handles possible OnCubeRemove, the visualisation of grabbing must be done in the caller</remarks>
    /// <param name="index">grabbed cube index</param>
    /// <returns></returns>
    public bool IfItIsYouSimulateGrab(int index)
    {
        if (index == tileIndex)
        {
            if ((placedAt != null) && (placedAt.Matches))
            {
                cp.OnCubeRemoved(true);
            }
            this.Freez();
            return true;
        }
        return false;
    }

    /// <summary>
    /// simulates placing this tile at <paramref name="containerIndex"/> during logfile replay
    /// </summary>
    /// <remarks>also visualizes - moves the cube to position of selected container</remarks>
    /// <param name="containerIndex">index of the used container</param>
    /// <param name="rot">rotation of the used tile</param>
    public void SimulatePlaceTo(int containerIndex, Quaternion rot)
    {
        //find the container
        TileContainer target = null;
        foreach (TileContainer cont in listOfContainers)
        {
            if (cont.ContainerIndex == containerIndex)
            {
                target = cont;
                break;
            }
        }

        //place it there
        placedAt = target;
        //snap position
        float offset = (cp.TileSize / 2) +0.01f;
        Vector3 newPos = new Vector3(target.gameObject.transform.position.x, target.gameObject.transform.position.y + offset, target.gameObject.transform.position.z);
        transform.position = newPos;
        //snap rotation
        transform.rotation = rot;

        //call stuff like OnCubePlaced
        placedAt.SetMatches(transform, tileIndex);
        placedAt.isEmpty = false;
        cp.OnCubePlaced(placedAt.Matches);
    }
}
