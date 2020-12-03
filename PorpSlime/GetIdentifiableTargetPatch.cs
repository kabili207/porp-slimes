using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PorpSlime
{
    [HarmonyPatch(typeof(TargetingUI), "GetIdentifiableTarget")]
    internal static class GetIdentifiableTargetPatch
    {
        public static void Postfix(TargetingUI __instance, bool __result, GameObject gameObject)
        {
            PorpSpawn spawn = gameObject.GetComponent<PorpSpawn>();
            SlimeEat eat = gameObject.GetComponent<SlimeEat>();
            Identifiable.Id id = Identifiable.GetId(gameObject);

            if (!__result || spawn is null || spawn.skin != PorpSpawn.Skin.Porp)
                return;
            __instance.nameText.text = "Porp " + __instance.nameText.text;

            if (eat != null && id != Identifiable.Id.PUDDLE_SLIME && id != Identifiable.Id.FIRE_SLIME)
                __instance.infoText.text = __instance.uiBundle.Xlate(MessageUtil.Compose("m.hudinfo_diet", new string[1]
                    {
                      eat.slimeDefinition.Diet.GetModulesFoodGroupsMsg()
                    }));
        }
    }
}
