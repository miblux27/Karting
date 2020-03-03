using UnityEngine;

public class GamepadInput : MonoBehaviour
{
    private KartMovement kart;

    void Awake()
    {
        kart = GetComponent<KartMovement>();
    }

    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        float rt = Input.GetAxis("Right Trigger");
        float lt = Input.GetAxis("Left Trigger");
        bool jump = Input.GetButtonDown("Jump");
        bool boost = Input.GetButton("Boost");
        bool handbrake = Input.GetButton("Handbrake");

        kart.Move(h, v, rt, lt, jump, boost, handbrake);
    }
}
