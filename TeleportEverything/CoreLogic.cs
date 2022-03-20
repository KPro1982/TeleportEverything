using System.Collections.Generic;
using UnityEngine;

namespace TeleportEverything
{
    internal partial class Plugin
    {
       public static bool IsValidEnemy(Character c)
        {

            if (c.GetComponent<BaseAI>() != null &&
                c.GetComponent<BaseAI>().IsEnemey(Player.m_localPlayer) && !c.IsTamed())
            {
                return true;
            }

            return false;
        }
      
        public static int CountEnemies()
        {
            var characters = new List<Character>();
            Character.GetCharactersInRange(Player.m_localPlayer.transform.position,
                SearchRadius.Value, characters);

            return characters.FindAll(c => IsValidEnemy(c) == true).Count;
           
            
        }
        

        public static List<DelayedSpawn> GetEnemyList(Vector3 pos,
            Quaternion rot)
        {
            var characters = new List<Character>();
            var Enemies = new List<DelayedSpawn>();
            
            Character.GetCharactersInRange(Player.m_localPlayer.transform.position,
                SearchRadius.Value, characters);

            var characterList = characters.FindAll(c => IsValidEnemy(c) == true);
            Vector3 offset = Player.m_localPlayer.transform.forward * SpawnForwardOffset.Value;
            
            foreach (Character c in characterList)
            {
                Enemies.Add(new DelayedSpawn(c,false, 10f, GetDelayTimer(), pos, rot, offset, false));
            }

            return Enemies;
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
        
            if (Allies != null)
            {
                foreach (DelayedSpawn ds in Allies)
                {
                    ds.TrySpawn(DelayTimer);
                }
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