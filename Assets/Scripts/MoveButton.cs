using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoveButton : MonoBehaviour
{
    public MoveType moveType;
    public TextMeshProUGUI label;

    public void Init(MoveType type, System.Action<MoveType> onClick)
    {
        moveType = type;
        label.text = type.ToString();
        GetComponent<Button>().onClick.AddListener(() => onClick?.Invoke(moveType));
    }
}

