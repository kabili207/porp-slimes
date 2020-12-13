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
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using IL = InlineIL.IL.Emit;
using System.CodeDom;
using SRML.SR.Templates;

namespace PorpSlime
{
    public class Main : ModEntryPoint
    {
        private static int topColorNameId = Shader.PropertyToID("_TopColor");
        private static int middleColorNameId = Shader.PropertyToID("_MiddleColor");
        private static int bottomColorNameId = Shader.PropertyToID("_BottomColor");

        private const string PORP_TOY_KEY = "t.porp_toy";
        private const string PORP_TOY_UI_KEY = "m.toy.name.t.porp_toy";
        private const string PORP_TOY_DESC_KEY = "m.toy.desc.t.porp_toy";

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

            TranslationPatcher.AddActorTranslation("l." + PorpId.PORP_SLIME.ToString().ToLower(), "Porp Slime");
            TranslationPatcher.AddActorTranslation("l." + PorpId.PORP_PLORT.ToString().ToLower(), "Porp Plort");
            new SlimePediaEntryTranslation(PorpId.PORP_SLIMES).SetTitleTranslation("Porp Slimes")
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
            PlortRegistry.AddPlortEntry(PorpId.PORP_PLORT, new[] { ProgressDirector.ProgressType.NONE });

            PediaRegistry.RegisterIdentifiableMapping(PediaDirector.Id.PLORTS, PorpId.PORP_PLORT);
            PediaRegistry.RegisterIdentifiableMapping(PorpId.PORP_SLIMES, PorpId.PORP_SLIME);
            PediaRegistry.SetPediaCategory(PorpId.PORP_SLIMES, PediaRegistry.PediaCategory.SLIMES);

            TranslationPatcher.AddTranslationKey("pedia", PORP_TOY_KEY, "Porp Lantern");
            TranslationPatcher.AddTranslationKey("pedia", PORP_TOY_UI_KEY, "Porp Lantern");
            TranslationPatcher.AddTranslationKey("pedia", PORP_TOY_DESC_KEY, "It's porple glow can calm even the angriest of porps.");

        }


        // Called before GameContext.Start
        // Used for registering things that require a loaded gamecontext
        public override void Load()
        {
            var pinkSlimeObject = SRSingleton<GameContext>.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(Identifiable.Id.TABBY_SLIME);

            SlimeDefinition porpDef = PrefabUtils.DeepCopyObject(pinkSlimeObject) as SlimeDefinition;
            porpDef.AppearancesDefault = new SlimeAppearance[1];
            porpDef.Diet.Produces = new Identifiable.Id[0];
            porpDef.Diet.MajorFoodGroups = new SlimeEat.FoodGroup[0];
            porpDef.Diet.AdditionalFoods = new Identifiable.Id[0];
            porpDef.Diet.Favorites = new Identifiable.Id[0];
            porpDef.Diet.EatMap?.Clear();
            porpDef.CanLargofy = false;
            porpDef.FavoriteToys = new Identifiable.Id[0];
            porpDef.Name = "Porp";
            porpDef.IdentifiableId = PorpId.PORP_SLIME;

            GameObject porpSlimeObject = PrefabUtils.CopyPrefab(SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(Identifiable.Id.TABBY_SLIME));
            porpSlimeObject.name = "porpSlime";
            porpSlimeObject.GetComponent<PlayWithToys>().slimeDefinition = porpDef;
            porpSlimeObject.GetComponent<SlimeAppearanceApplicator>().SlimeDefinition = porpDef;
            porpSlimeObject.GetComponent<SlimeEat>().slimeDefinition = porpDef;
            porpSlimeObject.GetComponent<Identifiable>().id = PorpId.PORP_SLIME;
            porpSlimeObject.AddComponent<ForcePorp>();

            SlimeAppearance porpSlimeAppearance = PrefabUtils.DeepCopyObject(pinkSlimeObject.AppearancesDefault.First()) as SlimeAppearance;
            porpDef.AppearancesDefault[0] = porpSlimeAppearance;

            UnityObject.Destroy(porpSlimeObject.GetComponent<MaybeCullOnReenable>());
            UnityObject.Destroy(porpSlimeObject.GetComponentInChildren<SlimeEatTrigger>());
            LookupRegistry.RegisterIdentifiablePrefab(porpSlimeObject);
            SlimeRegistry.RegisterSlimeDefinition(porpDef);

            GameObject firePlortObject = SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(Identifiable.Id.PINK_PLORT);
            Material firePlortMaterial = firePlortObject.GetComponentInChildren<MeshRenderer>().material;

            GameObject porpPlortObject = PrefabUtils.CopyPrefab(firePlortObject);
            porpPlortObject.GetComponent<Identifiable>().id = PorpId.PORP_PLORT;
            porpPlortObject.name = "porpPlort";
            UnityObject.DestroyImmediate(porpPlortObject.GetComponent<DestroyOnIgnite>());

            Material porpPlortMaterial = UnityObject.Instantiate(firePlortMaterial);
            porpPlortMaterial.SetColor(topColorNameId, porpColorMid);
            porpPlortMaterial.SetColor(middleColorNameId, porpColorTop);
            porpPlortMaterial.SetColor(bottomColorNameId, porpColorMid);
            porpPlortObject.GetComponentInChildren<MeshRenderer>().material = porpPlortMaterial;

            AssetBundle assetBundle = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(Main), "porp"));

            LookupRegistry.RegisterIdentifiablePrefab(porpPlortObject);

            RegisterFullVaccable(PorpId.PORP_SLIME, porpColor, GetSprite(assetBundle, "iconSlimePorp"));
            RegisterFullVaccable(PorpId.PORP_PLORT, porpColor, GetSprite(assetBundle, "iconPlortPorp"));

            PediaRegistry.RegisterIdEntry(PorpId.PORP_SLIMES, GetSprite(assetBundle, "iconSlimePorp"));
            PlortRegistry.AddEconomyEntry(PorpId.PORP_PLORT, 600f, 999f);
            DroneRegistry.RegisterBasicTarget(PorpId.PORP_PLORT);

            /*
            GameObject gameObject7 = PrefabUtils.CopyPrefab(SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(Identifiable.Id.NIGHT_LIGHT_TOY));
            gameObject7.AddComponent<Identifiable>().id = PorpId.PORP_LANTERN;
            //UnityObject.DestroyImmediate(gameObject7.GetComponent<GadgetPortableSlimeBaitPlaySoundOnHit>());
            //gameObject7.layer = LayerMask.NameToLayer("Actor");
            //gameObject7.transform.localScale *= 0.6f;
            gameObject7.AddComponent<RegionMember>();
            gameObject7.AddComponent<DragFloatReactor>();
            gameObject7.AddComponent<Vacuumable>().size = Vacuumable.Size.LARGE;
            gameObject7.AddComponent<CollisionAggregator>();
            GameObject gameObject8 = new GameObject();
            gameObject8.transform.parent = gameObject7.transform;
            gameObject8.AddComponent<VacDelaunchTrigger>();
            gameObject8.AddComponent<SphereCollider>().radius = gameObject7.GetComponent<SphereCollider>().radius;
            gameObject8.GetComponent<SphereCollider>().isTrigger = true;
            gameObject8.AddComponent<ToyProximityTrigger>();
            ToyDefinition toyDefinition = CreateObject<ToyDefinition>(new
            {
                cost = 900,
                icon = GetSprite(assetBundle, "iconSlimePorp"),
                nameKey = "porp_toy",
                toyId = PorpId.PORP_LANTERN
            });
            LookupRegistry.RegisterIdentifiablePrefab(gameObject7);
            LookupRegistry.RegisterToy(toyDefinition);*/
            //var UPGRADED_TOYS = (List<Identifiable.Id>)(typeof(ToyDirector).GetField("UPGRADED_TOYS", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));
            //UPGRADED_TOYS.Add(PorpId.PORP_LANTERN);
        }

        private static T CreateObject<T>(object anon) where T : ScriptableObject
        {
            Type objType = typeof(T);
            T createdObject = (T)Activator.CreateInstance(objType);
            var properties = anon.GetType().GetProperties();
            foreach (var property in properties)
            {
                var field = objType.GetField(property.Name, BindingFlags.NonPublic);
                field.SetValue(createdObject, property.GetValue(anon));
            }
            return createdObject;
        }

        // Called after all mods Load's have been called
        // Used for editing existing assets in the game, not a registry step
        public override void PostLoad()
        {
            foreach (GameObject identifiablePrefab in SRSingleton<GameContext>.Instance.LookupDirector.identifiablePrefabs)
            {
                var id = Identifiable.GetId(identifiablePrefab);
                if (Identifiable.IsSlime(id) && id != PorpId.PORP_SLIME)
                    identifiablePrefab.AddComponent<PorpSpawn>();
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
