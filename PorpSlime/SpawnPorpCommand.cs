using MonomiPark.SlimeRancher.Regions;
using SRML.Console.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PorpSlime
{
    public class SpawnPorpCommand : SpawnCommand
    {
        public override string ID => "spawnporp";

        public override string Usage => "spawnporp <id> [count]";

        public override string Description => "Spawns slime with 600% more porp";

        protected override bool ArgsOutOfBounds(int argCount, int min = 0, int max = 0) => base.ArgsOutOfBounds(argCount, min, max);

        public override bool Execute(string[] args)
        {
            if (args.Length < 1 || args.Length > 2)
            {
                SRML.Console.Console.LogError("Incorrect number of arguments!", true);
                return false;
            }
            Identifiable.Id id;
            GameObject prefab;
            try
            {
                id = (Identifiable.Id)Enum.Parse(typeof(Identifiable.Id), args[0], true);
                prefab = SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(id);
            }
            catch
            {
                SRML.Console.Console.LogError("Invalid ID!", true);
                return false;
            }
            if (!Identifiable.IsSlime(id))
            {
                SRML.Console.Console.LogError("Invalid ID!", true);
                return false;
            }
            PorpSpawn.Skin skin = PorpSpawn.Skin.Porp;
            int result;

            if (args.Length != 2 || !int.TryParse(args[1], out result))
                result = 1;

            for (int index = 0; index < result; ++index)
            {
                RaycastHit hitInfo;
                if (Physics.Raycast(new Ray(Camera.main.transform.position, Camera.main.transform.forward), out hitInfo))
                {
                    GameObject gameObject = SRBehaviour.InstantiateActor(prefab, SRSingleton<SceneContext>.Instance.Player.GetComponent<RegionMember>().setId, true);
                    gameObject.transform.position = hitInfo.point + hitInfo.normal * PhysicsUtil.CalcRad(gameObject.GetComponent<Collider>());
                    Vector3 vector3_1 = hitInfo.point - Camera.main.transform.position;
                    Vector3 vector3_2 = -vector3_1.normalized;
                    vector3_1 = vector3_2 - Vector3.Project(vector3_2, hitInfo.normal);
                    Vector3 normalized = vector3_1.normalized;
                    gameObject.transform.rotation = Quaternion.LookRotation(vector3_2, hitInfo.normal);
                    gameObject.GetComponent<PorpSpawn>().SetSkin(skin);
                }
            }
            return true;
        }

       }
}
