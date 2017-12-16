using UnityEngine;

//oblast kam hrac dosahne (aby se veci mohly respawnovat, kdyz se dostanou mimo dosah hrace)

public class PlayerReachArea : MonoBehaviour {

    private void OnTriggerExit(Collider other)//pokud neco vypadne z dosahu hrace
    {
        PuzzleTile pt;
        if((pt = other.gameObject.GetComponent<PuzzleTile>())!=null)//pokud to co vypadlo z dosahu je dilek
        {
                pt.RespawnYourself();
        }
    }
}
