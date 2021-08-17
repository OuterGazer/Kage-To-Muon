using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShinobiPatrol : MonoBehaviour
{
    [SerializeField] float waitingTime = 2.0f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Shinobi"))
        {
            EnemyMovement shinobi = collision.GetComponent<EnemyMovement>();

            if (shinobi.IsResumingPatrol) //avoid triggering other parts of the patrol if it is resuming patrol after pursuing player
                return;

            Animator shinobiAnim = collision.GetComponent<Animator>();

            shinobi.PatrolTriggerCounter++;
            if (shinobi.PatrolTriggerCounter > 2)
                shinobi.PatrolTriggerCounter = 1;

            this.StartCoroutine(this.LookAround(shinobi, shinobiAnim));
        }
    }

    private IEnumerator LookAround(EnemyMovement inShinobi, Animator inShinobiAnim)
    {
        if (inShinobi)// && !inShinobi.IsResumingPatrol)
        {
            inShinobi.IsIdle = true;
            inShinobiAnim.SetBool("isIdle", true);
        }

        yield return new WaitForSeconds(this.waitingTime);

        if (inShinobi)// && !inShinobi.IsResumingPatrol)
            inShinobi.FlipCharSpriteOnIdle();

        yield return new WaitForSeconds(this.waitingTime);

        if (inShinobi)// && !inShinobi.IsResumingPatrol)
        {
            if (inShinobi.PatrolTriggerCounter == 2)
                inShinobi.FlipCharSpriteOnIdle();

            inShinobi.IsIdle = false;
            inShinobiAnim.SetBool("isIdle", false);
        }
    }
}
