using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KunoichiPatrol : MonoBehaviour
{
    private bool hasLandingPlayed = false;
    private bool hasJumpingPlayed = false;

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Kunoichi"))
        {
            Animator anim = collision.GetComponent<Animator>();
            EnemyMovement kunoichi = collision.GetComponent<EnemyMovement>();

            if (!anim.GetBool("canJump"))
            {
                anim.SetBool("canJump", true);

                this.hasLandingPlayed = false;

                if (!this.hasJumpingPlayed)
                {
                    kunoichi.AudioSource.PlayOneShot(kunoichi.JumpingSFX);
                    this.hasJumpingPlayed = true;
                }

                kunoichi.RunSpeed = 10.0f;
            }
            else if(kunoichi.RunSpeed > 9.0f)
            {
                kunoichi.RunSpeed = 8.0f;
            }
            else if(anim.GetBool("canJump"))
            {
                anim.SetBool("canJump", false);

                if (!this.hasLandingPlayed)
                {
                    kunoichi.AudioSource.PlayOneShot(kunoichi.JumpLandingSFX);
                    this.hasLandingPlayed = true;
                }

                this.hasJumpingPlayed = false;

                kunoichi.RunSpeed = 5.0f;
            }
        }
       
    }
}
