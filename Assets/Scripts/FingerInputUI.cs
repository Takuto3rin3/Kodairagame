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
        leftButton.onClick.AddListener(ToggleLeft);
        rightButton.onClick.AddListener(ToggleRight);
        bothButton.onClick.AddListener(ToggleBoth);
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

    private void ToggleLeft()
    {
        if (currentPlayer == null) return;
        // 左指を反転
        currentPlayer.FingersUp[0] = !currentPlayer.FingersUp[0];
        Debug.Log($"左トグル: {currentPlayer.FingersUp[0]}");
        UpdateFingerButtons();
    }
    private void ToggleRight()
    {
        if (currentPlayer == null) return;
        // 右指を反転
        currentPlayer.FingersUp[1] = !currentPlayer.FingersUp[1];
        Debug.Log($"右トグル: {currentPlayer.FingersUp[1]}");
        UpdateFingerButtons();
    }

    private void ToggleBoth()
    {
        if (currentPlayer == null) return;
        // 両指を反転
        currentPlayer.FingersUp[0] = !currentPlayer.FingersUp[0];
        currentPlayer.FingersUp[1] = !currentPlayer.FingersUp[1];
        Debug.Log($"両トグル: 左={currentPlayer.FingersUp[0]}, 右={currentPlayer.FingersUp[1]}");
        UpdateFingerButtons();
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

