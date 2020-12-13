using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PorpSlime
{
    [HarmonyPatch(typeof(Incinerate), "CanBeIncinerated")]
    class IncineratePatch
    {
        public static void Postfix(Identifiable ident, ref bool __result)
        {
            __result &= ident.id != PorpId.PORP_PLORT;
        }
    }
}
