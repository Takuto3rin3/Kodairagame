using UnityEngine;
using TMPro;

public class PlayerStatusItem : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI shieldText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI fingerText;

    public void SetStatus(Player player)
    {
        nameText.text = player.Name;
        shieldText.text = $"Shield: {player.Shield}";
        statusText.text = "";

        if (player.IsBurned) statusText.text += "Burn ";
        if (player.IsParalyzed) statusText.text += "Paralyze";
        if (statusText.text == "") statusText.text = "None";

        string left = player.FingersUp[0] ? "UP" : "DOWN";
        string right = player.FingersUp[1] ? "UP" : "DOWN";
        fingerText.text = $"Fingers:\nLeft={left}\nRight={right}";
    }
}

