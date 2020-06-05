using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The values a kart applies in its driving.
/// </summary>
[CreateAssetMenu] public class KartStats : ScriptableObject
{
    [Tooltip("The rate at which the kart changes its velocity.")]
    public float forwardSpeed = 12.5f;
    [Tooltip("The rate at which the kart changes its rotation around its local axis.")]
    public float turnSpeed = 10f;
    [Tooltip("The magnitude of the jump.")]
    public float jumpForce = 200f;
    [Tooltip("The amount of force applied when boosting.  Capped by maximum speed.")]
    public float airborneTurnSpeed = 750f;
}
