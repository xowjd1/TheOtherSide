using UnityEngine;

public class PeopleTriggerCol : MonoBehaviour
{
    bool isTriggerd = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 12)
        {
            if (GameManager.Instance.status == GameStatus.TreeMission)
            {
                Debug.LogWarning("°É·Áµû!");
                isTriggerd = true;


                GameManager.Instance.status = GameStatus.PipeMission;
                GameManager.Instance.SetCompleteUI();
                GameManager.Instance.OnMissionComplete();
            }
        }
    }
}
