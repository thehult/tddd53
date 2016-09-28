using UnityEngine;
using System.Collections;

public class PuzzlePieceController : MonoBehaviour {

    private Vector2 originalPosition;
    private Quaternion originalRotation;
    private float difference;

    public float completeThreshold;
    public float hintThreshold;
    public float maxHintSize;
    public float hintLifetime;

    public float jumpMinTime;
    public float jumpMaxTime;
    public float jumpForce;
    private float nextJump = 0;

    private int holding = 0;

    [HideInInspector]
    public bool complete = true;

    // Use this for initialization
    void Start () {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        foreach (ParticleSystem ps in transform.GetComponentsInChildren<ParticleSystem>())
        {
            ps.startSize = 0f;
            ps.startLifetime = hintLifetime;
        }
        nextJump = Random.Range(jumpMinTime, jumpMaxTime);
    }
	
	// Update is called once per frame
	void Update () {
        if (!complete && holding <= 0 && Time.time > nextJump)
        {
            Debug.Log("JUMP!");
            GetComponent<Rigidbody2D>().AddForce(new Vector2(0, -jumpForce));
            GetComponent<Rigidbody2D>().AddTorque(Random.Range(-jumpForce, jumpForce));
            nextJump = Time.time + Random.Range(jumpMinTime, jumpMaxTime);
        }
	}

    public void pickedUp()
    {
        holding++;
    }
    
    public void moved()
    {
        if (holding > 0)
        {
            Vector2 currentPosition = transform.localPosition;
            Quaternion currentRotation = transform.localRotation;

            float positionDifference = Vector2.Distance(currentPosition, originalPosition);
            float rotationDifference = Mathf.Abs(Quaternion.Angle(currentRotation, originalRotation));

            difference = positionDifference + rotationDifference;

            float hintSize = 0;
            if (difference <= hintThreshold)
                hintSize = (maxHintSize * Mathf.Cos(difference * Mathf.PI / hintThreshold) + maxHintSize) / 2.0f;

            foreach (ParticleSystem ps in transform.GetComponentsInChildren<ParticleSystem>())
            {
                ps.startSize = hintSize;
            }
        }
    }

    public void dropped()
    {
        holding--;
        if(holding == 0)
        {
            foreach (ParticleSystem ps in transform.GetComponentsInChildren<ParticleSystem>())
            {
                ps.startSize = 0;
            }
        }
        if(difference <= completeThreshold)
        {
            complete = true;
            transform.localPosition = originalPosition;
            transform.localRotation = originalRotation;
            gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
        }
    }


}
