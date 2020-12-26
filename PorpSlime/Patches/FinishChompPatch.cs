using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PorpSlime.Patches
{
    [HarmonyPatch(typeof(SlimeEat), "FinishChomp")]
    internal static class FinishChompPatch
    {
        public static void Prefix(SlimeEat __instance)
        {
            if (!__instance.chomper.IsChomping() || SlimeEat.claimedFood.Contains(__instance.chomper.metadata?.gameObject))
                return;
            Chomper.Metadata metadata = __instance.chomper.metadata;
            if (metadata == null || metadata.id != Identifiable.Id.INDIGONIUM_CRAFT)
                return;
            PorpSpawn porpSpawn = __instance.gameObject.GetComponent<PorpSpawn>() ?? __instance.gameObject.AddComponent<PorpSpawn>();
            if (Randoms.SHARED.GetProbability(0.3f))
                porpSpawn.SetSkin(PorpSpawn.Skin.Porp);
        }
    }
}
