using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverflowingWater : MonoBehaviour
{
    [SerializeField] private float flowSpeed = 2.0f;

    [SerializeField] private AudioClip[] waterSounds; //should play through coroutines that play the 3 sounds one after the other with  pause in between

    private Rigidbody2D waterRB;
    private AudioSource audioSource;

    private bool shouldGoUp = false;

    private void Start()
    {
        this.waterRB = this.gameObject.GetComponent<Rigidbody2D>();
        this.audioSource = this.gameObject.GetComponent<AudioSource>();
    }

    private void Update()
    {
        if(this.shouldGoUp)
            if (this.gameObject.transform.position.y >= 67.0f)
            {
                this.shouldGoUp = false;
                this.audioSource.Stop();
                this.audioSource.enabled = false;
            }
                
    }

    private void FixedUpdate()
    {
        if (this.shouldGoUp)
            this.waterRB.MovePosition(this.waterRB.position + (new Vector2(0.0f, this.flowSpeed) * Time.fixedDeltaTime));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (!this.shouldGoUp)
            {
                this.shouldGoUp = true;
                GameObject.FindObjectOfType<MusicPlayer>().PlaysceneMusic(3);

                this.StartCoroutine(this.PlayWaterSounds(0));

                //this.gameObject.GetComponentInChildren<Collider2D>().enabled = false;
            }
        }
    }

    private IEnumerator PlayWaterSounds(int clipIndex)
    {
        this.audioSource.PlayOneShot(this.waterSounds[clipIndex]);

        yield return new WaitUntil(() => this.audioSource.isPlaying == false && this.shouldGoUp == true);

        if(clipIndex == 2)
            clipIndex = 0;
        else
            clipIndex++;

        this.StartCoroutine(this.PlayWaterSounds(clipIndex));
    }
}
