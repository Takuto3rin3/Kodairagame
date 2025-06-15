using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum StatusEffect { None, Burned, Paralyzed }

public class Player
{
    public string Name;
    public bool isCPU = false;

    public int Shield = 6; // 初期シールドを6に
    public StatusEffect Status = StatusEffect.None;
    public bool[] FingersUp = new bool[2]; // 0: Left, 1: Right

    public bool JustDied = false; // ダメージによる敗北判定用フラグ
    public bool HasPiercing = false; // 「溜める」で付与される貫通効果
    public bool IsAlive => !JustDied;
    public List<MoveType> MoveSet = new List<MoveType>(); // プレイヤーが持つ技リスト


    
    public int HealCureUses = 0;
    public const int MaxHealCureUses = 5;

    public Player(string name, bool isCPU = false)
    {
        Name = name;
        this.isCPU = isCPU;
    }

    public int FingersUpCount() => FingersUp.Count(f => f);

    public void TakeDamage(int amount, bool ignoreShield = false)
{
    if (!ignoreShield && !HasPiercing)
    {
        Shield = Mathf.Max(0, Shield - amount);
        if (Shield <= 0)
        {
            return; // シールドが0でもまだ生きてる（通常攻撃では死なない）
        }
    }
    else
    {
        Shield = Mathf.Max(0, Shield - amount);
        Die(); // 貫通 or ignoreShield の場合は即死
    }

    HasPiercing = false;
}


public void SetBurned(bool value)
{
    if (value)
        ApplyStatus(StatusEffect.Burned);
    else
        ClearStatus(); // 🔄 ここだけ修正！
}



    public void RecoverShield(int amount)
    {
        Shield = Mathf.Min(8, Shield + amount); // 最大8まで回復可能
    }

    public void ApplyStatus(StatusEffect status)
    {
        Status = status;
    }

    public void ClearStatus()
    {
        Status = StatusEffect.None;
    }

    public void StartTurn()
    {
        if (Status == StatusEffect.Burned)
        {
            TakeDamage(1); // やけどで毎ターン1ダメージ
        }
    }

    public bool CanAct()
{
    if (Status == StatusEffect.Paralyzed)
    {
        Status = StatusEffect.None;
        return false;
    }
    return true;
}

public bool IsBurned => Status == StatusEffect.Burned;
public bool IsParalyzed => Status == StatusEffect.Paralyzed;


    public void RandomizeFingers()
    {
        int pattern = Random.Range(0, 4);
        switch (pattern)
        {
            case 0: FingersUp[0] = true; FingersUp[1] = false; break;
            case 1: FingersUp[0] = false; FingersUp[1] = true; break;
            case 2: FingersUp[0] = true; FingersUp[1] = true; break;
            case 3: FingersUp[0] = false; FingersUp[1] = false; break;
        }
    }

    public Move ChooseMove(List<Player> allPlayers)
{
    var targets = allPlayers.Where(p => p != this && p.FingersUp.Any(up => up)).ToList();

    foreach (var moveType in MoveSet)
    {
        var move = new Move(moveType);
        if (move.CanActivate(this, targets))
            return move;
    }

    // 代替候補（MoveSetの中から失敗しないもの）
    var fallback = MoveSet
        .Select(mt => new Move(mt))
        .Where(m => m.CanActivate(this, targets))
        .ToList();

    if (fallback.Count > 0)
        return fallback[Random.Range(0, fallback.Count)];

    return new Move(MoveType.Heal); // 最後の保険
}


public void Die()
{
    JustDied = true;
    // 任意でログ出力や演出追加
}

public void RandomizeSingleFinger()
{
    int index = Random.Range(0, 2);
    FingersUp[0] = index == 0;
    FingersUp[1] = index == 1;
}

public bool HasSelectedFinger()
{
    return FingersUp[0] || FingersUp[1];
}

}

