using System;
using System.Collections;
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
        public Character Original { get; set; }
        public float delay { get; set; }
        public Vector3 Pos { get; set; }
        public Quaternion Rot { get; set; }
        public bool Follow { get; set; }

        public bool Ally { get; set; }

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
            Offset = _offset;
            Original = _original;
            saveZDO = Original.m_nview.GetZDO().Clone();
            
            
              Destroy(_original);
        }

        private void Destroy(Character orig)
        {
            //orig.transform.position *= 1000f;  // Kludge 

            ZNetScene.instance.Destroy(orig.gameObject);  
            // Each of the strategies below result in complications.
            // Object.Destroy(orig.gameObject);
            // orig.m_nview.Destroy();
        }

       
        public ZDO GetZdo()
        {
            saveZDO.Initialize(ZDOMan.instance, saveZDO.m_uid, Pos);
            saveZDO.m_owner = ZDOMan.instance.m_myid;
            //saveZDO.m_timeCreated = ZNet.instance.GetTime().Ticks;
            
            if(!ZDOMan.instance.m_objectsByID.TryGetValue(saveZDO.m_uid, out var zdo))
            {
                ZDOMan.instance.m_objectsByID.Add(saveZDO.m_uid, saveZDO);
            }

            return saveZDO;
        }

        public void SpawnNow()
        {
            if (spawned)
                return;

            spawned = true;
            ZDO zdo = GetZdo();
            
            if(zdo == null || !zdo.IsValid())
            {
                Debug.Log("Warning zdo = null or invalid in SpawnNow");
                return;
            } 

            GameObject clone = ZNetScene.instance.CreateObject(zdo);
            Debug.Log($"Spawning {clone.gameObject.name}");

            Tameable tame = clone.gameObject.GetComponent<Tameable>();
            if (tame != null)
            {
                tame.m_monsterAI.m_follow =
                    Player.m_localPlayer.gameObject;
            }       
        }

        //spawn now is called by a coroutine
        //public void TrySpawn(float delayT)
        //{
        //    if (!spawned && delayT - CreationTime > delay)
        //    {
        //        spawned = true;
        //        MessageHud.instance.ShowMessage(MessageHud.MessageType.TopLeft,
        //            $"Attempting to spawn");
        //        SpawnNow();
        //    }
        //}
        
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