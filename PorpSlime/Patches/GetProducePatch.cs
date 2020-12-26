using HarmonyLib;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PorpSlime.Patches
{
    // The version of Harmony that SRML uses doesn't support multiple prefix/postfix definitions in the same class,
    // So we make several nested classes instead

    class GetProducePatch
    {
        [HarmonyPatch(typeof(SlimeEat), "GetProducedIds", new[] { typeof(Identifiable.Id), typeof(List<Identifiable.Id>) })]
        class SlimeEat_GetProducedIds
        {
            public static void Postfix(SlimeEat __instance, List<Identifiable.Id> __result) => Do.Postfix(__instance, __result);
        }


        [HarmonyPatch(typeof(SlimeEatWater), "GetProducedIds", new[] { typeof(Identifiable.Id), typeof(List<Identifiable.Id>) })]
        class SlimeEatWater_GetProducedIds
        {
            public static void Postfix(SlimeEatWater __instance, List<Identifiable.Id> __result) => Do.Postfix(__instance, __result);

        }

        private static class Do
        {
            public static void Postfix(SRBehaviour __instance, List<Identifiable.Id> __result)
            {
                PorpSpawn spawn = __instance.GetComponent<PorpSpawn>();
                if (spawn is null || spawn.skin != PorpSpawn.Skin.Porp)
                    return;

                int num = __result != null ? __result.Count(new Func<Identifiable.Id, bool>(Identifiable.IsPlort)) : 0;
                __result.Clear();
                for (int index = 0; index < num; ++index)
                    __result.Add(Id.PORP_PLORT);
            }
        }
    }

    class ProducePatch
    {
        [HarmonyPatch(typeof(SlimeEat), "Produce")]
        class SlimeEat_Produce
        {
            public static void Prefix(SlimeEat __instance, ref GameObject produces) => Do.Prefix(__instance, ref produces);
        }

        [HarmonyPatch(typeof(SlimeEatWater), "Produce")]
        class SlimeEatWater_Produce
        {
            public static void Prefix(SlimeEatWater __instance, ref GameObject produces) => Do.Prefix(__instance, ref produces);
        }

        [HarmonyPatch(typeof(SlimeEatAsh), "ProduceAfterDelay")]
        class SlimeEatAsh_Produce
        {
            public static void Prefix(SlimeEatAsh __instance, ref GameObject produces) => Do.Prefix(__instance, ref produces);
        }

        private class Do
        {
            public static void Prefix(SRBehaviour __instance, ref GameObject produces)
            {
                PorpSpawn spawn = __instance.GetComponent<PorpSpawn>();
                Identifiable.Id id = Identifiable.GetId(produces);

                if (id == Id.PORP_PLORT || !Identifiable.IsPlort(id) || spawn is null || spawn.skin != PorpSpawn.Skin.Porp)
                    return;
                produces = SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(Id.PORP_PLORT);
            }
        }
    }



}
