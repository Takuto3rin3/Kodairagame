using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GunTargetUI : MonoBehaviour
{
    public Transform container; // ボタンの親
    public GameObject pointerButtonPrefab; // プレイヤー名ボタン（Prefab）

    private Action<Player> onTargetConfirmed;

    public void Show(Player attacker, List<Player> candidates, Action<Player> onConfirmed)
    {
    if (candidates == null || candidates.Count == 0)
    {
        Debug.LogWarning("No candidates for GunTargetUI");
        return;
    }
    
        gameObject.SetActive(true);
        onTargetConfirmed = onConfirmed;

        // 既存のボタン削除
        foreach (Transform child in container)
            Destroy(child.gameObject);

        // プレイヤーごとにボタン生成
        foreach (var target in candidates)
        {
            GameObject btn = Instantiate(pointerButtonPrefab, container);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = target.Name;

            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                onTargetConfirmed?.Invoke(target);   // ← GameManagerに通知
                gameObject.SetActive(false);         // UIを閉じる
            });
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}

