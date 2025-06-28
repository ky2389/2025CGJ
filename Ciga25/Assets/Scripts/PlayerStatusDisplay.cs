using UnityEngine;
using TMPro;

public class PlayerStatusDisplay : MonoBehaviour
{
    private TextMeshProUGUI text;
    private PlayerController player;

    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        player = FindObjectOfType<PlayerController>(); // 动态寻找场上的 Player 实例
    }

    private void Update()
    {
        if (player != null && text != null)
        {
            Vector2Int dir = player.LastDirection;
            bool pushing = player.IsPushing;
            text.text = $"[Player State] Direction: {dir}, IsPushing: {pushing}";
        }
    }
}