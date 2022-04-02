using System.Collections;
using UnityEngine;

namespace TeleportEverything
{
    public class DelayedAction : MonoBehaviour
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
