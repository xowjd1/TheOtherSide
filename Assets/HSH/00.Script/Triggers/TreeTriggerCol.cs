using UnityEngine;

public class TreeTriggerCol : MonoBehaviour
{
    bool isTriggerd = false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && GameManager.Instance.status == GameStatus.TreeMission)
        {
            Debug.LogWarning("°É·Áµû!");
            isTriggerd = true;
            GameManager.Instance.UpdateGameState(GameManager.Instance.status);
        }
    }
}
