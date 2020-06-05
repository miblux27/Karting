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

    private CapsuleCollider m_Capsule;

    private bool m_Grounded = true;
    private bool m_SecondJump = true;

    private float m_Steering;
    private float m_Acceleration;
    private float m_Footbrake;
    private bool m_Jump;

    private const float k_maxHeight = 1f;

    // Required by KartAnimation script
    public float Steering => m_Steering;
    public float LocalSpeed => (Quaternion.Inverse(m_Rigidbody.rotation) * m_Rigidbody.velocity).z;
    public bool IsGrounded => m_Grounded;
    public bool HasSecondJump => m_SecondJump;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Capsule = GetComponent<CapsuleCollider>();
    }

    public void Move(float steering, float accel, float footbrake, bool jump)
    {
        m_Steering = Mathf.Clamp(steering, -1f, 1f);
        m_Acceleration = Mathf.Clamp(accel, 0f, 1f);
        m_Footbrake = Mathf.Clamp(footbrake, 0f, 1f);
        m_Jump = jump;

        CheckGrounded();

        ApplyDrive();

        ApplyJump();

        //CapHeight();
    }

    public void Reset()
    {
        m_Grounded = true;
        m_SecondJump = true;
    }

    private void CheckGrounded()
    {
        if (!m_Grounded)
        {
            m_Rigidbody.AddForce(3f * transform.up * Physics.gravity.y, ForceMode.Acceleration);
        }

        if (m_Grounded && !m_SecondJump)
        {
            m_SecondJump = true;
        }
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
            float localSpeedHelper = LocalSpeed;
            if (Mathf.Abs(localSpeedHelper) < 0.01f) localSpeedHelper = 0f;
            m_Rigidbody.AddTorque(transform.up * defaultStats.airborneTurnSpeed * m_Steering * Mathf.Sign(localSpeedHelper) * Time.fixedDeltaTime, ForceMode.VelocityChange);            
        }
    }

    private void ApplyJump()
    {
        if (m_Jump)
        {
            if (m_Grounded)
            {
                m_Rigidbody.AddForce(transform.up * defaultStats.jumpForce, ForceMode.Impulse);
                m_Grounded = false;
            }
            else if (m_SecondJump)
            {
                m_Rigidbody.AddForce(transform.forward * defaultStats.jumpForce, ForceMode.Impulse);
                m_SecondJump = false;
            }
        }
    }

    private void CapHeight()
    {
        if (m_Rigidbody.position.y >= k_maxHeight) {
            Vector3 velocityHelper = m_Rigidbody.velocity;
            velocityHelper.y = 0f;
            m_Rigidbody.velocity = velocityHelper;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("field"))
            m_Grounded = true;
    }
}
