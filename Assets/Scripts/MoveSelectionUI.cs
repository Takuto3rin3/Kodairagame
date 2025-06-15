using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MoveSelectionUI : MonoBehaviour
{
    public Transform moveButtonContainer; // ボタン配置先
    public GameObject moveButtonPrefab;   // ボタンプレハブ
    private GameManager gameManager;

    public void Init(GameManager manager)
    {
        gameManager = manager;

        // 既存のボタンを削除
        foreach (Transform child in moveButtonContainer)
            Destroy(child.gameObject);

        // 現在のプレイヤーの技セットからボタンを生成
        var moveSet = manager.GetCurrentPlayer().MoveSet;

        foreach (var moveType in moveSet)
        {
            GameObject btn = Instantiate(moveButtonPrefab, moveButtonContainer);
            MoveButton moveButton = btn.GetComponent<MoveButton>();
            moveButton.Init(moveType, manager.OnMoveSelected);
        }

        Hide(); // 初期状態は非表示
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}

