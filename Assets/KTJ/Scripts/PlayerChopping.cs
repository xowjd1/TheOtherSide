using UnityEngine;

public class PlayerChopping : MonoBehaviour
{
    public Animator animator;
    public string chopTrigger = "TreeChop";
    public float triggerCooldown = 0.45f;
    public bool requireAxeEquipped = true;
    public GameObject handAxe;
    public GameObject chopAxe;
    public BoxCollider axeCollider;
    public Behaviour[] movementScriptsToDisable;

    int chopTriggerHash;
    float nextTriggerTime = 0f;

    void Awake()
    {
        if (!animator) animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();
        chopTriggerHash = Animator.StringToHash(chopTrigger);

        if (chopAxe) chopAxe.SetActive(false);
        axeCollider.enabled = false;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) TryChop();
    }

    void TryChop()
    {
        if (Time.time < nextTriggerTime) return;
        if (requireAxeEquipped && handAxe && !handAxe.activeInHierarchy) return;

        animator.SetTrigger(chopTriggerHash);
        nextTriggerTime = Time.time + triggerCooldown;
    }

    public void ChopBegin()
    {
        ToggleMovement(false);
        if (handAxe) handAxe.SetActive(false);
        if (chopAxe) chopAxe.SetActive(true);
    }

    public void ChopEnd()
    {
        ToggleMovement(true);
        if (handAxe) handAxe.SetActive(true);
        if (chopAxe) chopAxe.SetActive(false);

    }

    void ToggleMovement(bool enable)
    {
        if (movementScriptsToDisable == null) return;
        foreach (var b in movementScriptsToDisable)
        {
            if (!b) continue;
            if (b == this) continue;
            if (b is Animator) continue;
            b.enabled = enable;
        }
    }

    public void AxeColOn()
    {
        axeCollider.enabled = true;
    }

    public void AxeColOff()
    {
        axeCollider.enabled = false;
    }
}