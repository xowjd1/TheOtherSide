using UnityEngine;

public class TreeChopping : MonoBehaviour
{
    public Rigidbody treePart1;
    public Rigidbody treePart2;

    public SphereCollider part1Col;
    public SphereCollider part2Col;
    
    private void Start()
    {
        treePart1.isKinematic = true;
        treePart1.useGravity = false;
        treePart2.isKinematic = true;
        treePart2.useGravity = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Axe"))
        {
            Debug.Log("HI");
            if (other.bounds.Intersects(part1Col.bounds))
            {
                DropTreePart(treePart1);
            }
            if (other.bounds.Intersects(part2Col.bounds))
            {
                DropTreePart(treePart2);
            }
        }
    }

    void DropTreePart(Rigidbody part)
    {
        part.isKinematic = false;
        part.useGravity = true;
    }
    
}
