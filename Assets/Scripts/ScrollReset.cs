using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScrollReset : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(ResetScrollPosition());
    }

    IEnumerator ResetScrollPosition()
    {
        yield return null; // 1フレーム待つ（レイアウト完了を待つ）

        ScrollRect scrollRect = GetComponent<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }
}

