// CameraFollow.cs
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float followSpeed = 10f;
    private Vector3 currentVelocity;

    void LateUpdate()
    {
        if (player != null)
        {
            Vector3 targetPos = player.position;
            targetPos.z = transform.position.z; // maintain camera Z-depth

            // Apply smooth follow
            Vector3 basePosition = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, 1f / followSpeed);

            // Get shake offset from CameraShake
            Vector3 shakeOffset = CameraShake.Instance != null ? CameraShake.Instance.GetShakeOffset() : Vector3.zero;
            float shakeRotation = CameraShake.Instance != null ? CameraShake.Instance.GetShakeRotation() : 0f;

            // Apply both follow and shake
            transform.position = basePosition + shakeOffset;
            transform.rotation = Quaternion.Euler(0, 0, shakeRotation);
        }
    }
}