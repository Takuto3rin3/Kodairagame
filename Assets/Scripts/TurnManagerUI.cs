using UnityEngine;
using UnityEngine.UI;

public class TurnManagerUI : MonoBehaviour
{
    public Text turnText;

    public void UpdateTurnText(string name)
    {
        turnText.text = $"{name}'s Turn";
    }
}

