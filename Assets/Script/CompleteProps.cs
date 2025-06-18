using UnityEngine;

public class CompleteProps : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PuzzleTarget"))
        {       
            other.gameObject.GetComponent<PuzzleTarget>().puzzlePiece.ForceComplete();
        }
    }
}
