using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TeleportEverything
{
    internal partial class Plugin
    {
        public static string GetPrefabName(Character c) => c.name.Replace("(Clone)", "");
        public static List<Regex> CommaSeparatedStringToList(string mask) => 
            mask.Split(',').Select(p => new Regex("\\b"+p.Trim().ToLower()+"\\b", RegexOptions.IgnoreCase)).ToList();

        private static bool IsInMask(string prefabName, string mask)

        {
            if (string.IsNullOrWhiteSpace(mask))
            {
                return false;
            }

            List<Regex> maskList = CommaSeparatedStringToList(mask);
            var isInMask = maskList.FirstOrDefault(name => name.IsMatch(prefabName.ToLower()));

            return isInMask != null;
        }

        public static void GetCreatures()
        {
            var creatures = new List<Character>();
            if (SearchRadius != null)
                Character.GetCharactersInRange(Player.m_localPlayer.transform.position, SearchRadius.Value, creatures);
            Allies = GetAllies(creatures);
            Enemies = GetEnemies(creatures);
        }

       public static bool IsValidEnemy(Character c)
        {
            if (c.GetComponent<MonsterAI>()?.IsAlerted() == false) return false;
            return BaseAI.IsEnemy(Player.m_localPlayer, c) && !c.IsTamed();
        }

        public static List<Character> GetEnemies(List<Character> creatures)
        {
            var chars = new List<Character>();
            foreach (var c in creatures)
            {
                if (c.GetBaseAI() != null)
                {
                    if (IsValidEnemy(c) && IsEnemyAllowedInMask(c))
                    {
                        chars.Add(c);
                    }
                }
            }
            return chars;
        }

        public static bool IsEnemyAllowedInMask(Character c)
        {
            if (EnemiesMaskMode is null || string.IsNullOrEmpty(EnemiesMaskMode.Value)) return true;
            if (EnemiesTransportMask is null || string.IsNullOrEmpty(EnemiesTransportMask.Value)) return true;

            switch (EnemiesMaskMode.Value.ToLower())
            {
                case "block":
                    if (IsInMask(GetPrefabName(c), EnemiesTransportMask.Value)) return false;
                    break;
                case "allow only":
                    if (!IsInMask(GetPrefabName(c), EnemiesTransportMask.Value)) return false;
                    break;
            }

            return true;
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

        public static void DisplayAlliesMessage()
        {
            if (TransportAllies && Allies?.Count > 0)
            {
                DisplayMessage(Localization.instance.Localize("$te_transporting_allies_message", Allies.Count.ToString()));
            }
        }

        public static void DisplayEnemiesMessage()
        {
            if (TeleportMode != null && Enemies?.Count > 0 && TeleportMode.Value.Contains("Take"))
            {
                DisplayMessage(Localization.instance.Localize("$te_transporting_enemies_message", Enemies.Count.ToString()));
            }
        }

        private static int placedEnemies;
        public static void TeleportCreatures(Player player, List<Character> creatures, bool hasEnemies=false)
        {
            placedEnemies = 0;
            foreach (Character c in creatures)
            {
                c.m_nview.ClaimOwnership();
                SetPositionAttempt(c, player.m_teleportTargetPos, player.m_teleportTargetRot, hasEnemies);
            }

            if (placedEnemies > 0)
            {
                var placedEnemiesMessage = Localization.instance.Localize("$te_transported_enemies_message", placedEnemies.ToString());
                TeleportEverythingLogger.LogInfo(placedEnemiesMessage);
                DisplayMessage(placedEnemiesMessage);
            }
        }

        public static bool IsModEnabled()
        {
            return EnableMod?.Value == true;
        }

        private static void SetPositionAttempt(Character c, Vector3 destination, Quaternion playerRotation, bool hasEnemies)
        {
            var radius = EnemySpawnRadius?.Value ?? 3;
            var tries = 1;
            var offset = GetSpawnOffset(c, playerRotation, radius, hasEnemies);

            while (tries <= 5)
            {
                var newPosition = destination;
                newPosition += offset;
                newPosition += GetRandomLocation(radius, hasEnemies);
                if (!CharacterFits(newPosition, c, hasEnemies, out var placeAt))
                {
                    tries++;
                    continue;
                }

                newPosition.y = placeAt + UP_OFFSET;
                SetPosition(c, newPosition, playerRotation);
                c.SetLookDir(c.transform.position);
                if (hasEnemies)
                {
                    placedEnemies++;
                }

                break;
            }
        }

        private static Vector3 GetRandomLocation(int radius, bool hasEnemies)
        {
            var random = (hasEnemies) ? Random.insideUnitCircle * radius : Random.insideUnitCircle * 0.5f;
            return new Vector3(random.x, 0, random.y);
        }

        internal static Vector3 SetForwardOffset(Quaternion rot, float offsetValue)
        {
            return rot * Vector3.forward * offsetValue;
        }

        private static Vector3 GetSpawnOffset(Character c, Quaternion playerRotation, int radius, bool hasEnemies)
        {
            if (SpawnForwardOffset == null) return SetForwardOffset(playerRotation, 0f);
            var alliesOffset = SpawnForwardOffset.Value;
            if (!hasEnemies)
            {
                if (GetPrefabName(c).Equals(LOX, StringComparison.OrdinalIgnoreCase))
                {
                    alliesOffset = alliesOffset < 4 ? 4 : alliesOffset;
                }
            }

            if (SpawnEnemiesForwardOffset == null) return SetForwardOffset(playerRotation, 0f);
            var spawnForward = (hasEnemies) ? SpawnEnemiesForwardOffset.Value + radius / 2 : alliesOffset;
            return SetForwardOffset(playerRotation, spawnForward);
        }

        private static void SetPosition(Component c, Vector3 position, Quaternion rotation)
        {
            var transform = c.transform;
            transform.position = position;
            transform.rotation = rotation;
        }

        private static bool FoundFloor(Vector3 position, out float floorHeight)
        {
            if (ZoneSystem.instance.FindFloor(position, out var height))
            {
                floorHeight = height;
                return true;
            }

            floorHeight = 0f;
            return false;
        }

        private static bool FoundRoof(Vector3 p, out float height)
        {
            if (Physics.Raycast(p, Vector3.up, out var hitInfo, 1000f, ZoneSystem.instance.m_blockRayMask))
            {
                height = hitInfo.point.y;
                return true;
            }

            height = 0f;
            return false;
        }

        private static bool CharacterFits(Vector3 position, Character c, bool hasEnemies, out float placeAt)
        {
            placeAt = 0f;

            if (!FoundFloor(position, out var floorHeight))
            {
                if(!ZoneSystem.instance.GetGroundHeight(new Vector3(position.x, position.y, position.z), out floorHeight))
                {
                    if (hasEnemies)
                    {
                        return false;
                    }

                    floorHeight = position.y;
                }
            }

            if (floorHeight - position.y > 20) return false;

            placeAt = floorHeight;
            if (!FoundRoof(new Vector3(position.x, floorHeight, position.z), out var height))
            {
                return true; 
            }
            var availableHeight = height - floorHeight;
            var characterHeight = GetCharacterHeight(c);

            return (characterHeight < availableHeight);
        }
        
        
        private static Vector3 FindBlocker(Vector3 p, Vector3 direction, Quaternion rotation, float maxDistance)
        {
            var point = p + rotation * Vector3.forward * maxDistance;
            if (!Physics.Raycast(p, direction, out var hitInfo, maxDistance, ZoneSystem.instance.m_blockRayMask))
                return point;
            
            point = hitInfo.point;
            return point;
        }

        private static float GetCharacterHeight(Character c)
        {
            return c.GetCollider().bounds.size.y;
        }

        private static bool IsFloatEqual(float first, float second)
        {
            return Math.Abs(first - second) < 0.01f;
        }
    }
}