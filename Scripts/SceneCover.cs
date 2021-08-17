using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneCover : MonoBehaviour
{
    private Player player;

    [SerializeField] GameObject bushHideVFX;
    [SerializeField] AudioClip bushHideSFXIn;
    [SerializeField] AudioClip bushHideSFXOut;
    [SerializeField] GameObject rockHideVFX;
    [SerializeField] AudioClip rockHideSFX;

    private AudioSource audioSource;


    private void Start()
    {
        this.audioSource = this.gameObject.GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && this.player == null)
        {
            this.player = collision.GetComponent<Player>(); //this way we can update the reference to the player instance after a death
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && Input.GetButton("Crouch") && Mathf.Approximately(Input.GetAxis("Horizontal"), 0) && this.player.CanPlayerHide)
        {
            if(!this.player.IsBehindCover)
                this.StartCoroutine(this.SpawnHidingVFX());

            this.player.IsBehindCover = true;
            this.player.HidePlayerBehindCover();
        }
        else if(collision.CompareTag("Player") && !Input.GetButton("Crouch") && this.player.IsBehindCover)
        {
            this.PlayHidingEffects();

            this.player.IsBehindCover = false;
            this.player.UnHidePlayerBehindCover();
        }

        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && this.player.IsBehindCover)
        {
            this.player.IsBehindCover = false;
            this.player.UnHidePlayerBehindCover();
        }
    }

    private IEnumerator SpawnHidingVFX()
    {
        this.PlayHidingEffects();

        yield return new WaitUntil(() => this.player.IsBehindCover == false);
    }

    private void PlayHidingEffects()
    {
        if (this.gameObject.CompareTag("Bushes"))
        {
            Instantiate<GameObject>(this.bushHideVFX, this.player.transform.position, Quaternion.identity);

            if (!this.player.IsBehindCover)
                this.audioSource.PlayOneShot(this.bushHideSFXIn);
            else
                this.audioSource.PlayOneShot(this.bushHideSFXOut);
        }
        else if (this.gameObject.CompareTag("Stones"))
        {
            Instantiate<GameObject>(this.rockHideVFX, this.player.transform.position, Quaternion.identity);
            this.audioSource.PlayOneShot(this.rockHideSFX);
        }
    }
}
