using On.Steamworks;
using UnityEngine;

namespace TeleportEverything
{
    public class DelayedSpawn
    {
        public Character character { get; }
        public float delay { get; set; }
        public Vector3? pos { get; set; }
        public Quaternion? rot { get; set; }
        public bool? follow { get; set; }
        
        public bool? ally { get; set; }

        public Transform transform { get; set; }


        public  DelayedSpawn(Character _original)
        {
            character = Object.Instantiate(_original);
            delay = 0f;
            pos = null;
            rot = null;
            follow = null;
            ally = null;
            transform = character.transform;

        }

        public  DelayedSpawn(Character _original, bool _ally, float _delay, Vector3 _pos, Quaternion _rot, bool? _follow)
        {
            
            delay = _delay;
            pos = _pos;
            rot = _rot;
            follow = _follow;
            ally = _ally;
            transform = character.transform;
            
        }

        private Character CloneCharacter(Character _original)
        {
            Character clone = Object.Instantiate(_original);
            
            _original.GetComponent<tameable>())
        }
        
        
        
    }
}