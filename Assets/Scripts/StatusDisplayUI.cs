using UnityEngine;
using System.Collections.Generic;

public class StatusDisplayUI : MonoBehaviour
{
    public Transform statusParent; // Horizontal Layout Groupを持つTransform
    public GameObject statusItemPrefab; // プレハブ

    private List<GameObject> currentItems = new List<GameObject>();

    public void UpdateStatus(List<Player> players)
    {
        // 既存UI削除
        foreach (var item in currentItems)
            Destroy(item);
        currentItems.Clear();

        // 各プレイヤーごとにUI生成
        foreach (var player in players)
        {
            GameObject obj = Instantiate(statusItemPrefab, statusParent);
            var item = obj.GetComponent<PlayerStatusItem>();
            item.SetStatus(player);
            currentItems.Add(obj);
        }
    }
}

