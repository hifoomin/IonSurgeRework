using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using R2API;
using R2API.ContentManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using RoR2.Skills;
using MonoMod.Cil;
using EntityStates;

namespace IonSurgeRework
{
    [BepInDependency(LanguageAPI.PluginGUID)]
    // [BepInDependency(DirectorAPI.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "HIFU";
        public const string PluginName = "IonSurgeRework";
        public const string PluginVersion = "1.0.0";
        public static ConfigEntry<float> ionSurgeAoe;
        public static ConfigEntry<float> ionSurgeCooldown;
        public static ConfigEntry<float> ionSurgeDamage;
        public static ConfigEntry<float> ionSurgeDashSpeed;
        public static ConfigEntry<float> ionSurgeDuration;
        public static ConfigEntry<bool> ionSurgeScaleWithSpeed;
        public static ConfigEntry<int> ionSurgeBaseMaxStock;
        public static ConfigEntry<int> ionSurgeRechargeStock;

        public static ConfigEntry<float> flamethrowerDamage;
        public static ConfigEntry<float> flamethrowerIgniteChance;
        public static ConfigEntry<float> flamethrowerRange;

        public static ManualLogSource logger;

        public void Awake()
        {
            logger = this.Logger;

            ionSurgeAoe = Config.Bind("Ion Surge", "Area of Effect", 14f, "Vanilla is 14");
            ionSurgeCooldown = Config.Bind("Ion Surge", "Cooldown", 5f, "Vanilla is 8");
            ionSurgeDamage = Config.Bind("Ion Surge", "Damage", 8f, "Decimal. Vanilla is 8");
            ionSurgeDashSpeed = Config.Bind("Ion Surge", "Dash Speed Multiplier", 3.5f, "");
            ionSurgeDuration = Config.Bind("Ion Surge", "Skill Duration", 0.3f, "This affects how high you go. Vanilla is 1.66");
            ionSurgeScaleWithSpeed = Config.Bind("Ion Surge", "Make height scale with movement speed?", false, "Vanilla is true");

            ionSurgeBaseMaxStock = Config.Bind("Ion Surge", "Maximum Stock", 1, "");
            ionSurgeRechargeStock = Config.Bind("Ion Surge", "Stock to Recharge", 1, "");

            flamethrowerDamage = Config.Bind("Flamethrower", "Damage", 20f, "Decimal. Vanilla is 20");
            flamethrowerIgniteChance = Config.Bind("Flamethrower", "Ignite Chance", 50f, "Decimal. Vanilla is 50");
            flamethrowerRange = Config.Bind("Flamethrower", "Range", 20f, "Vanilla is 20");

            LanguageAPI.Add("MAGE_SPECIAL_LIGHTNING_DESCRIPTION", "<style=cIsDamage>Stunning</style>. Soar and dash, dealing <style=cIsDamage>" + (ionSurgeDamage.Value * 100f) + "% damage</style> in a large area." + (ionSurgeBaseMaxStock.Value > 1 ? " <style=cIsUtility>Can dash up to " + ionSurgeBaseMaxStock.Value + "times</style>." : ""));
            var flamethrowerFinalDamage = (flamethrowerDamage.Value + (flamethrowerDamage.Value / (1f / (flamethrowerIgniteChance.Value / 100f)))) * 100f;
            LanguageAPI.Add("MAGE_SPECIAL_FIRE_DESCRIPTION", "<style=cIsDamage>Ignite</style>. Burn all enemies in front of you for <style=cIsDamage>" + flamethrowerFinalDamage + "% damage</style>.");

            var ionSurgeSkillDef = Addressables.LoadAssetAsync<SkillDef>("fc060d4c1b29e1445b355c9e3e4925d1").WaitForCompletion();
            // guid is mage body fly up
            ionSurgeSkillDef.baseRechargeInterval = ionSurgeCooldown.Value;
            ionSurgeSkillDef.baseMaxStock = ionSurgeBaseMaxStock.Value;
            ionSurgeSkillDef.rechargeStock = ionSurgeRechargeStock.Value;

            On.EntityStates.Mage.FlyUpState.OnEnter += IonSurgeOnEnter;
            if (!ionSurgeScaleWithSpeed.Value)
            {
                IL.EntityStates.Mage.FlyUpState.HandleMovements += IonSurgeHandleMovements;
            }

            On.EntityStates.Mage.Weapon.Flamethrower.OnEnter += FlamethrowerOnEnter;
        }

        private void FlamethrowerOnEnter(On.EntityStates.Mage.Weapon.Flamethrower.orig_OnEnter orig, EntityStates.Mage.Weapon.Flamethrower self)
        {
            EntityStates.Mage.Weapon.Flamethrower.ignitePercentChance = flamethrowerIgniteChance.Value;
            EntityStates.Mage.Weapon.Flamethrower.totalDamageCoefficient = flamethrowerDamage.Value;
            self.maxDistance = flamethrowerRange.Value;
            orig(self);
        }

        private void IonSurgeHandleMovements(ILContext il)
        {
            ILCursor c = new(il);
            bool found = c.TryGotoNext(MoveType.After,
                x => x.MatchLdfld<BaseState>("moveSpeedStat"));

            if (!found)
            {
                logger.LogError("Failed to IL hook Ion Surge Handle Movements");
                return;
            }

            c.EmitDelegate<Func<float, float>>((orig) =>
            {
                return 10.15f;
            });
        }

        private void IonSurgeOnEnter(On.EntityStates.Mage.FlyUpState.orig_OnEnter orig, EntityStates.Mage.FlyUpState self)
        {
            EntityStates.Mage.FlyUpState.blastAttackRadius = ionSurgeAoe.Value;
            EntityStates.Mage.FlyUpState.blastAttackDamageCoefficient = ionSurgeDamage.Value;
            EntityStates.Mage.FlyUpState.duration = ionSurgeDuration.Value;
            orig(self);
            if (self.isAuthority)
            {
                Vector3 direction = self.inputBank.moveVector == Vector3.zero ? Vector3.zero : self.inputBank.moveVector.normalized;
                Vector3 a = direction.normalized * ionSurgeDashSpeed.Value * self.moveSpeedStat;
                self.characterMotor.Motor.ForceUnground();
                self.characterMotor.velocity = a;
            }
        }
    }
}
