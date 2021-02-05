using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PorpSlime.Patches
{
    [HarmonyPatch(typeof(WeaponVacuum))]
    [HarmonyPatch(nameof(WeaponVacuum.ConsumeVacItem))]
    public static class ConsumeVacItemPatch
    {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = new List<CodeInstruction>(instructions);

            int index = -1;

            for (var i = 0; i < codes.Count - 1; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt &&
                    ((MethodInfo)codes[i].operand).DeclaringType == typeof(GroundVine) &&
                    ((MethodInfo)codes[i].operand).Name == "Release")
                {
                    index = i;
                    break;
                }
            }

            if (index >= 0)
            {
                codes.InsertRange(index, new[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0), // this
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WeaponVacuum), nameof(WeaponVacuum.pediaDir))),
                    new CodeInstruction(OpCodes.Ldarg_1), // Local gameObj
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ConsumeVacItemPatch), nameof(CheckPedia)))
                });
            }
            return codes;
        }

        static void CheckPedia(PediaDirector pediaDir, GameObject gameObj)
        {
            var porp = gameObj.GetComponent<PorpSpawn>();

            if (porp != null && porp.skin == PorpSpawn.Skin.Porp)
            {
                pediaDir.MaybeShowPopup(Id.PORP_SLIMES);
            }
        }
    }

}
