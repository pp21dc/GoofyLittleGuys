using UnityEngine;

public class CaptureBase : MonoBehaviour
{
    // -- Variables --
    [SerializeField] private GameObject barrier;
    private int maxTime = 10;
    private float time = 0.0f;
    private bool complete = false;

    // Update is called once per frame
    void Update()
    {
        if (complete)
        {
            //need ref for player passed along from capture menu
            CaptureLilGuy();
            //play anim
            //remove minigame
        }
        else
        {
            //need ui for timer on screen
            time += Time.deltaTime;
            if (time >= maxTime)
            {
                LostMinigame();
            }
        }
    }

    private void CaptureLilGuy()
    {
        //add lil guy to team of player
    }

    private void LostMinigame()
    {
        //send fail message on Ui?
        //play escape animation
        Destroy(this.gameObject);
    }
    
}