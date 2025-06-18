using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MoveSetupManager : MonoBehaviour
{
    public List<Button> slotButtons;
    public Transform moveListContainer;
    public GameObject moveButtonPrefab;

   [Header("Colors")]
   public Color selectedColor = new Color(0.4f,0.8f,1f);
   public Color normalColor   = Color.white;

    private MoveType[] selectedMoves = new MoveType[8];
    private int selectedSlotIndex = -1;

   // ★ デフォルト 8 技（Heal/Cure 除く）
   private static readonly MoveType[] defaultEight =
   {
       MoveType.Straight,
       MoveType.FireKick,
       MoveType.ThunderPunch,
       MoveType.Earthquake,
       MoveType.StraightFire,
       MoveType.CrossThunder,
       MoveType.Gun,
       MoveType.Charge
   };

    void Start()
    {
        // 1. スロット初期化 ― PlayerPrefs 優先
        bool hasSaved = false;

         for (int i = 0; i < slotButtons.Count; i++)
         {
             string saved = PlayerPrefs.GetString($"SkillSlot{i}", "");
             if (!string.IsNullOrEmpty(saved) && System.Enum.TryParse(saved, out MoveType parsed))
             {
                 selectedMoves[i] = parsed;
                hasSaved = true;
             }
             else
             {
                 selectedMoves[i] = MoveType.None;
             }
         }

        // 2. 保存が無ければデフォルト 8 技を入れる
        if (!hasSaved)
        {
            for (int i = 0; i < slotButtons.Count; i++)
            {
                selectedMoves[i] = defaultEight[i];
            }
        }

        // 3. スロットのテキストと色を更新
        RefreshSlotVisuals();

         // スロットボタンにクリックイベント
         for (int i = 0; i < slotButtons.Count; i++)
         {
             int index = i;
             slotButtons[i].onClick.AddListener(() => OnSlotClicked(index));
         }

         // 技一覧生成（Heal/Cure除外）
         foreach (MoveType move in System.Enum.GetValues(typeof(MoveType)))
         {
             if (move == MoveType.None || move == MoveType.Heal || move == MoveType.Cure) continue;

             GameObject btnObj = Instantiate(moveButtonPrefab, moveListContainer);
             MoveButton moveBtn = btnObj.GetComponent<MoveButton>();
             moveBtn.Init(move, OnMoveSelected);
         }
     }

     void OnSlotClicked(int index)
     {
         selectedSlotIndex = index;
         Debug.Log($"Selected slot {index}");
     }

     void OnMoveSelected(MoveType move)
     {
         if (selectedSlotIndex < 0)
         {
             Debug.LogWarning("Please select a slot first.");
             return;
         }

         bool alreadySelected = selectedMoves
             .Where((m, i) => i != selectedSlotIndex)
             .Contains(move);

         if (alreadySelected)
         {
             Debug.LogWarning($"{move} is already selected in another slot.");
             return;
         }

         selectedMoves[selectedSlotIndex] = move;

        RefreshSlotVisuals();

         Debug.Log($"Assigned {move} to slot {selectedSlotIndex + 1}");
     }

    // スロットの文字 & 色をまとめて更新
    private void RefreshSlotVisuals()
    {
        for (int i = 0; i < slotButtons.Count; i++)
        {
            TextMeshProUGUI label = slotButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            label.text = selectedMoves[i] == MoveType.None ? "-" : selectedMoves[i].ToString();

            slotButtons[i].image.color = selectedMoves[i] == MoveType.None
                ? normalColor : selectedColor;
        }
    }

     public void OnConfirm()
     {
         for (int i = 0; i < selectedMoves.Length; i++)
         {
             PlayerPrefs.SetString($"SkillSlot{i}", selectedMoves[i].ToString());
         }
         PlayerPrefs.Save();

         List<MoveType> finalMoves = new List<MoveType> { MoveType.Heal, MoveType.Cure };
         finalMoves.AddRange(selectedMoves);

         GameSettings.playerMoves = finalMoves;

         Debug.Log("Finished selecting skills: " + string.Join(", ", finalMoves));
         SceneManager.LoadScene("TitleScene");
     }
}
