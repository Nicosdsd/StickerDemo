using UnityEngine;

public class DestroyItem : MonoBehaviour
{
    public float destroyTime = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, destroyTime);
    }

  
}
