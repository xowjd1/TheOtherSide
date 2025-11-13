using UnityEngine;

public class PipeGameTrigger : MonoBehaviour
{
    [Header("Visual")]
    public GameObject visualObject;  // 컴퓨터 모니터 등 시각적 오브젝트

    private PipeGameInteraction gameInteraction;
    private MeshRenderer meshRenderer;
    public Outline outline;

    void Start()
    {
        // Collider 설정
        Collider col = GetComponent<SphereCollider>();
        col.isTrigger = true;

        //// Quick Outline 컴포넌트 추가
        //outline = visualObject.GetComponent<Outline>();
        //if (outline == null)
        //    outline = visualObject.AddComponent<Outline>();

        outline.OutlineColor = Color.yellow;
        outline.OutlineWidth = 5f;
        outline.enabled = false; // 시작할 때는 비활성화

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            outline.enabled = true;

            PipeGameInteraction pi = other.GetComponent<PipeGameInteraction>();
            if(pi)
            {
                pi.isInRange = true;
                pi.currentTrigger = gameObject;
                pi.ShowPrompt();
            }
                
            
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            outline.enabled = false;

            PipeGameInteraction pi = other.GetComponent<PipeGameInteraction>();
            if (pi)
            {
                pi.isInRange = false;
                pi.currentTrigger = null;
                pi.HidePrompt();
            }
        }
    }
}