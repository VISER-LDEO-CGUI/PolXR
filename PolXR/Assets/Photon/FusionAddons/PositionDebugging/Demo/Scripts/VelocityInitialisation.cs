using UnityEngine;
using Fusion;



/**
 * Allow to send a rb at an initial speed.
 */
class VelocityInitialisation : NetworkBehaviour
{
    Rigidbody rb;

    public enum LaunchTiming
    {
        FUN,
        Update
    }
    public LaunchTiming launchTiming = LaunchTiming.FUN;

    public bool launch = false;
    public Vector3 targetVelocity = 0.05f * Vector3.one;
    public Vector3 targetAngularVelocity = Vector3.zero;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (launchTiming == LaunchTiming.Update)
            TryLaunch();
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        if (launchTiming == LaunchTiming.FUN)
            TryLaunch();
    }

    void TryLaunch()
    {
        if (launch == false) return;
        if (Object != null && Object.HasStateAuthority == false) return;
        launch = false;
        rb.velocity = targetVelocity;
        if (targetAngularVelocity != Vector3.zero)
        {
            rb.angularVelocity = targetAngularVelocity;
        }
    }

   
}