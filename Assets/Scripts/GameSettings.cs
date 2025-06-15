using UnityEngine;
using System.Collections.Generic; // ← これを追加！

public static class GameSettings
{
public static int cpuCount = 1;
    public static bool useCpuOpponent = true;
    public static List<MoveType> selectedMoves = new List<MoveType>();

    public static List<MoveType> playerMoves = new List<MoveType>(); // Heal+Cure+8技

    public static void SetDefaultMoves()
    {
        playerMoves = new List<MoveType>
        {
            MoveType.Heal,
            MoveType.Cure,
            MoveType.Straight,
            MoveType.Cross,
            MoveType.FireKick,
            MoveType.ThunderPunch,
            MoveType.Earthquake,
            MoveType.StraightFire,
            MoveType.CrossThunder,
            MoveType.Ace
        };
    }
}

