using System;
using System.Dynamic;
using UnityEngine;
using Object = UnityEngine.Object;


namespace TeleportEverything
{
    public class InvokeHelper : MonoBehaviour
    {
        private void Awake()
        {
            
        }

        private void Start()
        {
            
        }

        private void InvSpawn(DelayedSpawn ds)
        {
            Invoke(nameof(ds.SpawnNow), ds.delay);
        }
    }
}