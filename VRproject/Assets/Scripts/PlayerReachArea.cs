using UnityEngine;

/// <summary>
/// Describes the area of the player's reach, so that objects out of his reach can be respawned, GameObject with this script on must have a Collider component
/// </summary>

public class PlayerReachArea : MonoBehaviour {

    /// <summary>
    /// Unity OnTriggerExit: Respawns PuzzleTile that fell out of reach
    /// </summary>
    /// <param name="other">object that fell out of reach</param>
    private void OnTriggerExit(Collider other)
    {
        PuzzleTile pt;
        if((pt = other.gameObject.GetComponent<PuzzleTile>())!=null)
        {
                pt.RespawnYourself();
        }
    }
}
