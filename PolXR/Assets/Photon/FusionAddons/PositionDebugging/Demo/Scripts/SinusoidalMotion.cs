using Fusion;
using UnityEngine;

public class SinusoidalMotion : NetworkBehaviour
{
    public float amplitude = 1.0f; // Amplitude of the sinusoidal motion
    public float frequency = 1.0f; // Frequency of the sinusoidal motion
    public float speed = 1.0f; // Speed of motion

    private Rigidbody rb;
    private Vector3 initialPosition;
    private float time;

    public enum MovingTiming
    {
        FUN,
        Update
    }
    public MovingTiming movingTiming = MovingTiming.FUN;

    public bool launch = false;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (movingTiming == MovingTiming.Update)
            MoveObject();
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        if (movingTiming == MovingTiming.FUN)
            MoveObject();
    }

    public override void Spawned()
    {
        base.Spawned();
        initialPosition = transform.position;
        time = 0;
    }

    private void MoveObject()
    {

        if (Object != null && Object.HasStateAuthority == false) return;

        if (Object)
        { 
        time += Time.deltaTime;

        // Calculate the new position in a sinusoidal pattern
        float x = initialPosition.x;
        float y = initialPosition.y + amplitude * Mathf.Sin(frequency * time);
        float z = initialPosition.z + speed * time;

        // Calculate velocity to move towards the new position
        Vector3 targetPosition = new Vector3(x, y, z);
        Vector3 velocity = (targetPosition - transform.position) / Runner.DeltaTime;
            
        // Set the velocity of the Rigidbody
        rb.velocity = velocity;
        }
    }
}
