using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ReBot.API;

namespace ReBot
{
    public enum MainPoison
    {
        WoundPoison = 8679,
        InstantPoison = 157584,
        DeadlyPoison = 2823,
    }
    public enum SubPoison
    {
        CripplingPoison = 3408,
        LeechingPoison = 108211,
    }
    [Rotation("Combat Rogue", "Shalzuth", WoWClass.Rogue, Specialization.RogueCombat, 5, 25)]
    public class CombatRogue : CombatRotation
    {
        [JsonProperty("MainHand"), JsonConverter(typeof(StringEnumConverter))]
        public MainPoison MainHand = MainPoison.InstantPoison;
        [JsonProperty("OffHand"), JsonConverter(typeof(StringEnumConverter))]
        public SubPoison OffHand = SubPoison.LeechingPoison;
        [JsonProperty("Raiding")]
        public Boolean RaidMode = true;
        public Int32 Energy { get { return Me.GetPower(WoWPowerType.Energy); } }
        public Boolean TargettingMe { get { return Target.Target == (UnitObject)Me; } }
        public String RangedAtk = "Throw";
        public Int32 OraliusWhisperingCrystal = 118922;
        public Int32 OraliusWhisperingCrystalBuff = 176151;
        public CombatRogue()
        {
            if (HasSpell("Shuriken Toss"))
                RangedAtk = "Shuriken Toss";
        }

        public override bool OutOfCombat()
        {
            if (CastSelf("Recuperate", () => Me.HealthFraction < 0.8 && Energy >= 30 && Me.ComboPoints > 0 && !HasAura("Recuperate"))) return true;

            if (CastSelf("Cloak of Shadows", () => Me.Auras.Any(x => x.IsDebuff && x.DebuffType.Contains("magic")))) return true;
            if (MainHand == MainPoison.InstantPoison)
            {
                if (!HasAura((int)MainHand))
                {
                    if (!HasAura((int)MainPoison.DeadlyPoison))
                    {
                        API.ExecuteMacro("/cast Deadly Poison");
                        return true;
                    }
                }
            }
            else
                if (CastSelf((int)MainHand, () => !HasAura((int)MainHand))) return true;
            if (CastSelf((int)OffHand, () => !HasAura((int)OffHand))) return true;

            if (API.HasItem(OraliusWhisperingCrystal) && !HasAura(OraliusWhisperingCrystalBuff) && API.ItemCooldown(OraliusWhisperingCrystal) == 0)
            {
                API.UseItem(OraliusWhisperingCrystal);
                return true;
            }

            return false;
        }
        public void StealthActions()
        {
            if (Cast("Ambush", () => Energy >= 60)) return;
        }
        public void RegularActions()
        {
            if (Cast("Kick", () => Target.IsCastingAndInterruptible())) return;
            UnitObject castingAddInRange = Adds.Where(x => x.IsInCombatRangeAndLoS).ToList().FirstOrDefault(x => x.IsCastingAndInterruptible());
            if (castingAddInRange != null)
                if (Cast("Kick", castingAddInRange)) return;

            if (CastSelf("Evasion", () => TargettingMe && Me.HpLessThanOrElite(0.85))) return;

            if (CastSelf("Recuperate", () => !RaidMode && Me.HealthFraction < 0.9 && Energy >= 30 && Me.ComboPoints > 0 && !HasAura("Recuperate"))) return;

            if (Cast("Shiv", () => !RaidMode && Me.HealthFraction < 0.7 && Energy >= 30)) return;

            if (CastSelf("Slice and Dice", () => Me.ComboPoints >= 2 && Energy >= 25 && (!HasAura("Slice and Dice") || AuraTimeRemaining("Slice and Dice") < 2.5f))) return;

            if (Cast("Killing Spree", () => Energy < 20 && !HasAura("Adrenaline Rush"))) return;

            if (CastSelf("Adrenaline Rush", () => !HasAura("Bloodlust") && !HasAura("Heroism") && !HasAura("Time Warp"))) return;

            if (CastSelf("Preparation", () => !TargettingMe && Energy > 60 && HasAura("Deep Insight") && SpellCooldown("Vanish") > 0)) return;
            if (CastSelf("Vanish", () => RaidMode && !TargettingMe && Energy > 60 && HasAura("Deep Insight"))) return;

            if (Cast("Revealing Strike", () => Energy >= 40 && !Target.HasAura("Revealing Strike"))) return;

            if (Adds.Where(x => x.IsInCombatRangeAndLoS).ToList().Count > 4)
            {
                if (Cast("Crimson Tempest", () => Energy >= 35 && (Me.ComboPoints == 5))) return;
            }
            else
            {
                if (Cast("Eviscerate", () => Energy >= 35 && (Me.ComboPoints == 5))) return;
            }
            if (Cast("Sinister Strike", () => Energy >= 50)) return;
        }
        public void RunToEnemy()
        {
            if (CastSelfPreventDouble("Stealth", () => !Me.InCombat && !HasAura("Stealth"))) return;
            if (Cast("Shadowstep", () => !HasAura("Sprint") && HasSpell("Shadowstep"))) return;
            if (CastSelf("Sprint", () => !HasAura("Sprint") && !HasAura("Burst of Speed"))) return;
            if (CastSelf("Burst of Speed", () => !HasAura("Sprint") && !HasAura("Burst of Speed") && HasSpell("Burst of Speed") && Energy > 20)) return;
            if (Cast(RangedAtk, () => Energy >= 40 && !HasAura("Stealth") && Target.IsInLoS)) return;
        }

        public override void Combat()
        {
            List<UnitObject> addsInRange = Adds.Where(x => x.IsInCombatRangeAndLoS).ToList();
            if (addsInRange.Count > 0)
            {
                if (Cast("Blade Flurry", () => !HasAura("Blade Flurry"))) return;
            }
            else
            {
                if (Cast("Blade Flurry", () => HasAura("Blade Flurry"))) return;
            }
            if (CastSelf("Recuperate", () => Me.HealthFraction < 0.75 && Me.ComboPoints > 2 && Energy >= 30 && !HasAura("Recuperate"))) return;
            if (CastSelf("Cloak of Shadows", () => TargettingMe && Me.HealthFraction < 0.75 && Target.IsCasting && Target.RemainingCastTime < 1)) return;
            if (CastSelf("Cloak of Shadows", () => Me.Auras.Any(x => x.IsDebuff && x.DebuffType.Contains("magic")))) return;
            if (Target.IsInCombatRangeAndLoS)
            {
                if (HasAura("Stealth"))
                {
                    StealthActions();
                }
                else
                {
                    RegularActions();
                }
            }
            else
            {
                RunToEnemy();
            }
        }
    }
}
