using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpaceBall : MonoBehaviour
{
    [Header("Physics Settings")]
    public float drag = 0.3f;
    public float angularDrag = 0.4f; // for spinning motion
    public float maxSpeed = 200f;

    [Header("Ball Material")]
    public PhysicMaterial ballMaterial;

    [Header("Impact Boost")]
    public float initialBoost = 100f; // initial velocity boost when collided with ship
    public float baseImpact = 100f;      // base force (game feel)
    public float velocityScale = 4f;  // multiplier for ship speed

    public float collisionCooldown = 1f; // prevents immediate, repeated collision calculation

    private Rigidbody rb;
    private Rigidbody lastCollidedShip;
    private float lastCollisionTime = -1f;

    public enum Team { Blue, Red, None }

    public Team lastTouchedTeam = Team.None;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (ballMaterial != null && TryGetComponent(out Collider col))
        {
            col.material = ballMaterial;
        }
    }

    private void FixedUpdate()
    {
        ApplyDrag();
        ClampVelocity();
    }

    private void ApplyDrag()
    {
        rb.velocity *= 1f - (drag * Time.fixedDeltaTime);
        rb.angularVelocity *= 1f - (angularDrag * Time.fixedDeltaTime);
    }

    private void ClampVelocity()
    {
        if (rb.velocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }

    public void ResetBall(Vector3 position)
    {
        lastTouchedTeam = Team.None;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = position;
    }

    private void OnCollisionEnter(Collision col)
    {
        if (col.rigidbody != null && col.rigidbody.CompareTag("Player"))
        {
            if (col.transform.TryGetComponent<PlayerTeam>(out var team))
            {
                lastTouchedTeam = team.team;
            }

            Rigidbody shipRb = col.rigidbody;

            // Prevent repeated impulses:
            if (lastCollidedShip == shipRb && Time.time - lastCollisionTime < collisionCooldown)
            {
                return; // Skip repeated immediate collision calculation 
            }

            // Record collision
            lastCollidedShip = shipRb;
            lastCollisionTime = Time.time;

            Vector3 shipVelocity = shipRb.velocity;

            Vector3 hitDir = (transform.position - col.contacts[0].point).normalized;

            Vector3 impulse =
                hitDir * baseImpact +
                shipVelocity * velocityScale +
                hitDir * initialBoost * 10;

            rb.AddForce(impulse, ForceMode.Impulse);
        }
    }

    private void OnCollisionExit(Collision col)
    {
        // Reset so next fresh collision applies a boost again
        if (col.rigidbody != null && col.rigidbody == lastCollidedShip)
        {
            lastCollidedShip = null;
        }
    }
}
