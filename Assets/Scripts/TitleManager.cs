using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public void OnStartBattleWithCPU1() => OnSelectCpuCount(1);
    public void OnStartBattleWithCPU2() => OnSelectCpuCount(2);
    public void OnStartBattleWithCPU3() => OnSelectCpuCount(3);

    public void OnSelectCpuCount(int count)
    {
        GameSettings.useCpuOpponent = true;
        GameSettings.cpuCount = count;
        SceneManager.LoadScene("BattleScene");
    }

    public void OnStartBattleWithPlayer()
    {
        GameSettings.useCpuOpponent = false;
        GameSettings.cpuCount = 0;
        SceneManager.LoadScene("BattleScene");
    }

    // ✅ 技セット選択画面に遷移
    public void OnSelectMoves()
    {
        SceneManager.LoadScene("MoveSetupScene"); // ← シーン名は正確に
    }
}

