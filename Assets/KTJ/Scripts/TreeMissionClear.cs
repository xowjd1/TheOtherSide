using UnityEngine;
using System.Collections;

public class TreeMissionClear : MonoBehaviour
{
    public Rigidbody treePart1;
    public Rigidbody treePart2;
    public Rigidbody handAxe;
    public BoxCollider handAxeCol;
    public PlayerChopping playerChopping;

    public float destroyDelay = 3f;

    public bool treesCleared = false;

    bool waitStarted = false;
    bool finalized = false;

    void Awake()
    {
        if (handAxeCol) handAxeCol.enabled = false;
        if (handAxe)
        {
            handAxe.isKinematic = true;
            handAxe.useGravity  = false;
        }
    }

    void Update()
    {
        if (!waitStarted && IsDropped(treePart1) && IsDropped(treePart2))
            StartCoroutine(ClearAfterDelay());
    }

    bool IsDropped(Rigidbody rb)
    {
        return rb && rb.useGravity && !rb.isKinematic;
    }

    IEnumerator ClearAfterDelay()
    {
        waitStarted = true;

        yield return new WaitForSeconds(destroyDelay);

        if (treePart1) treePart1.gameObject.SetActive(false);
        if (treePart2) treePart2.gameObject.SetActive(false);

        treesCleared = true;

        FinalizeMission();
    }

    void FinalizeMission()
    {
        if (finalized) return;
        finalized = true;

        if (playerChopping)
        {
            playerChopping.ChopEnd();
            playerChopping.enabled = false;
        }
        
        if (handAxe)
        {
            handAxe.transform.SetParent(null, true);
            handAxe.isKinematic = false;
            handAxe.useGravity  = true;
            handAxe.WakeUp();
        }
        if (handAxeCol) handAxeCol.enabled = true;
    }
}