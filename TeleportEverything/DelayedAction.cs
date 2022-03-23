using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeleportEverything
{
    public class DelayedAction:MonoBehaviour
    {
        public void InvokeDelayed(System.Action aDelegate, float delay)
        {
            StartCoroutine(DelayedCoroutine(aDelegate, delay));
        }

        private IEnumerator DelayedCoroutine(System.Action aDelegate, float delay)
        {
            yield return new WaitForSeconds(delay);
            aDelegate();
        }
    }
}
