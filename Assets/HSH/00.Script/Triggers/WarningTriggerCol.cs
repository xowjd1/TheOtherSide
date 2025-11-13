using UnityEngine;

public class WarningTriggerCol : MonoBehaviour
{
    public bool isTriggerd = false;
    public H_CharacterMovement move;
    public float slowWalkSpeed = 1.0f;
    public float slowRunSpeed = 2.0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance.status == GameStatus.TreeMission)
            {
                Debug.LogWarning("�ɷ���!");
                isTriggerd = true;
                GameManager.Instance.SetAlarmText(GameManager.Instance.Panel_Warning);

                move.walkSpeed = slowWalkSpeed;
                move.runSpeed = slowRunSpeed;
            }
        }
    }
}
