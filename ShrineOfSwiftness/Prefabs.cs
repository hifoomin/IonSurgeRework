using System.Security.AccessControl;
using System.Linq.Expressions;
using System;
using System.Linq;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ShrineOfSwiftness
{
    public class Prefabs
    {
        public static GameObject shrinePrefab;
        public static InteractableSpawnCard interactableSpawnCard;
        public static GameObject shrineVFX;
        public static void Init()
        {
            shrineVFX = Addressables.LoadAssetAsync<GameObject>("0fa235b9d7e778f4ba2cd8f2437f72d9").WaitForCompletion();
            // guid is shrine use effect
            shrinePrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("8a681654848ac374980fea55c4cf55a7").WaitForCompletion(), "Shrine of Swiftness");
            // guid is shrine chance

            shrinePrefab.AddComponent<ShrineOfSwiftnessController>();
            shrinePrefab.AddComponent<UnityJankMoment>();

            var shrineTransform = shrinePrefab.transform;

            var modelLocator = shrinePrefab.GetComponent<ModelLocator>();
            var modelTransform = modelLocator._modelTransform;
            modelTransform.name = "mdlShrineSwiftness";
            var meshRenderer = modelTransform.GetComponent<MeshRenderer>();

            var newModelMaterial = new Material(Addressables.LoadAssetAsync<Material>("0de2169c1f513a44a8b439821be523eb").WaitForCompletion());
            // guid is mat shrine chance
            newModelMaterial.SetTexture("_MainTex", Addressables.LoadAssetAsync<Texture2D>("50576454418d775439d1c15ea6e3d694").WaitForCompletion());
            // guid is tex shrine chance ao filtered
            newModelMaterial.SetColor("_TintColor", Color.white);
            newModelMaterial.SetFloat("_SpecularStrength", 1f);
            newModelMaterial.SetFloat("_SpecularExponent", 1.164457f);
            newModelMaterial.SetFloat("_SnowBias", -1f);
            newModelMaterial.SetFloat("_Depth", 0.9161675f);
            newModelMaterial.SetInt("_RampInfo", 4); // ramp choice - subsurface

            meshRenderer.material = newModelMaterial;

            // TODO: swap material/texture/color to be marble-like

            var symbol = shrineTransform.Find("Symbol").GetComponent<MeshRenderer>();
            var symbolMeshRenderer = symbol.GetComponent<MeshRenderer>();

            var newSymbolMaterial = new Material(Addressables.LoadAssetAsync<Material>("4f1b6f101f0d1cb42893fca3d83b9154").WaitForCompletion());
            // guid is mat shrine chance symbol
            newSymbolMaterial.SetTexture("_MainTex", Main.bundle.LoadAsset<Texture2D>("texShrineOfSwiftnessSymbol.png"));
            newSymbolMaterial.SetColor("_TintColor", new Color32(130, 188, 232, 255));

            symbolMeshRenderer.material = newSymbolMaterial;

            var purchaseInteraction = shrinePrefab.GetComponent<PurchaseInteraction>();
            purchaseInteraction.displayNameToken = "SHRINE_SWIFTNESS_NAME";
            purchaseInteraction.contextToken = "SHRINE_SWIFTNESS_CONTEXT";
            purchaseInteraction.cost = Main.baseCost.Value;
            purchaseInteraction.automaticallyScaleCostWithDifficulty = true;

            var genericDisplayNameProvider = shrinePrefab.GetComponent<GenericDisplayNameProvider>();
            genericDisplayNameProvider.displayToken = "SHRINE_SWIFTNESS_NAME";

            var inspectDef = ScriptableObject.CreateInstance<InspectDef>();
            var inspectInfo = inspectDef.Info = new()
            {
                TitleToken = genericDisplayNameProvider.displayToken,
                DescriptionToken = "SHRINE_SWIFTNESS_DESCRIPTION",
                FlavorToken = "shrine of swiftness flavor token",
                isConsumedItem = false,
                Visual = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texShrineIconOutlined.png").WaitForCompletion(),
                TitleColor = Color.white
            };

            shrinePrefab.GetComponent<GenericInspectInfoProvider>().InspectInfo = UnityEngine.Object.Instantiate(shrinePrefab.GetComponent<GenericInspectInfoProvider>().InspectInfo);
            shrinePrefab.GetComponent<GenericInspectInfoProvider>().InspectInfo.Info = inspectInfo;

            UnityEngine.Object.DestroyImmediate(shrinePrefab.GetComponent<ShrineChanceBehavior>());

            PrefabAPI.RegisterNetworkPrefab(shrinePrefab);
            ContentAddition.AddNetworkedObject(shrinePrefab);

            interactableSpawnCard = ScriptableObject.CreateInstance<InteractableSpawnCard>();
            interactableSpawnCard.prefab = shrinePrefab;
            interactableSpawnCard.sendOverNetwork = true;
            interactableSpawnCard.hullSize = HullClassification.Golem;
            interactableSpawnCard.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;
            interactableSpawnCard.requiredFlags = RoR2.Navigation.NodeFlags.None;
            interactableSpawnCard.directorCreditCost = 0;
            interactableSpawnCard.occupyPosition = true;
            interactableSpawnCard.eliteRules = SpawnCard.EliteRules.Default;
            interactableSpawnCard.orientToFloor = false;
            interactableSpawnCard.slightlyRandomizeOrientation = false;
            interactableSpawnCard.skipSpawnWhenDevotionArtifactEnabled = true;
            interactableSpawnCard.weightScalarWhenSacrificeArtifactEnabled = 1;
            interactableSpawnCard.skipSpawnWhenDevotionArtifactEnabled = false;
            interactableSpawnCard.maxSpawnsPerStage = 1;
            interactableSpawnCard.prismaticTrialSpawnChance = 1f;
            interactableSpawnCard.name = "iscShrineSwiftness";

            LanguageAPI.Add("SHRINE_SWIFTNESS_NAME", "Shrine of Swiftness");
            LanguageAPI.Add("SHRINE_SWIFTNESS_CONTEXT", "Offer to Shrine of Swiftness");
            LanguageAPI.Add("SHRINE_SWIFTNESS_USE_MESSAGE", "<style=cShrine>{0} offered to the shrine and received a gift of celerity.</style>");
            LanguageAPI.Add("SHRINE_SWIFTNESS_USE_MESSAGE_2P", "<style=cShrine>You offer to the shrine and receive a gift of celerity.</style>");
            LanguageAPI.Add("SHRINE_SWIFTNESS_DESCRIPTION", "When activated by a survivor the Shrine of Swiftness drops a random movement speed item.");

            SceneDirector.onPrePopulateSceneServer += OnPrePopulateSceneServer;

        }

        private static void OnPrePopulateSceneServer(SceneDirector director)
        {
            Main.sosLogger.LogError("on pre populate scene server called");
            Main.sosLogger.LogError("most recent scene def cached name is " + SceneCatalog.mostRecentSceneDef.cachedName);
            if (!Main.stagesToAppearOn.Contains(SceneCatalog.mostRecentSceneDef.cachedName))
            {
                Main.sosLogger.LogError("most recent scene def cached name IS NOT in stages to appear on list");
                return;
            }

            Main.sosLogger.LogError("trying to place shrine of swiftness");

            var directorPlacementRule = new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Random
            };
            var shrineInstance = DirectorCore.instance.TrySpawnObject(
                new DirectorSpawnRequest(
                    interactableSpawnCard, directorPlacementRule,
                    Run.instance.runRNG));

            if (shrineInstance)
            {
                Main.sosLogger.LogError("successfully placed shrine of swiftness");
                var purchaseInteraction = shrineInstance.GetComponent<PurchaseInteraction>();
                if (purchaseInteraction && purchaseInteraction.costType == CostTypeIndex.Money)
                {
                    purchaseInteraction.Networkcost = Run.instance.GetDifficultyScaledCost(purchaseInteraction.cost);
                }
            }
        }
    }

    public class UnityJankMoment : MonoBehaviour
    {
        public PurchaseInteraction purchaseInteraction;
        public ShrineOfSwiftnessController shrineOfSwiftnessController;

        public void Start()
        {
            shrineOfSwiftnessController = GetComponent<ShrineOfSwiftnessController>();
            purchaseInteraction = GetComponent<PurchaseInteraction>();
            purchaseInteraction.onPurchase.AddListener(SoTrue);
        }

        public void SoTrue(Interactor interactor)
        {
            shrineOfSwiftnessController.AddShrineStack(interactor);
        }
    }

    public class ShrineOfSwiftnessController : ShrineBehavior
    {
        public int maxPurchaseCount = Main.maxActivations.Value;

        public float costMultiplierPerPurchase = Main.costIncreasePerActivation.Value;

        public Transform symbolTransform;

        private PurchaseInteraction purchaseInteraction;

        private int purchaseCount = 0;

        private float refreshTimer;

        private const float refreshDuration = 2f;

        private bool waitingForRefresh;
        public Transform dropletOrigin;

        public Xoroshiro128Plus rng;

        public override int GetNetworkChannel()
        {
            return RoR2.Networking.QosChannelIndex.defaultReliable.intVal;
        }

        private void Start()
        {
            purchaseInteraction = GetComponent<PurchaseInteraction>();
            symbolTransform = transform.Find("Symbol");
            dropletOrigin = transform.Find("DropletOrigin");
            if (NetworkServer.active)
            {
                rng = new Xoroshiro128Plus(Run.instance.treasureRng.nextUlong);
            }
        }

        public void AddShrineStack(Interactor interactor)
        {
            if (!NetworkServer.active)
            {
                return;
            }
            waitingForRefresh = true;
            var interactorBody = interactor.GetComponent<CharacterBody>();

            if (interactorBody)
            {
                Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                {
                    subjectAsCharacterBody = interactorBody,
                    baseToken = "SHRINE_SWIFTNESS_USE_MESSAGE",
                });
            }

            var randomItemIndex = Main.itemIndexList[rng.RangeInt(0, Main.itemIndexList.Count)];

            var pickupIndex = PickupCatalog.FindPickupIndex(randomItemIndex);

            PickupDropletController.CreatePickupDroplet(pickupIndex, dropletOrigin.position, dropletOrigin.forward * 20f);

            EffectManager.SpawnEffect(Prefabs.shrineVFX, new EffectData
            {
                origin = base.transform.position,
                rotation = Quaternion.identity,
                scale = 1f,
                color = new Color32(130, 188, 232, 255)
            }, true);

            Util.PlaySound("Play_mage_m2_iceSpear_shoot", gameObject);
            Util.PlaySound("Play_ui_obj_lunarPool_activate", gameObject);

            purchaseCount++;
            refreshTimer = 2f;

            if (purchaseCount >= maxPurchaseCount)
            {
                symbolTransform.gameObject.SetActive(value: false);
                CallRpcSetPingable(value: false);
            }
        }

        public void FixedUpdate()
        {
            if (waitingForRefresh)
            {
                refreshTimer -= Time.fixedDeltaTime;
                if (refreshTimer <= 0f && purchaseCount < maxPurchaseCount)
                {
                    purchaseInteraction.SetAvailable(newAvailable: true);
                    purchaseInteraction.Networkcost = (int)((float)purchaseInteraction.cost + costMultiplierPerPurchase);
                    waitingForRefresh = false;
                }
            }
        }

        private void UNetVersion()
        { }

        public override bool OnSerialize(NetworkWriter writer, bool forceAll)
        {
            return base.OnSerialize(writer, forceAll);
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            base.OnDeserialize(reader, initialState);
        }

        public override void PreStartClient()
        {
            base.PreStartClient();
        }
    }
}