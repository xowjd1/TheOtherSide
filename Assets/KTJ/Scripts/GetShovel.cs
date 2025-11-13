using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GetShovel : MonoBehaviour
{
    public Outline outlineTarget;
    public string playerTag = "Player";
    public TextMeshProUGUI text;

    public GameObject shovelInPlayer; 
    public GameObject shovelInMap;  

    [SerializeField] TerrainDigger digger; 
    [SerializeField] ShovelUser shovelUser;
    private bool inRange = false;
    private bool hasShovel = false;

    void Awake()
    {
        if (!digger) digger = FindObjectOfType<TerrainDigger>();
        if (outlineTarget) outlineTarget.enabled = false;
        if (text) text.gameObject.SetActive(false);
        if (shovelInPlayer) shovelInPlayer.SetActive(false); 
    }

    void Update()
    {
        if (!inRange || hasShovel) return;

        if (Input.GetKeyDown(KeyCode.G))
            Pickup();
    }

    private void Pickup()
    {
        shovelUser?.SetHasShovel(true);
        if (digger) digger.SetHasShovel(true);

        shovelInPlayer?.SetActive(true);
        shovelInMap?.SetActive(false);

        gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other) || hasShovel) return;
        inRange = true;
        if (outlineTarget) outlineTarget.enabled = true;
        if (text)          text.gameObject.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;
        inRange = false;
        if (outlineTarget) outlineTarget.enabled = false;
        if (text)          text.gameObject.SetActive(false);
    }

    bool IsPlayer(Collider c)
    {
        return c.CompareTag(playerTag) || c.GetComponent<CharacterController>() != null;
    }
}
