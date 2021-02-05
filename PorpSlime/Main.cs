using HarmonyLib;
using MonomiPark.SlimeRancher.Regions;
using SRML;
using SRML.Console;
using SRML.SR;
using SRML.SR.SaveSystem;
using SRML.SR.Translation;
using SRML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace PorpSlime
{
    public class Main : ModEntryPoint
    {
        private static int topColorNameId = Shader.PropertyToID("_TopColor");
        private static int middleColorNameId = Shader.PropertyToID("_MiddleColor");
        private static int bottomColorNameId = Shader.PropertyToID("_BottomColor");

        private const string PORP_TOY_KEY = "t.trans_toy";
        private const string PORP_TOY_UI_KEY = "m.toy.name.t.trans_toy";
        private const string PORP_TOY_DESC_KEY = "m.toy.desc.t.trans_toy";

        private static readonly Color porpColor = new Color(0.40f, 0.04f, 0.592f);

        private static readonly Color porpColorTop = new Color(0.671f, 0.165f, 0.965f);
        private static readonly Color porpColorMid = new Color(0.40f, 0.04f, 0.592f);
        private static readonly Color porpColorBot = new Color(0.114f, 0.008f, 0.176f);

        // Called before GameContext.Awake
        // You want to register new things and enum values here, as well as do all your harmony patching
        public override void PreLoad()
        {
            HarmonyInstance.PatchAll();

            SaveRegistry.RegisterDataParticipant<PorpSpawn>();
            //SystemContext.IsModded = true;
            SRML.Console.Console.RegisterCommand(new SpawnPorpCommand());

            TranslationPatcher.AddActorTranslation("l." + Id.PORP_PLORT.ToString().ToLower(), "Porp Plort");
            TranslationPatcher.AddActorTranslation("l." + Id.TRANS_BALL.ToString().ToLower(), "Colorful Ball");
            TranslationPatcher.AddActorTranslation("l." + Id.PASTEL_POTION.ToString().ToLower(), "Pastel Potion");
            new SlimePediaEntryTranslation(Id.PORP_SLIMES).SetTitleTranslation("Porp Slimes")
                .SetIntroTranslation("More porp than your body has room for!")
                .SetSlimeologyTranslation(
                    "Porp porp porple porp porp porp porp porp porp porp porp porp porple. " +
                    "Porple porple porp. Porp porp porple porp porp porp porple porp porpin'" +
                    "porp porple porp.")
                .SetDietTranslation("All da meats")
                .SetFavoriteTranslation("Hen Hens")
                .SetRisksTranslation(
                    "This is a LOT of porp to deal with. Unexperienced ranchers may quickly find themselves " +
                    "overwhelmed with all of the porp.")
                .SetPlortonomicsTranslation(
                    "Look, it's porp. Everyone wants porp. Do I really need to say more?");
            PlortRegistry.AddPlortEntry(Id.PORP_PLORT, new[] { ProgressDirector.ProgressType.NONE });

            PediaRegistry.RegisterIdentifiableMapping(PediaDirector.Id.PLORTS, Id.PORP_PLORT);
            PediaRegistry.SetPediaCategory(Id.PORP_SLIMES, PediaRegistry.PediaCategory.SLIMES);

            TranslationPatcher.AddTranslationKey("pedia", PORP_TOY_KEY, "Colorful Ball");
            TranslationPatcher.AddTranslationKey("pedia", PORP_TOY_UI_KEY, "Colorful Ball");
            TranslationPatcher.AddTranslationKey("pedia", PORP_TOY_DESC_KEY, "It's unknown why, but porp slimes absolutely love the pastel blues and pinks colorful ball.");

        }


        // Called before GameContext.Start
        // Used for registering things that require a loaded gamecontext
        public override void Load()
        {
            GameObject firePlortObject = SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(Identifiable.Id.PINK_PLORT);
            Material firePlortMaterial = firePlortObject.GetComponentInChildren<MeshRenderer>().material;

            GameObject porpPlortObject = PrefabUtils.CopyPrefab(firePlortObject);
            porpPlortObject.GetComponent<Identifiable>().id = Id.PORP_PLORT;
            porpPlortObject.name = "porpPlort";
            UnityObject.DestroyImmediate(porpPlortObject.GetComponent<DestroyOnIgnite>());

            Material porpPlortMaterial = UnityObject.Instantiate(firePlortMaterial);
            porpPlortMaterial.SetColor(topColorNameId, porpColorMid);
            porpPlortMaterial.SetColor(middleColorNameId, porpColorTop);
            porpPlortMaterial.SetColor(bottomColorNameId, porpColorMid);
            porpPlortObject.GetComponentInChildren<MeshRenderer>().material = porpPlortMaterial;

            AssetBundle assetBundle = GetAssetBundle("porp");

            LookupRegistry.RegisterIdentifiablePrefab(porpPlortObject);

            RegisterFullVaccable(Id.PORP_PLORT, porpColor, GetSprite(assetBundle, "iconPlortPorp"));

            PediaRegistry.RegisterIdEntry(Id.PORP_SLIMES, GetSprite(assetBundle, "iconSlimePorp"));
            PlortRegistry.AddEconomyEntry(Id.PORP_PLORT, 600f, 300f);
            DroneRegistry.RegisterBasicTarget(Id.PORP_PLORT);




            Material pastelPotionMat = assetBundle.LoadAsset<Material>("pastelPotionMat");
            //Material pastelPotionCapMat = assetBundle.LoadAsset<Material>("SlimeySolutionCapMat");

            GameObject b3 = PrefabUtils.CopyPrefab(SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(Identifiable.Id.PRIMORDY_OIL_CRAFT));
            b3.GetComponent<Identifiable>().id = Id.PASTEL_POTION;
            b3.name = "resourcePastelPotion";
            b3.transform.GetChild(0).GetChild(1).GetChild(0).gameObject.GetComponent<MeshRenderer>().material = pastelPotionMat;
            //b3.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = pastelPotionCapMat;
            LookupRegistry.RegisterIdentifiablePrefab(b3);
            RegisterFullVaccable(Id.PASTEL_POTION, porpColor, GetSprite(assetBundle, "trans_flag"));


            GameObject gameObject7 = PrefabUtils.CopyPrefab(SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(Identifiable.Id.BEACH_BALL_TOY));
            gameObject7.AddComponent<Identifiable>().id = Id.TRANS_BALL;
            gameObject7.name = "transToy";
            //UnityObject.DestroyImmediate(gameObject7.GetComponent<GadgetPortableSlimeBaitPlaySoundOnHit>());
            //gameObject7.layer = LayerMask.NameToLayer("Actor");
            //gameObject7.transform.localScale *= 0.6f;
            /*gameObject7.AddComponent<RegionMember>();
            gameObject7.AddComponent<DragFloatReactor>();
            gameObject7.AddComponent<Vacuumable>().size = Vacuumable.Size.LARGE;
            gameObject7.AddComponent<CollisionAggregator>();
            GameObject gameObject8 = new GameObject();
            gameObject8.transform.parent = gameObject7.transform;
            gameObject8.AddComponent<VacDelaunchTrigger>();
            gameObject8.AddComponent<SphereCollider>().radius = gameObject7.GetComponent<SphereCollider>().radius;
            gameObject8.GetComponent<SphereCollider>().isTrigger = true;
            gameObject8.AddComponent<ToyProximityTrigger>();*/
            ToyDefinition toyDefinition = new ToyDefinition()
            {
                cost = 900,
                icon = GetSprite(assetBundle, "trans_flag"),
                nameKey = "trans_toy",
                toyId = Id.TRANS_BALL
            };
            LookupRegistry.RegisterIdentifiablePrefab(gameObject7);
            LookupRegistry.RegisterToy(toyDefinition);
            LookupRegistry.RegisterVacEntry(Id.TRANS_BALL, porpColor, GetSprite(assetBundle, "trans_flag"));
            //ToyDirector.UPGRADED_TOYS.Add(PorpId.TRANS_BALL);
        }

        private static AssetBundle GetAssetBundle(string bundleName)
        {
            string resourceName;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                resourceName = $"UnityAssets.Windows.{bundleName}";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                resourceName = $"UnityAssets.Linux.{bundleName}";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                resourceName = $"UnityAssets.OSX.{bundleName}";
            }
            else
            {
                throw new Exception("Unsupported platform detected");
            }

            return AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(Main), resourceName));
        }

        // Called after all mods Load's have been called
        // Used for editing existing assets in the game, not a registry step
        public override void PostLoad()
        {
            foreach (SlimeDefinition slime in SRSingleton<GameContext>.Instance.SlimeDefinitions.Slimes)
            {
                var id = slime.IdentifiableId;

                slime.Diet.EatMap.Add(new SlimeDiet.EatMapEntry()
                {
                    eats = Identifiable.Id.INDIGONIUM_CRAFT,
                    becomesId = Identifiable.Id.NONE,
                    producesId = Identifiable.Id.NONE,
                    driver = SlimeEmotions.Emotion.AGITATION,
                    extraDrive = 1f,
                    isFavorite = false,
                    minDrive = 0.1f
                });

                if (Identifiable.IsSlime(id))
                    slime.BaseModule.AddComponent<PorpSpawn>();
            }
        }

        private Sprite GetSprite(AssetBundle ab, string name)
        {
            if (ab is null)
                throw new ArgumentNullException(nameof(ab));
            Sprite s = ab.LoadAsset<Sprite>(name);
            if (s is null)
                throw new ArgumentException($"Asset {name} is missing", nameof(name));
            return s;
        }

        public static void RegisterFullVaccable(Identifiable.Id id, Color color, Sprite icon)
        {
            AmmoRegistry.RegisterAmmoPrefab(PlayerState.AmmoMode.DEFAULT, SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(id));
            LookupRegistry.RegisterVacEntry(id, color, icon);
            SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(id).GetComponent<Vacuumable>().size = Vacuumable.Size.NORMAL;
            if (!Identifiable.SLIME_CLASS.Contains(id))
                return;
            SlimeAppearance slimeAppearance = SRSingleton<GameContext>.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(id).Appearances.First();
            slimeAppearance.Icon = icon;
            slimeAppearance.ColorPalette.Ammo = color;
        }

    }
}
