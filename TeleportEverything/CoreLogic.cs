using System.Collections.Generic;
using UnityEngine;

namespace TeleportEverything
{
    internal partial class Plugin
    {
        private static void PopulateEntityLists()
        {
            SetIncludeMode();

            allies.Clear();
            enemies.Clear();

            var characters = new List<Character>();
            Character.GetCharactersInRange(Player.m_localPlayer.transform.position,
                SearchRadius.Value, characters);


            foreach (var c in characters)
            {
                if (IsEligibleAlly(c) && IsAllyTransportable(c) && TransportAllies)
                {
                    if (HorizontalDistance(c) <= TransportRadius.Value &&
                        VerticalDistance(c) <= TransportVerticalTolerance.Value)
                    {
                        allies.Add(c);
                    }
                }

                if (c.GetComponent<BaseAI>() != null &&
                    c.GetComponent<BaseAI>().IsEnemey(Player.m_localPlayer) && !c.IsTamed())
                {
                    enemies.Add(c);
                }
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
    }
}