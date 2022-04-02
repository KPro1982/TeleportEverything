using System.Collections.Generic;
using UnityEngine;

namespace TeleportEverything
{
    internal partial class Plugin
    {
        public static void GetCreatures()
        {
            var creatures = new List<Character>();
            Character.GetCharactersInRange(Player.m_localPlayer.transform.position,
                SearchRadius.Value, creatures);
            allies = GetAllies(creatures);
            enemies = GetEnemies(creatures);
        }

       public static bool IsValidEnemy(Character c)
        {
            if (c.GetComponent<BaseAI>() != null &&
                c.GetComponent<BaseAI>().IsEnemey(Player.m_localPlayer) && !c.IsTamed())
            {
                return true;
            }

            return false;
        }

        public static List<Character> GetEnemies(List<Character> creatures)
        {
            var chars = new List<Character>();
            foreach (var c in creatures)
            {
                if (IsValidEnemy(c))
                {
                    chars.Add(c);
                }
            }
            return chars;
        }

        public static float CalcDistToEntity(Character e) => VectorToEntity(e).magnitude;

        public static Vector3 VectorToEntity(Character e) =>
            e.transform.position - Player.m_localPlayer.transform.position;

        public static float HorizontalDistance(Character e)
        {
            var v3 = VectorToEntity(e);
            var v2 = new Vector2(v3.x, v3.z);
            return v2.magnitude;
        }

        public static float VerticalDistance(Character e) => Mathf.Abs(VectorToEntity(e).y);

        public static void DisplayMessage(string msg)
        {
            if (MessageMode.Value.Equals("top left"))
            {
                MessageHud.instance.ShowMessage(MessageHud.MessageType.TopLeft, msg);
            }
            else if (MessageMode.Value.Equals("centered"))
            {
                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, msg);
            }
        }

        public static void TeleportCreatures(Player player, List<Character> creatures, bool hasEnemies=false)
        {
            AlliesSpawn = new();
            EnemiesSpawn = new();

            float addDelay = 0f;
            foreach (Character c in creatures)
            {
                TakeOwnership(c, ZDOMan.instance.GetMyID());

                var destination = player.m_teleportTargetPos;
                var rotation = player.m_teleportTargetRot;
                Vector3 forward = player.m_teleportTargetRot * Vector3.forward;
                Vector3 offset = forward * SpawnForwardOffset.Value;

                if (hasEnemies)
                {
                    offset = forward * SpawnEnemiesForwardOffset.Value;
                    Vector2 circle = Random.insideUnitCircle * (enemies.Count * MaximumDisplacement.Value);
                    destination += new Vector3(circle.x, 0, circle.y); ;

                    float distDelay = HorizontalDistance(c) / 10f + 10;  // assume mobs can run at 10m/s
                    TeleportEverythingLogger.LogInfo($"{GetPrefabName(c)} will charge the gate in {distDelay} seconds");
                    EnemiesSpawn.Add(new DelayedSpawn(c, false, distDelay, destination + offset, rotation, false));
                }
                else {
                    AlliesSpawn.Add(new DelayedSpawn(c, true, ALLIES_SPAWN_DELAY + addDelay, destination + offset, rotation, IsFollowing(c)));
                    addDelay += 0.8f;
                }              
            }
        }

        public static void TakeOwnership(Character c, long userId)
        {
            if (c.GetComponent<ZNetView>() is { } netView)
            {
                if (c.GetOwner() != userId)
                {
                    netView.GetZDO()?.SetOwner(userId);
                }
            }
        }

        static void SetPosition(Character c, Vector3 destination, Quaternion rotation, Vector3 forward, bool hasEnemies)
        {
            Vector3 offset = forward * SpawnForwardOffset.Value;

            if (hasEnemies)
            {
                offset = forward * SpawnEnemiesForwardOffset.Value;
                Vector2 circle = Random.insideUnitCircle * (enemies.Count * MaximumDisplacement.Value);
                destination += new Vector3(circle.x, 0, circle.y); ;
            }

            c.transform.position = destination + offset;
            c.transform.rotation = rotation;
            c.SetLookDir(c.transform.position);
        }

        public static void CreateDelayedSpawn()
        {
            if (AlliesSpawn != null)
            {
                foreach (DelayedSpawn ds in AlliesSpawn)
                {
                    delayedAction.InvokeDelayed(ds.SpawnNow, ds.delay);
                }
            }

            if (EnemiesSpawn != null)
            {
                foreach (DelayedSpawn ds in EnemiesSpawn)
                {
                    delayedAction.InvokeDelayed(ds.SpawnNow, ds.delay);
                }
            }
        }
    }
}