using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The values a kart applies in its driving.
/// </summary>
[CreateAssetMenu] public class KartStats : ScriptableObject
{
    [Tooltip("The maximum speed a kart can reach.  Measured in m/s.")]
    public float maxSpeed = 6f;
    [Tooltip("The rate at which the kart changes its velocity.")]
    public float forwardSpeed = 12.5f;
    [Tooltip("The rate at which the kart changes its rotation around its local axis.")]
    public float turnSpeed = 10f;
    [Tooltip("The rate at which the kart changes its rotation to settle on the ground.")]
    public float corretionSpeed = 500f;
    [Tooltip("The magnitude of the jump.")]
    public float jumpForce = 200f;
    [Tooltip("The amount of force applied when boosting.  Capped by maximum speed.")]
    public float boostForce = 750f;
    [Tooltip("The amount of torque applied to smooth movement in curved areas.")]
    public float turnHelperTorque = 25f;
    [Tooltip("The amount of friction so that the kart doesn't slide.")]
    public float sidewaysFriction = 10f;
}
