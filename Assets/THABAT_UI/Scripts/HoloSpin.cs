using UnityEngine;

// Rotates around Z. Use on Holo_DashRing around the orb (negative speed = reverse).
public class HoloSpin : MonoBehaviour
{
    public float degreesPerSecond = 36f;

    void Update()
    {
        transform.Rotate(0f, 0f, -degreesPerSecond * Time.unscaledDeltaTime, Space.Self);
    }
}
