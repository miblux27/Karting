using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KartMovement : MonoBehaviour
{
    [Tooltip("The layers which represent anything the kart can collide with. This should include the ground layers.")]
    [SerializeField] private LayerMask groundLayers;

    [Tooltip("A class representing the fundamental stats required to move the kart. They are separated in this way in order to easily adjust any part of how the kart behaves.")]
    [SerializeField] private KartStats defaultStats;

    private Rigidbody m_Rigidbody;

    private SphereCollider m_Sphere;
    private CapsuleCollider m_Capsule;

    private Collider[] m_ColliderBuffer = new Collider[8];

    private RaycastHit[] m_RaycastHitBuffer = new RaycastHit[8];

    private ContactPoint[] m_ContactPointBuffer = new ContactPoint[16];

    private bool m_Grounded = true;
    private bool m_SecondJump = true;
    private bool m_HasControl = true;

    private float m_AirborneTime;

    private Vector3 m_Velocity;

    private float m_Steering;
    private float m_Slope;
    private float m_Acceleration;
    private float m_Footbrake;
    private bool m_Jump;
    private bool m_Boost;
    private bool m_Handbrake;

    private const float k_MaximumTimeSecondJump = 2f;

    // Required by KartAnimation script
    public float Steering => m_Steering;
    public float LocalSpeed => (Quaternion.Inverse(m_Rigidbody.rotation) * m_Rigidbody.velocity).z;
    public bool IsGrounded => m_Grounded;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Capsule = GetComponent<CapsuleCollider>();
        m_Sphere = GetComponent<SphereCollider>();
    }

    public void Move(float steering, float slope, float accel, float footbrake, bool jump, bool boost, bool handbrake)
    {
        if (!m_HasControl) return;

        m_Steering = Mathf.Clamp(steering, -1f, 1f);
        m_Slope = Mathf.Clamp(slope, -1f, 1f);
        m_Acceleration = Mathf.Clamp(accel, 0f, 1f);
        m_Footbrake = Mathf.Clamp(footbrake, 0f, 1f);
        m_Jump = jump;
        m_Boost = boost;
        m_Handbrake = handbrake;

        ApplyDrive();
        //CalculateSidewaysFriction();

        UpdateAirborne();
        ApplyJump();

        //ApplyBoost();

        //TurnHelper();
        //CapSpeed();
    }

    private void ApplyDrive()
    {
        m_Rigidbody.AddForce(transform.forward * defaultStats.forwardSpeed * (m_Acceleration - m_Footbrake) * Time.fixedDeltaTime, ForceMode.VelocityChange);

        if (m_Grounded)
        {
            if (m_Acceleration - m_Footbrake != 0f)
            {
                var capsuleAxis = m_Rigidbody.rotation * Vector3.forward * m_Capsule.height * 0.45f;

                var frontPoint = m_Rigidbody.position + capsuleAxis;

                m_Rigidbody.AddForceAtPosition(transform.right * defaultStats.turnSpeed * m_Steering * Mathf.Sign(LocalSpeed) * Time.fixedDeltaTime, frontPoint, ForceMode.VelocityChange);
            }
        }

        else
        {            
            m_Rigidbody.AddTorque(transform.up * defaultStats.airborneTurnSpeed * m_Steering * Time.fixedDeltaTime, ForceMode.VelocityChange);            
        }
    }

    private void CalculateSidewaysFriction()
    {
        Vector3 localVelocity = Quaternion.Inverse(m_Rigidbody.rotation) * m_Rigidbody.velocity;
        localVelocity.x = Mathf.MoveTowards(localVelocity.x, 0f, defaultStats.sidewaysFriction * Time.fixedDeltaTime);
        m_Rigidbody.velocity = m_Rigidbody.rotation * localVelocity;
    }

    private void UpdateAirborne()
    {
        if (m_Grounded)
        {
            m_SecondJump = true;
            m_AirborneTime = 0f;
        }  
        else
            m_AirborneTime += Time.fixedDeltaTime;
    }

    private void ApplyJump()
    {
        if (m_Jump)
        {
            if (m_Grounded)
            {
                m_Rigidbody.AddForce(transform.up * defaultStats.jumpForce, ForceMode.Impulse);
            }
            else if (m_SecondJump)
            {
                var force = new Vector3(m_Steering, 0f, m_Slope);

                if (force.x == 0f && force.z == 0f) force.y = -Physics.gravity.y;
                else force.y = 0.25f;

                force.Normalize();

                //m_Rigidbody.AddRelativeForce(force * defaultStats.jumpForce, ForceMode.Impulse);

                m_Rigidbody.AddForce(transform.forward * defaultStats.jumpForce * 2f, ForceMode.Impulse);

                m_SecondJump = false;
            }
        }
    }

    private void ApplyBoost()
    {
        if (m_Boost)
        {
            //m_Rigidbody.AddForce(transform.forward * defaultStats.boostForce);
        }
    }

    private void CapSpeed()
    {
        if (m_Rigidbody.velocity.magnitude > defaultStats.maxSpeed)
            m_Rigidbody.velocity = m_Rigidbody.velocity.normalized * defaultStats.maxSpeed;
    }

    private void TurnHelper()
    {
        var capsuleAxis = m_Rigidbody.rotation * Vector3.forward * m_Capsule.height * 0.5f;

        var frontPoint = m_Rigidbody.position + capsuleAxis;

        var rearPoint = m_Rigidbody.position - capsuleAxis;

        if (Physics.OverlapSphereNonAlloc(frontPoint, 0.05f, m_ColliderBuffer, groundLayers, QueryTriggerInteraction.Ignore) > 0f
            || Physics.OverlapSphereNonAlloc(rearPoint, 0.05f, m_ColliderBuffer, groundLayers, QueryTriggerInteraction.Ignore) > 0f)
        {
            if (m_Acceleration  > 0.5f || m_Footbrake > 0.5f && Mathf.Abs(Vector3.Dot(transform.up, Vector3.up)) > 0.5f)
            {
                if (LocalSpeed > 0f)
                    m_Rigidbody.AddTorque(-transform.right * defaultStats.turnHelperTorque, ForceMode.Impulse);
                else
                    m_Rigidbody.AddTorque(transform.right * defaultStats.turnHelperTorque, ForceMode.Impulse);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("field"))
            m_Grounded = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("entering by kart movement");

        var force = 1000f;

        var dir = other.transform.position - transform.position;
        dir = dir.normalized;
        other.attachedRigidbody.AddForce(dir * force);
    }

    private void OnCollisionStay(Collision collision)
    {
        /*if (!collision.collider.CompareTag("field"))
            return;

        var normalAverage = Vector3.zero;

        int contacts = collision.GetContacts(m_ContactPointBuffer);
        for (int i = 0; i < contacts; i++)
        {
            normalAverage += m_ContactPointBuffer[i].normal;
        }
        normalAverage /= contacts;

        var toRotation = Quaternion.LookRotation(transform.forward, normalAverage);
        m_Rigidbody.rotation = Quaternion.RotateTowards(m_Rigidbody.rotation, toRotation, defaultStats.corretionSpeed * Time.fixedDeltaTime);*/
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("field"))
            m_Grounded = false;
    }
}
