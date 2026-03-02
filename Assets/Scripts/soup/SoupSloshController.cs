using UnityEngine;

public class SoupSloshController : MonoBehaviour
{
    [Header("References")]
    public Rigidbody bowlRigidbody;      // Rigidbody on BowlRoot
    public Transform bowlRoot;           // BowlRoot transform (grabbed object)

    [Header("Surface Level")]
    public float surfaceHeightLocal = 0.05f;  // local Y height of soup surface inside bowl
    public float maxVisualTiltDeg = 18f;      // clamp visual tilt (keeps it believable)

    [Header("Slosh Spring")]
    public float spring = 10f;           // strength toward target
    public float damping = 0.85f;        // 0..1 (higher = more damped)
    public float angVelInfluence = 0.8f; // how much angular velocity adds wobble
    public float linVelInfluence = 0.15f;// optional: little push from linear velocity

    // internal state: we model surface "tilt vector" (like a small gravity offset)
    private Vector3 tiltVel;
    private Vector3 tilt; // x,z tilt in bowl space

    void Reset()
    {
        bowlRoot = transform.parent;
        if (bowlRoot) bowlRigidbody = bowlRoot.GetComponentInParent<Rigidbody>();
    }

    void LateUpdate()
    {
        if (!bowlRoot) return;

        // World up (gravity opposite)
        Vector3 upW = -Physics.gravity.normalized;

        // Convert bowl angular velocity to bowl space
        Vector3 angVelW = bowlRigidbody ? bowlRigidbody.angularVelocity : Vector3.zero;
        Vector3 linVelW = bowlRigidbody ? bowlRigidbody.linearVelocity : Vector3.zero;

        // Compute a "desired tilt" in bowl space that lags behind motion
        // We push opposite of angular velocity to create slosh.
        Vector3 angPushW = Vector3.Cross(angVelW, upW) * angVelInfluence; // sideways push
        Vector3 linPushW = Vector3.ProjectOnPlane(linVelW, upW) * linVelInfluence;

        // Desired surface normal in world: mostly up, slightly offset by pushes
        Vector3 desiredNormalW = (upW + angPushW + linPushW).normalized;

        // Clamp visual tilt (keeps it from going crazy)
        float angle = Vector3.Angle(upW, desiredNormalW);
        if (angle > maxVisualTiltDeg)
        {
            desiredNormalW = Vector3.Slerp(upW, desiredNormalW, maxVisualTiltDeg / angle);
        }

        // Convert desired normal into bowl local space, then extract XZ tilt
        Vector3 desiredNormalLocal = bowlRoot.InverseTransformDirection(desiredNormalW);
        Vector3 desiredTilt = new Vector3(desiredNormalLocal.x, 0f, desiredNormalLocal.z);

        // Spring-damper toward desired tilt
        Vector3 accel = (desiredTilt - tilt) * spring;
        tiltVel = (tiltVel + accel * Time.deltaTime) * Mathf.Pow(damping, Time.deltaTime * 60f);
        tilt += tiltVel * Time.deltaTime;

        // Build a rotation for the surface: normal is (tilt.x, 1, tilt.z) in bowl local
        Vector3 surfaceNormalLocal = new Vector3(tilt.x, 1f, tilt.z).normalized;

        // Place surface at a fixed local height inside bowl, but orient to normal
        transform.localPosition = new Vector3(0f, surfaceHeightLocal, 0f);
        transform.localRotation = Quaternion.FromToRotation(Vector3.up, surfaceNormalLocal);
    }
}