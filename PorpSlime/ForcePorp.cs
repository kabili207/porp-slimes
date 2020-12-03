
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using MonomiPark.SlimeRancher.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PorpSlime
{
    internal class ForcePorp : RegisteredActorBehaviour
    {
        private RegionMember regionMember;
        private SlimeEmotions emotions;
        private LookupDirector lookupDir;

        public override void Start()
        {
            base.Start();
            regionMember = GetComponent<RegionMember>();
            emotions = GetComponent<SlimeEmotions>();
            lookupDir = SRSingleton<GameContext>.Instance.LookupDirector;
            Transform(Identifiable.Id.TABBY_SLIME);
        }

        private void Transform(Identifiable.Id replacementId)
        {
            Identifiable.Id id = Identifiable.GetId(gameObject);
            var idOldName = Enum.GetName(typeof(Identifiable.Id), id);
            var idNewName = Enum.GetName(typeof(Identifiable.Id), replacementId);
            Debug.Log($"Forcing transformation from {idOldName} to PORP_{idNewName}");

            Destroyer.DestroyActor(gameObject, "PorpSlime.ForcePorp");
            GameObject replacement = InstantiateActor(lookupDir.GetPrefab(replacementId), regionMember.setId, transform.position, transform.rotation);
            SlimeEmotions component2 = replacement.GetComponent<SlimeEmotions>();
            if (component2 != null)
                component2.SetAll(emotions);
            PorpSpawn porp = replacement.GetComponent<PorpSpawn>();
            if (porp != null)
                porp.SetSkin(PorpSpawn.Skin.Porp);
            replacement.transform.DOScale(replacement.transform.localScale, 0.5f).From(gameObject.transform.localScale).SetEase(Ease.OutElastic);
            replacement.GetComponent<OnTransformed>()?.OnTransformed();
        }
    }
}
