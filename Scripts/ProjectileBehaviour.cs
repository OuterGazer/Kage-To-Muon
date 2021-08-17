using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    [SerializeField] float movementSpeedX = 15.0f;
    private float movementSpeedY = 0;
    [SerializeField] GameObject parrySparks;

    private float throwRange;

    private Rigidbody2D projectileRB;

    // Start is called before the first frame update
    void Start()
    {
        this.projectileRB = this.gameObject.GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (Mathf.Approximately(this.gameObject.transform.position.x, this.throwRange + 0.1f) ||
            Mathf.Approximately(this.gameObject.transform.position.x, this.throwRange) ||
            Mathf.Approximately(this.gameObject.transform.position.x, this.throwRange - 0.1f))
        {
            this.DestroyProjectile();
        }
            
    }

    private void FixedUpdate()
    {
        this.projectileRB.MovePosition(this.projectileRB.position +
                                       new Vector2(this.movementSpeedX * Time.fixedDeltaTime, this.movementSpeedY * Time.fixedDeltaTime));
    }

    public void GetMovementDirection(float inDirection, float inThrowRange)
    {
        this.throwRange = this.gameObject.transform.position.x + (inThrowRange * inDirection);

        this.movementSpeedX *= inDirection;
        this.gameObject.transform.localScale = new Vector3(Mathf.Sign(inDirection), 
                                                           this.gameObject.transform.localScale.y,
                                                           this.gameObject.transform.localScale.z);
    }

    public void ChangeDirectionOnParry(float inDirection)
    {
        this.movementSpeedX *= inDirection;
        this.movementSpeedY = Mathf.Abs(this.movementSpeedX);
        Instantiate<GameObject>(this.parrySparks, this.gameObject.transform.position, Quaternion.identity);
    }

    public void DestroyProjectile()
    {
        GameObject.Destroy(this.gameObject);
    }
}
