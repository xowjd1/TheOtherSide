using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PipeGameInteraction : MonoBehaviour
{
    [Header("UI References")]
    public GameObject GamePanel;
    public GameObject SuccessPanel;
    public GameObject interactionPrompt;
    public TextMeshProUGUI promptText;

    [Header("Game References")]
    public PipePuzzleManager pipePuzzleManager;
    public H_CharacterMovement playerController;

    [Header("Interaction Settings")]
    public float interactionDistance = 3f;
    public KeyCode interactionKey = KeyCode.Q;

    public bool isInRange = false;
    public bool isGameActive = false;
    public GameObject currentTrigger;
    public Transform EndingPos;

    H_CamController hc;

    void Start()
    {
        // �ʱ� ���� ����
        if (GamePanel != null)
            GamePanel.SetActive(false);

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        // �÷��̾� �ڵ� ã��
        if (playerController == null)
        {
            playerController = FindFirstObjectByType<H_CharacterMovement>(FindObjectsInactive.Exclude);
        }
        

        if (promptText != null)
        {
            promptText.text = $"{interactionKey} Ű�� ���� ������ ���� ����";
        }

        hc = Camera.main.GetComponent<H_CamController>();

    }

    void Update()
    {
        // ���� ���� �ְ� QŰ�� ������
        if (isInRange && !isGameActive && Input.GetKeyDown(interactionKey))
        {
            StartPipeGame();
        }

        // ���� �� ESC�� ������
        if (isGameActive && Input.GetKeyDown(KeyCode.K))
        {
            ExitPipeGame();
        }
    }

    public void ShowPrompt()
    {
        if (interactionPrompt != null && !isGameActive)
        {
            interactionPrompt.SetActive(true);
        }
    }

    public void HidePrompt()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    public void StartPipeGame()
    {
        isGameActive = true;

        // UI Ȱ��ȭ
        if (GamePanel != null)
            GamePanel.SetActive(true);
        if (pipePuzzleManager != null)
            pipePuzzleManager.gameObject.SetActive(true);


        HidePrompt();

        // �÷��̾� ������ ��Ȱ��ȭ
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // ���콺 Ŀ�� ǥ��
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("������ ���� ����!");
    }

    public void ExitPipeGame()
    {
        isGameActive = false;

        // UI ��Ȱ��ȭ
        if (GamePanel != null)
            GamePanel.SetActive(false);
        if (pipePuzzleManager != null)
            pipePuzzleManager.gameObject.SetActive(false);


        // �÷��̾� ������ Ȱ��ȭ
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // ���콺 Ŀ�� �����
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // ���� ���� ������ ������Ʈ �ٽ� ǥ��
        if (isInRange)
        {
            ShowPrompt();
        }

        Debug.Log("������ ���� ����!");
    }

    // PipePuzzleManager���� ȣ���� �� �ִ� �޼���
    public void OnPuzzleComplete()
    {
        // ���� �Ϸ� �� ó��
        Debug.Log("���� �Ϸ�!");

        // ���� ���� �� �߰� ����
        // ...
        isGameActive = false;

        // UI ��Ȱ��ȭ
        if (SuccessPanel != null)
            SuccessPanel.SetActive(false);
        if (GamePanel != null)
            GamePanel.SetActive(false);
        if (pipePuzzleManager != null)
            pipePuzzleManager.gameObject.SetActive(false);


        // �÷��̾� ������ Ȱ��ȭ
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        hc.camStatus = CamStatus.CinematicMode;
        GameManager.Instance.status = GameStatus.Ending;
        CameraManager.Instance.MoveAndRotate(EndingPos.position, EndingPos.eulerAngles, 5);
        // ���콺 Ŀ�� �����
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        StartCoroutine(EndingSequance());

        // ��� �� �ڵ����� ���� ����
        Invoke("ExitPipeGame", 2f);
    }

    IEnumerator EndingSequance()
    {
        yield return new WaitForSeconds(5);

        GameManager.Instance.panel_Ending.SetActive(true);
    }

    //void OnDrawGizmosSelected()
    //{
    //    // ��ȣ�ۿ� ���� ǥ��
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireSphere(transform.position, interactionDistance);
    //}
}
