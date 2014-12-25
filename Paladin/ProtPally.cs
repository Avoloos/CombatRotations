using System;
using System.Linq;
using Newtonsoft.Json;
using ReBot.API;

namespace ReBot
{
    [Rotation("Prot Pally", "Shalzuth", WoWClass.Paladin, Specialization.PaladinProtection, 5)]
    public class ProtPally : CombatRotation
    {
        [JsonProperty("DisableDivineShield")]
        public Boolean DisableDivineShield = true;
        public Int32 HolyPower { get { return Me.GetPower(WoWPowerType.PaladinHolyPower); } }
        public Boolean TargettingTank { get { return ((PlayerObject)Target.Target).IsTank; } }

        public ProtPally()
        {
            PullSpells = new[] { "Judgment" };
        }

        public override bool OutOfCombat()
        {
            if (CastSelf("Flash of Light", () => Me.HealthFraction <= 0.9)) return true;

            if (CastSelf("Righteous Fury", () => !HasAura("Righteous Fury"))) return true;

            if (CastSelf("Blessing of Kings", () => !HasAura("Blessing of Kings") && !HasAura("Mark of the Wild") && !HasAura("Legacy of the Emperor"))) return true;
            if (CastSelf("Blessing of Might", () => !HasAura("Blessing of Kings", true))) return true;

            if (CastSelf("Cleanse", () => Me.Auras.Any(x => x.IsDebuff && "Disease Poison".Contains(x.DebuffType)))) return true;

            if (CastSelf("Seal of Insight", () => !IsInShapeshiftForm("Seal of Insight"))) return true;

            return false;
        }

        public override void Combat()
        {
            if (CastSelf("Lay on Hands", () => Me.HealthFraction <= 0.15 && !HasAura("Divine Shield") && !HasAura("Immunity"))) return;
            if (CastSelf("Divine Shield", () => Me.HealthFraction <= 0.1 && !HasAura("Immunity"))) return;

            if (CastSelf("Flash of Light", () => Me.HealthFraction <= 0.75 && HasAura("Divine Shield") || Me.HealthFraction <= 0.4)) return;

            if (Cast("Rebuke", () => Target.IsCastingAndInterruptible())) return;
            if (Cast("Fist of Justice", () => Target.IsCastingAndInterruptible())) return;

            if (CastSelf("Hand of Freedom", () => !Target.IsInCombatRange && Me.MovementSpeed > 0 && Me.MovementSpeed < MovementSpeed.NormalRunning)) return;

            if (CastSelf("Cleanse", () => Me.Auras.Any(x => x.IsDebuff && "Disease Poison".Contains(x.DebuffType)))) return;

            if (CastSelf("Ardent Defender", () => Me.HealthFraction < 0.2 && !HasAura("Divine Shield"))) return;
            if (CastSelf("Guardian of Ancient Kings", () => Me.HealthFraction < 0.5 && !HasAura("Divine Shield"))) return;
            if (CastSelf("Seraphim", () => HasSpell("Seraphim") && Me.HealthFraction < 0.7 && !HasAura("Divine Shield"))) return;
            if (CastSelf("Divine Protection", () => Me.HealthFraction <= 0.3 && Target.IsCasting && !HasAura("Divine Shield"))) return;

            if (Cast("Reckoning", () => !TargettingTank)) return;
            if (Cast("Hand of Salvation", () => !TargettingTank, Target.Target)) return;
            if (Cast("Hand of Sacrifice", () => !TargettingTank, Target.Target)) return;
            var UntauntedAdd = Adds.FirstOrDefault(p => !((PlayerObject)p.Target).IsTank);
            if (UntauntedAdd != null)
            {
                if (Cast("Reckoning", UntauntedAdd)) return;
                if (Cast("Hand of Salvation", UntauntedAdd.Target)) return;
                if (Cast("Hand of Sacrifice", UntauntedAdd.Target)) return;
            }

            if (CastSelf("Sacred Shield", () => HasSpell("Sacred Shield") && !HasAura("Sacred Shield"))) return;
            if (CastSelf("Eternal Flame", () => HasSpell("Eternal Flame") && !HasAura("Eternal Flame") && HolyPower >= 3)) return;
            if (CastSelf("Word of Glory", () => HasSpell("Word of Glory") && Me.HealthFraction <= 0.7 && HolyPower >= 3)) return;
            if (CastSelf("Word of Glory", () => HasSpell("Word of Glory") && Me.HealthFraction <= 0.5 && HolyPower >= 2)) return;

            if (Cast("Shield of the Righteous", () => HolyPower >= 3)) return;

            if (Adds.Where(x => Me.DistanceSquaredTo(x) < 64).Count() >= 3)
            {
                if (CastSelf("Seal of Righteousness", () => !IsInShapeshiftForm("Seal of Righteousness"))) return;
                if (Cast("Hammer of the Righteous")) return;
            }
            else
            {
                if (CastSelf("Seal of Insight", () => !IsInShapeshiftForm("Seal of Insight"))) return;
                if (Cast("Crusader Strike")) return;
            }
            if (Cast("Judgment")) return;
            if (Cast("Holy Wrath", () => Target.IsInCombatRangeAndLoS)) return;
            if (Cast("Avenger's Shield")) return;
            if (Cast("Execution Sentence", () => HasSpell("Execution Sentence"))) return;
            if (Cast("Light's Hammer", () => HasSpell("Light's Hammer"))) return;
            if (Cast("Holy Prism", () => HasSpell("Holy Prism"))) return;
            if (Cast("Hammer of Wrath", () => Target.HealthFraction < 0.2)) return;
            if (Cast("Consecration", () => Target.IsInCombatRangeAndLoS)) return;
        }
    }
}
