using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using On.Steamworks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TeleportEverything
{
    public class DelayedSpawn
    {
        public Character character { get; set; }
        public float delay { get; set; }
        public Vector3 Pos { get; set; }
        public Quaternion Rot { get; set; }
        public bool Follow { get; set; }

        public bool Ally { get; set; }

        public Transform Transform { get; set; }

        public Vector3 Offset { get; set; }

        private bool spawned = false;

        private ZDO saveZDO;

        public float CreationTime { get; set; }

        public int Version;

        public DelayedSpawn(Character _original, bool _ally, float _delay, float _creationTime,
            Vector3 _pos, Quaternion _rot, Vector3 _offset, bool _follow)
        {
            delay = _delay;
            CreationTime = _creationTime;
            Pos = _pos;
            Rot = _rot;
            Follow = _follow;
            Ally = _ally;
            Transform = _original.transform;
            Offset = _offset;
            character = _original;
            saveZDO = character.m_nview.GetZDO().Clone();
            if (saveZDO == null)
            {
                Debug.Log("Warning: saveZDO is null in constructor");
            }
            else
            {
                Debug.Log($"Constructor: saveZDO IsPersistent = {saveZDO.m_persistent}");
            }
            
            // Destroy(_original);
        }

        private void Destroy(Character orig)
        {
            ZNetScene.instance.Destroy(orig.gameObject);
        }

       
        public ZDO GetZdo()
        {
           // ZDO zdo = ZDOMan.instance.CreateNewZDO(Pos);
            saveZDO.Initialize(ZDOMan.instance, saveZDO.m_uid, Pos);
            saveZDO.m_owner = ZDOMan.instance.m_myid;
            saveZDO.m_timeCreated = ZNet.instance.GetTime().Ticks;
            ZDOMan.instance.m_objectsByID.Remove(saveZDO.m_uid);
            ZDOMan.instance.m_objectsByID.Add(saveZDO.m_uid, saveZDO);
            
            return saveZDO;
        }

        public void SpawnNow()
        {
            GameObject clone = null;
            ZDO zdo = GetZdo();
            Debug.Log($"Spawning {character.m_name}");
            if (zdo != null)
            {
                clone = ZNetScene.instance.CreateObject(zdo);
            }
            else
            {
                Debug.Log("Warning zdo = null in SpawnNow");
            }
            
            //   clone.gameObject.GetComponent<Tameable>().m_monsterAI.m_follow =
            //       Player.m_localPlayer.gameObject;
        }


        public void TrySpawn(float delayT)
        {
            if (!spawned && delayT - CreationTime > delay)
            {
                spawned = true;
                MessageHud.instance.ShowMessage(MessageHud.MessageType.TopLeft,
                    $"Attempting to spawn");
                SpawnNow();
            }
        }
        
        private void SaveZdoToDisk(ZDO zdo)  // backup strategy
        {
            Directory.CreateDirectory(Utils.GetSaveDataPath() + "/characters");
            string savename = Utils.GetSaveDataPath() + "/characters/ally.dat";

            if (File.Exists(savename))
            {
                File.Delete(savename);
            }

            ZPackage zpackage = new ZPackage();
            zdo.Save(zpackage);


            byte[] array = zpackage.GenerateHash();
            byte[] array2 = zpackage.GetArray();
            FileStream fileStream = File.Create(savename);
            BinaryWriter binaryWriter = new BinaryWriter(fileStream);
            binaryWriter.Write(array2.Length);
            binaryWriter.Write(array2);
            binaryWriter.Write(array.Length);
            binaryWriter.Write(array);
            binaryWriter.Flush();
            fileStream.Flush(true);
            fileStream.Close();
            fileStream.Dispose();
        }

        private ZDO LoadZdoFromDisk()
        {
            string text = Utils.GetSaveDataPath() + "/characters/ally.dat";
            FileStream fileStream;
            try
            {
                fileStream = File.OpenRead(text);
            }
            catch
            {
                ZLog.Log("  failed to load " + text);
                return null;
            }

            byte[] data;
            try
            {
                BinaryReader binaryReader = new BinaryReader(fileStream);
                int num = binaryReader.ReadInt32();
                data = binaryReader.ReadBytes(num);
                int num2 = binaryReader.ReadInt32();
                binaryReader.ReadBytes(num2);
            }
            catch
            {
                fileStream.Dispose();
                return null;
            }

            fileStream.Dispose();

            ZDO zdo = ZDOMan.instance.CreateNewZDO(Pos);
            zdo.Load(new ZPackage(data), 24);
            return zdo;
        }

    }
}