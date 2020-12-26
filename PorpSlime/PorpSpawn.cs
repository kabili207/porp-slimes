using HarmonyLib;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using SRML.SR.SaveSystem;
using SRML.SR.SaveSystem.Data;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PorpSlime
{
    public class PorpSpawn : RegisteredActorBehaviour, ExtendedData.Participant
    {
        internal const float PROBABILITY_PORP = 0.005f;
        private static int topColorNameId = Shader.PropertyToID("_TopColor");
        private static int middleColorNameId = Shader.PropertyToID("_MiddleColor");
        private static int bottomColorNameId = Shader.PropertyToID("_BottomColor");

        private static readonly Color colorPorpTop = new Color(0.671f, 0.165f, 0.965f);
        private static readonly Color colorPorpMid = new Color(0.40f, 0.04f, 0.592f);
        private static readonly Color colorPorpBot = new Color(0.114f, 0.008f, 0.176f);

        public Skin skin;
        private SlimeAppearanceApplicator slimeAppearanceApplicator;
        public static List<Identifiable.Id> slimes = new List<Identifiable.Id>();
        public static List<Color> colorTop = new List<Color>();
        public static List<Color> colorMiddle = new List<Color>();
        public static List<Color> colorBottom = new List<Color>();

        internal Color curTop;
        internal Color curColor;
        internal Color curMid;
        internal Color curBot;
        internal Color curspecular;
        internal Color curFlameOut = Color.black;
        internal Color curFlamein = Color.black;
        internal Color curFireGlow = Color.black;
        internal Color curRadInner = Color.black;
        internal Color curRadOuter = Color.black;
        internal Color testtop = Color.red;
        internal Color testmid = Color.blue;
        internal Color testbot = Color.green;
        private bool didGetSet;

        private static SlimeDiet _porpDiet = null;
        private static SlimeDefinition _porpSlimeDef = new SlimeDefinition()
        {
            CanLargofy = false,
            IsLargo = false
        };
        private SlimeEat slimeEat;
        private GotoConsumable consumable;
        private RegionMember regionMember;

        public enum Skin
        {
            Normal,
            Porp,
        }

        public override void Start()
        {
            slimeEat = GetComponent<SlimeEat>();
            consumable = GetComponent<GotoConsumable>();
            regionMember = GetComponent<RegionMember>();
            UpdateSkin();
        }

        private void UpdateSkin()
        {
            if (!didGetSet && !Identifiable.IsLargo(Identifiable.GetId(gameObject)) && Identifiable.GetId(gameObject) != Identifiable.Id.TARR_SLIME)
            {
                skin = ShinyCheck();
            }

            switch (skin)
            {
                case Skin.Porp:
                    Porpify();
                    break;
            }
        }

        public void SetAll(PorpSpawn other)
        {
            Debug.Log("SetAll called");
            skin = other.skin;
            didGetSet = other.didGetSet;
            UpdateSkin();
        }

        public void SetSkin(Skin skin)
        {
            this.skin = skin;
            didGetSet = true;
            UpdateSkin();
        }

        public void ReadData(CompoundDataPiece piece)
        {
            curTop = piece.GetValue<Color>("top");
            curMid = piece.GetValue<Color>("middle");
            curBot = piece.GetValue<Color>("bottom");
            skin = piece.GetValue<Skin>("skin");
            didGetSet = true;
        }

        public void WriteData(CompoundDataPiece piece)
        {
            piece.SetValue("top", curTop);
            piece.SetValue("middle", curMid);
            piece.SetValue("bottom", curBot);
            piece.SetValue("skin", skin);
        }

        public void SetStyleAppearances()
        {
        }

        private static void EnsurePorpDiet()
        {
            if (_porpDiet is null)
            {
                _porpDiet = new SlimeDiet()
                {
                    Produces = new[] { Id.PORP_PLORT },
                    MajorFoodGroups = new[] { SlimeEat.FoodGroup.MEAT },
                    AdditionalFoods = new Identifiable.Id[0],
                    Favorites = new[] { Identifiable.Id.HEN },
                    FavoriteProductionCount = 2,
                };
                _porpSlimeDef.Diet = _porpDiet;
                _porpDiet.RefreshEatMap(SRSingleton<GameContext>.Instance.SlimeDefinitions, _porpSlimeDef);
            }
        }

        private Skin ShinyCheck()
        {
            Identifiable.Id id = Identifiable.GetId(gameObject);

            if (regionMember.IsInZone(ZoneDirector.Zone.QUARRY) && Randoms.SHARED.GetProbability(PROBABILITY_PORP))
                return Skin.Porp;
            return Skin.Normal;
        }

        public void SetColors() => SetColors(curTop, curMid, curBot, curFireGlow, curFlamein, curFlameOut, curRadInner, curRadOuter, curspecular);

        public void SetColors(Color top,  Color middle, Color bottom, Color fireGlow, Color flameInner, Color flameOuter,
            Color radAurain, Color radAuraOut, Color specular)
        {
            foreach (Renderer componentsInChild in GetComponentsInChildren<Renderer>(true))
            {
                Material material = componentsInChild.material;
                material.SetColor(topColorNameId, top);
                material.SetColor(middleColorNameId, middle);
                material.SetColor(bottomColorNameId, bottom);

                switch (Identifiable.GetId(gameObject))
                {
                    case Identifiable.Id.RAD_SLIME:
                        material.SetColor("_Color", radAurain);
                        material.SetColor("_EdgeColor", radAuraOut);
                        break;
                    case Identifiable.Id.HONEY_SLIME:
                        material.SetColor("_Color", Color.black);
                        break;
                    case Identifiable.Id.CRYSTAL_SLIME:
                        material.SetColor("_RimColor", Color.yellow);
                        material.SetColor("_SpecularColor", specular);
                        break;
                    case Identifiable.Id.FIRE_SLIME:
                        material.SetColor("_ColorInside", flameInner);
                        material.SetColor("_ColorOutside", flameOuter);
                        material.SetColor("_CrackColor", fireGlow);
                        break;
                }
            }
        }

        public void SetDiet()
        {
            // We need to make sure we can override the food preferences
            if (slimeEat != null && skin == Skin.Porp)
            {
                EnsurePorpDiet();
                slimeEat.slimeDefinition = _porpSlimeDef;
                slimeEat.InitFood();
                if (consumable != null)
                    consumable.UpdateSearchIds();
            }
        }

        public void Porpify()
        {
            SetDiet();
            switch (Identifiable.GetId(gameObject))
            {
                case Identifiable.Id.RAD_SLIME:
                    curRadInner = new Color(0.86f, 0.35f, 0.99f, 1f);
                    curRadOuter = new Color(0.86f, 0.35f, 0.99f, 1f);
                    goto default;
                case Identifiable.Id.CRYSTAL_SLIME:
                    curspecular = Color.black;
                    goto default;
                case Identifiable.Id.FIRE_SLIME:
                    curFireGlow = new Color(0.15f, 0.89f, 0.97f, 1f);
                    curFlamein = new Color(0.0f, 0.3f, 1f, 1f);
                    curFlameOut = new Color(0.1f, 0.42f, 1f, 1f);
                    goto default;
                case Identifiable.Id.PINK_SLIME:
                case Identifiable.Id.BOOM_SLIME:
                case Identifiable.Id.TABBY_SLIME:
                case Identifiable.Id.HUNTER_SLIME:
                case Identifiable.Id.PUDDLE_SLIME:
                case Identifiable.Id.QUANTUM_SLIME:
                case Identifiable.Id.DERVISH_SLIME:
                case Identifiable.Id.MOSAIC_SLIME:
                case Identifiable.Id.TANGLE_SLIME:
                case Identifiable.Id.QUICKSILVER_SLIME:
                default:
                    curTop = colorPorpTop;
                    curMid = colorPorpMid;
                    curBot = colorPorpBot;
                    SetColors();
                    break;

            }
        }

        [HarmonyPatch(typeof(SlimeEat))]
        [HarmonyPatch("EatAndTransform")]
        public static class PorpPersistance
        {

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                var codes = new List<CodeInstruction>(instructions);

                int index = -1;

                for (var i = 0; i < codes.Count - 1; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldloc_1 && codes[i + 1].opcode == OpCodes.Callvirt &&
                        ((MethodInfo)codes[i + 1].operand).Name == "GetComponent" &&
                         ((MethodInfo)codes[i + 1].operand).GetGenericArguments().SequenceEqual(new[] { typeof(OnTransformed) }))
                    {
                        index = i;
                        break;
                    }
                }

                codes.InsertRange(index, new[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0), // this
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Component), "get_gameObject")),
                    new CodeInstruction(OpCodes.Ldloc_1), // Local gameObject
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PorpPersistance), nameof(CopyPorp)))
                });

                return codes;
            }
            static void CopyPorp(GameObject orig, GameObject newObj)
            {
                var origPorp = orig.GetComponent<PorpSpawn>();
                var newPorp = newObj.GetComponent<PorpSpawn>();

                if (origPorp != null && newPorp != null)
                {
                    newPorp.SetAll(origPorp);
                }
            }
        }
    }
}
