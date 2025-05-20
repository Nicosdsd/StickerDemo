using UnityEngine;

[ExecuteInEditMode]
public class SimpleFollow : MonoBehaviour
{
    public Transform target; // 要跟随的目标
    private Vector3 initialOffset; // 初始时与目标的相对偏移

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (target != null)
        {
            initialOffset = transform.position - target.position;
        }
    }

    // LateUpdate is called once per frame, after all Update functions have been called.
    // This is often used for camera controls or other logic that needs to happen after object movement.
    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + initialOffset;
        }
    }
}
