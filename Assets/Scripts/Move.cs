using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MoveType
{
None,
    Straight, Cross, FireKick, ThunderPunch, Earthquake,
    StraightFire, CrossThunder, Heal, Cure, Ace, Katana, Gun, Charge
}

public class Move
{
    public MoveType Type;
    public string Name;

    public Move(MoveType type)
    {
        Type = type;
        Name = type.ToString();
    }

         public bool CanActivate(Player self, List<Player> targets)
     {
         int totalFingers = targets.Sum(p => p.FingersUpCount()) + self.FingersUpCount();
        bool anyNoFingerOpp = targets.Any(p => p.FingersUpCount() == 0);
        bool selfNoFinger   = self.FingersUpCount() == 0;
         bool selfRightUp = self.FingersUp[0];
         bool selfLeftUp  = self.FingersUp[1];

         switch (Type)
         {
            case MoveType.Earthquake:
                // 自分 or 相手の誰かが「両手 DOWN」なら発動可
                return anyNoFingerOpp || selfNoFinger;


            case MoveType.Cross:
                return targets.Any(p =>
                    (selfRightUp && p.FingersUp[0]) ||
                    (selfLeftUp && p.FingersUp[1])
                );

            case MoveType.FireKick:
                return totalFingers == 2;

            case MoveType.ThunderPunch:
                return totalFingers == 3;

            case MoveType.StraightFire:
            case MoveType.CrossThunder:
                return self.FingersUp.All(up => up) &&
                       targets.All(p => p.FingersUp.All(up => up));

            case MoveType.Heal:
            case MoveType.Cure:
                return self.HealCureUses < Player.MaxHealCureUses;

            case MoveType.Ace:
                return totalFingers == 1;

            case MoveType.Katana:
            case MoveType.Gun:
                return self.Shield >= 2;

            case MoveType.Charge:
                return totalFingers <= 1;

            default:
                return true;
        }
    }

    public void ApplyEffect(Player self, List<Player> targets, List<Player> allPlayers)
    {
        GameManager gm = Object.FindFirstObjectByType<GameManager>();

        switch (Type)
        {
            case MoveType.Straight:
                var straightTargets = targets.Where(p => p.FingersUpCount() == 1).ToList();
                foreach (var t in straightTargets) t.TakeDamage(1);
                gm?.LogDamage(self, straightTargets, Name, 1);
                break;

            case MoveType.Cross:
                var crossTargets = targets.Where(p => p.FingersUp.All(up => up)).ToList();
                foreach (var t in crossTargets) t.TakeDamage(1);
                gm?.LogDamage(self, crossTargets, Name, 1);
                break;

            case MoveType.FireKick:
                foreach (var t in targets)
                {
                    t.TakeDamage(2);
                    t.ApplyStatus(StatusEffect.Burned);
                }
                gm?.LogDamage(self, targets, Name, 2);
                break;

            case MoveType.ThunderPunch:
                foreach (var t in targets)
                {
                    t.TakeDamage(3);
                    t.ApplyStatus(StatusEffect.Paralyzed);
                }
                gm?.LogDamage(self, targets, Name, 3);
                break;

            case MoveType.Earthquake:
          　　　　// 指を上げていないプレイヤー全員（自分含む場合あり）が対象
                var quakeTargets = allPlayers
                   .Where(p => p.IsAlive && p.FingersUpCount() == 0)
                   .ToList();

               foreach (var t in quakeTargets) t.TakeDamage(4);
               gm?.LogDamage(self, quakeTargets, Name, 4);
                break;

            case MoveType.StraightFire:
                var sfTargets = targets.Where(p => p.FingersUp.All(up => up)).ToList();
                foreach (var t in sfTargets)
                {
                    t.TakeDamage(4);
                    t.ApplyStatus(StatusEffect.Burned);
                }
                gm?.LogDamage(self, sfTargets, Name, 4);
                break;

            case MoveType.CrossThunder:
                var ctTargets = targets.Where(p => p.FingersUp.All(up => up)).ToList();
                foreach (var t in ctTargets)
                {
                    t.TakeDamage(4);
                    t.ApplyStatus(StatusEffect.Paralyzed);
                }
                gm?.LogDamage(self, ctTargets, Name, 4);
                break;

            case MoveType.Heal:
                self.HealCureUses++;
                self.RecoverShield(2);
                gm?.AppendMoveLog($"{self.Name} used {Name} and recovered 2 shield. ({self.HealCureUses}/{Player.MaxHealCureUses})");
                break;

            case MoveType.Cure:
                self.HealCureUses++;
                self.ClearStatus();
                gm?.AppendMoveLog($"{self.Name} used {Name} and cured all status. ({self.HealCureUses}/{Player.MaxHealCureUses})");
                break;

            case MoveType.Ace:
                foreach (var target in targets.Where(p => p.IsAlive && p.FingersUpCount() > 0))
                {
                    target.TakeDamage(1, ignoreShield: false);
                    target.SetBurned(true);
                }
                break;

            case MoveType.Katana:
                self.Shield -= 2;
                foreach (var target in targets.Where(p => p.IsAlive))
                {
                    target.Shield = target.Shield / 2;
                }
                break;

            case MoveType.Charge:
                self.HasPiercing = true;
                break;
        }
    }
}

