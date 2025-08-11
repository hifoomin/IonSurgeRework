using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using RoR2;
using R2API;
using R2API.ContentManagement;
using UnityEngine;

[assembly: HG.Reflection.SearchableAttribute.OptInAttribute]

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
        public static List<string> stagesToAppearOn = new();
        public static List<string> itemsToDrop = new();
        public static List<ItemIndex> itemIndexList = new();
        public static Main Instance;
        public void Awake()
        {
            Instance = this;

            sosLogger = base.Logger;

            bundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Instance.Info.Location), "shrineofswiftness"));

            baseCost = Config.Bind("Shrine of Swiftness", "Base Cost", 25, "The base cost of the Shrine of Swiftness. For reference, a Chest is $25, and a Large Chest is $50.");
            costIncreasePerActivation = Config.Bind("Shrine of Swiftness", "Cost Increase per Activation", 25, "");
            maxActivations = Config.Bind("Shrine of Swiftness", "Maximum Amount of Activations", 3, "");
            stageWhitelist = Config.Bind("Shrine of Swiftness", "Stage Whitelist", "frozenwall | wispgraveyard | sulfurpools | habitat | habitatfall", "A list of internal stage names that Shrine of Swiftness can spawn on, separated by a space, following a pipe symbol and another space. Use the DebugToolkit mod and its list_stage command to see all internal stage names.");
            itemWhitelist = Config.Bind("Shrine of Swiftness", "Item Whitelist", "ITEM_HOOF_NAME | ITEM_SPRINTBONUS_NAME | ITEM_SPRINTOUTOFCOMBAT_NAME", "A list of internal item name tokens that Shrine of Swiftness can drop, separated by a space, following a pipe symbol and another space. Use the DebugToolkit mod and its list_item or item_list command to see all internal item names.");

            stagesToAppearOn = Main.stageWhitelist.Value.Split(" | ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
            itemsToDrop = Main.itemWhitelist.Value.Split(" | ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

            for (int i = 0; i < stagesToAppearOn.Count; i++)
            {
                var stage = stagesToAppearOn[i];
                Main.sosLogger.LogError("stages to appear on is " + stage);
            }

            for (int j = 0; j < itemsToDrop.Count; j++)
            {
                var item = itemsToDrop[j];
                Main.sosLogger.LogError("items to drop is " + item);
            }

            Prefabs.Init();
        }

        [SystemInitializer(typeof(ItemCatalog))]
        private static void GetItemIndices()
        {
            Main.sosLogger.LogError("GetItemIndices called");
            foreach (ItemDef itemDef in ItemCatalog.allItemDefs)
            {
                if (itemsToDrop.Contains(itemDef.nameToken))
                {
                    Main.sosLogger.LogError("itemstodrop contains itemdef name, adding itemindex to itemindexlist !!");
                    itemIndexList.Add(itemDef.itemIndex);
                }
            }

            for (int k = 0; k < itemIndexList.Count; k++)
            {
                var index = itemIndexList[k];
                Main.sosLogger.LogError("itemindex is " + index);
            }
        }
    }
}
