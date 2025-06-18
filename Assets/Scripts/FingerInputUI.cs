using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FingerInputUI : MonoBehaviour
{
    public Button leftButton;
    public Button rightButton;
    public Button bothButton;

    private Player currentPlayer;
    private bool isGunMode = false;

    void Start()
    {
        leftButton.onClick.AddListener(() => SetFingers(true, false));
        rightButton.onClick.AddListener(() => SetFingers(false, true));
        bothButton.onClick.AddListener(() => SetFingers(true, true));
    }

    public void SetCurrentPlayer(Player player)
    {
        currentPlayer = player;
        UpdateFingerButtons();
    }

    public void SetGunMode(bool enable)
    {
        isGunMode = enable;
        UpdateFingerButtons();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        UpdateFingerButtons(); // 必ず表示時に更新
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void SetFingers(bool left, bool right)
    {
        if (currentPlayer != null)
        {
            currentPlayer.FingersUp[0] = left;
            currentPlayer.FingersUp[1] = right;
            Debug.Log($"指の入力：左={left}, 右={right}");
            UpdateFingerButtons(); // 状態変わったら再更新
        }
    }

    public void UpdateFingerButtons()
    {
        if (currentPlayer == null) return;

        leftButton.gameObject.SetActive(true);
       rightButton.gameObject.SetActive(true);
       bothButton.gameObject.SetActive(true);

       if (isGunMode)
       {
           // Gun モード：基本は片方ずつだが、両指も選べるように
           leftButton.interactable  = !currentPlayer.FingersUp[1];
           rightButton.interactable = !currentPlayer.FingersUp[0];
           bothButton.interactable  = true;
       }
       else
       {
           // 通常モード：すべて選択可能
           leftButton.interactable  = true;
           rightButton.interactable = true;
           bothButton.interactable  = true;
       }

        // ボタンラベル更新（任意）
        leftButton.GetComponentInChildren<TextMeshProUGUI>().text = currentPlayer.FingersUp[0] ? "UP" : "DOWN";
        rightButton.GetComponentInChildren<TextMeshProUGUI>().text = currentPlayer.FingersUp[1] ? "UP" : "DOWN";
    }

    public int GetSelectedFinger()
    {
        if (currentPlayer.FingersUp[0]) return 0;
        if (currentPlayer.FingersUp[1]) return 1;
        return -1; // どちらも上がっていない場合
    }
    
    public void UpdateFingerDisplay()
{
    UpdateFingerButtons(); // 現在の指の状態に応じてUIボタン表示を更新
}

}

