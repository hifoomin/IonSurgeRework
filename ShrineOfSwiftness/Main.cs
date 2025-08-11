using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using R2API;
using R2API.ContentManagement;
using UnityEngine;

namespace ShrineOfSwiftness
{
    [BepInDependency(PrefabAPI.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency(R2APIContentManager.PluginGUID)]
    // [BepInDependency(DirectorAPI.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "HIFU";
        public const string PluginName = "ShrineOfSwiftness";
        public const string PluginVersion = "1.0.0";
        public static ManualLogSource sosLogger;
        public static AssetBundle bundle;
        public static ConfigEntry<int> baseCost;
        public static ConfigEntry<int> costIncreasePerActivation;
        public static ConfigEntry<int> maxActivations;
        public static ConfigEntry<string> stageWhitelist;
        public static ConfigEntry<string> itemWhitelist;
        public static List<string> stagesToAppearOn;
        public static List<string> itemsToDrop;
        public void Awake()
        {
            sosLogger = base.Logger;
            baseCost = Config.Bind("Shrine of Swiftness", "Base Cost", 25, "The base cost of the Shrine of Swiftness. For reference, a Chest is $25, and a Large Chest is $50.");
            costIncreasePerActivation = Config.Bind("Shrine of Swiftness", "Cost Increase per Activation", 25, "");
            maxActivations = Config.Bind("Shrine of Swiftness", "Maximum Amount of Activations", 3, "");
            stageWhitelist = Config.Bind("Shrine of Swiftness", "Stage Whitelist", "frozenwall | wispgraveyard | sulfurpools | habitat | habitatfall", "A list of internal stage names that Shrine of Swiftness can spawn on, separated by a space, following a pipe symbol and another space. Use the DebugToolkit mod and its list_stage command to see all internal stage names.");
            itemWhitelist = Config.Bind("Shrine of Swiftness", "Item Whitelist", "Hoof | SprintBonus | SprintOutOfCombat", "A list of internal item names that Shrine of Swiftness can drop, separated by a space, following a pipe symbol and another space. Use the DebugToolkit mod and its list_item or item_list command to see all internal item names.");

            stagesToAppearOn = Main.stageWhitelist.Value.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
            itemsToDrop = Main.itemWhitelist.Value.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

            Prefabs.Init();
        }
    }
}
