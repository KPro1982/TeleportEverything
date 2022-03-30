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
            return creatures.FindAll(IsValidEnemy);   
        }

        public static List<DelayedSpawn> EnemiesSpawn;
        public static void CreateEnemyList(Vector3 pos,
            Quaternion rot)
        {
            Vector3 offset = Player.m_localPlayer.transform.forward * SpawnForwardOffset.Value;
            
            foreach (Character c in enemies)
            {
                float distDelay = HorizontalDistance(c) / 10f + DEFAULT_DELAY + SpawnEnemiesDelay.Value;  // assume mobs can run at 10m/s
                TeleportEverythingLogger.LogInfo($"{GetPrefabName(c)} will charge the gate in {distDelay} seconds");
                EnemiesSpawn.Add(new DelayedSpawn(c,false, distDelay, GetDelayTimer(), pos, rot, offset, false));
            }   
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
        
        public static void UpdateDelayTimer(float dt)
        {
            DelayTimer += dt;

            if (teleportTriggered)
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
                teleportTriggered = false;
            }
        }

        public static float GetDelayTimer()
        {
            return DelayTimer;
        }
        public static void ResetDelayTimer()
        {
            // DelayTimer = 0f;
        }
    }
}