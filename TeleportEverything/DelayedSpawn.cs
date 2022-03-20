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
        
        public float CreationTime { get; set; }

        public int Version;
        public DelayedSpawn(Character _original, bool _ally, float _delay, float _creationTime, Vector3 _pos,
            Quaternion _rot, Vector3 _offset, bool _follow)
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
            SaveZDOToDisk();
            Destroy(_original);
            
            
        }

        private void Destroy(Character orig)
        {
            // ZNetView znv = orig.m_nview;
            // ZDO zdo = znv.GetZDO();
            // znv.ResetZDO();
            // UnityEngine.Object.Destroy(znv.gameObject);
            // ZDOMan.instance.DestroyZDO(zdo);
            ZNetScene.instance.Destroy(orig.gameObject);

        }

        private void SaveZDOToDisk()
        {
            
            Directory.CreateDirectory(Utils.GetSaveDataPath() + "/characters");
            string savename = Utils.GetSaveDataPath() + "/characters/ally.dat";

            if (File.Exists(savename))
            {
                File.Delete(savename);
            }

            ZPackage zpackage = new ZPackage();
            character.m_nview.GetZDO().Save(zpackage);
            
                
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

        private ZDO LoadZDOFromDisk()
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
        

        public void SpawnNow()
        {
           
            GameObject clone = null;
            ZDO zdo = LoadZDOFromDisk();
            Debug.Log($"Spawning {character.m_name}");
            if (zdo != null)
            {
             
                clone = ZNetScene.instance.CreateObject(zdo);
                
            }



            clone.transform.position = Pos + Offset;
            clone.transform.rotation = Rot;
            clone.gameObject.GetComponent<Tameable>().m_monsterAI.m_follow =
                Player.m_localPlayer.gameObject;
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
    }
}