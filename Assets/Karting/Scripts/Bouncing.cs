using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bouncing : MonoBehaviour
{
    [SerializeField] private float bounceForce = 5f;
    private ContactPoint[] m_ContactPointBuffer = new ContactPoint[16];

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("ball"))
            return;

        var normalAverage = Vector3.zero;

        int contacts = collision.GetContacts(m_ContactPointBuffer);

        for (int i = 0; i < contacts; i++)
        {
            normalAverage += m_ContactPointBuffer[i].normal;
        }
        normalAverage /= contacts;

        normalAverage = -normalAverage;

        SphereCollider sphere = collision.collider.GetComponent<SphereCollider>();
        if (sphere != null)
        {
            Rigidbody rigidbody = sphere.attachedRigidbody;
            rigidbody.AddForce(normalAverage * bounceForce, ForceMode.VelocityChange);
        }
    }
}
