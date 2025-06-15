using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MoveSetupManager : MonoBehaviour
{
    public List<Button> slotButtons; // SlotButton0〜7をアサイン
    public Transform moveListContainer; // MoveListScroll/Viewport/Content
    public GameObject moveButtonPrefab; // MoveButtonPrefabプレハブ

    private MoveType[] selectedMoves = new MoveType[8]; // Heal/Cure除いた8個分
    private int selectedSlotIndex = -1;

    void Start()
    {
        // スロット初期化
        for (int i = 0; i < slotButtons.Count; i++)
        {
            string saved = PlayerPrefs.GetString($"SkillSlot{i}", "");
            if (!string.IsNullOrEmpty(saved) && System.Enum.TryParse(saved, out MoveType parsed))
            {
                selectedMoves[i] = parsed;
            }
            else
            {
                selectedMoves[i] = MoveType.None;
            }

            TextMeshProUGUI label = slotButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            label.text = selectedMoves[i].ToString();
        }

        // 初期技セット
        MoveType[] initialMoves = new MoveType[]
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

        for (int i = 0; i < slotButtons.Count; i++)
        {
            selectedMoves[i] = initialMoves[i];
            TextMeshProUGUI label = slotButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            label.text = initialMoves[i].ToString();
        }

        // スロットボタンにクリックイベントを設定
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

        TextMeshProUGUI label = slotButtons[selectedSlotIndex].GetComponentInChildren<TextMeshProUGUI>();
        label.text = move.ToString();

        Debug.Log($"Assigned {move} to slot {selectedSlotIndex + 1}");
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

