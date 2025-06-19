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
    public Color selectedColor = new Color(0.4f, 0.8f, 1f);
    public Color normalColor = Color.white;

    private MoveType[] selectedMoves = new MoveType[8];
    private int selectedSlotIndex = -1;

    private static readonly MoveType[] defaultEight = {
        MoveType.Straight, MoveType.FireKick, MoveType.ThunderPunch, MoveType.Earthquake,
        MoveType.StraightFire, MoveType.CrossThunder, MoveType.Gun, MoveType.Charge
    };

    void Start()
    {
        // 1. Load slots from PlayerPrefs or set defaults
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
        if (!hasSaved)
        {
            for (int i = 0; i < slotButtons.Count; i++)
                selectedMoves[i] = defaultEight[i];
        }

        // 2. Update slot visuals
        RefreshSlotVisuals();

        // 3. Setup slot button clicks
        for (int i = 0; i < slotButtons.Count; i++)
        {
            int index = i;
            slotButtons[i].onClick.AddListener(() => OnSlotClicked(index));
        }

        // 4. Draw initial move list (unselected moves only)
        RefreshMoveList();
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

        // Assign move to selected slot
        selectedMoves[selectedSlotIndex] = move;

        // Update UI
        RefreshSlotVisuals();
        RefreshMoveList();

        Debug.Log($"Assigned {move} to slot {selectedSlotIndex + 1}");
    }

    // Update slot labels and colors
    private void RefreshSlotVisuals()
    {
        for (int i = 0; i < slotButtons.Count; i++)
        {
            var label = slotButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            label.text = selectedMoves[i] == MoveType.None ? "-" : selectedMoves[i].ToString();
            slotButtons[i].image.color =
                selectedMoves[i] == MoveType.None ? normalColor : selectedColor;
        }
    }

    // Rebuild moveListContainer: show only moves not in selectedMoves
    private void RefreshMoveList()
    {
        // Clear existing buttons
        foreach (Transform child in moveListContainer)
            Destroy(child.gameObject);

        // Instantiate buttons for unselected moves
        foreach (MoveType move in System.Enum.GetValues(typeof(MoveType)))
        {
            if (move == MoveType.None || move == MoveType.Heal || move == MoveType.Cure)
                continue;
            if (selectedMoves.Contains(move))
                continue;

            var btnObj = Instantiate(moveButtonPrefab, moveListContainer);
            var moveBtn = btnObj.GetComponent<MoveButton>();
            moveBtn.Init(move, OnMoveSelected);
        }
    }

    public void OnConfirm()
    {
        // Save selections
        for (int i = 0; i < selectedMoves.Length; i++)
            PlayerPrefs.SetString($"SkillSlot{i}", selectedMoves[i].ToString());
        PlayerPrefs.Save();

        // Build final move list
        var finalMoves = new List<MoveType> { MoveType.Heal, MoveType.Cure };
        finalMoves.AddRange(selectedMoves);
        GameSettings.playerMoves = finalMoves;

        Debug.Log("Finished selecting skills: " + string.Join(", ", finalMoves));
        SceneManager.LoadScene("TitleScene");
    }
}
