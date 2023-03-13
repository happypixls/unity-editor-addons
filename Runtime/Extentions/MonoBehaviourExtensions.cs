using System;
using System.Collections;
using UnityEngine;

namespace HappyPixels.EditorAddons.Extensions
{
    // This is based on https://wp.flyingshapes.com/dont-use-monobehaviour-invoke-or-how-to-properly-invoke-a-method/
    public static class MonoBehaviourExtensions
    {
        public static void Invoke(this MonoBehaviour mb, Action method, float delay) =>
            mb.StartCoroutine(ExecuteAfterTime(method, delay));

        private static IEnumerator ExecuteAfterTime(Action theDelegate, float delay)
        {
            yield return new WaitForSeconds(delay);
            theDelegate();
        }
        
        public static Coroutine InvokeRepeating(this MonoBehaviour mb, System.Action method, float delay, float repeatRate)
        {
            if (mb == null) throw new System.ArgumentNullException("mb");
            if (method == null) throw new System.ArgumentNullException("method");
            return mb.StartCoroutine(InvokeRepeatingWithParams(method, delay, repeatRate));
        }

        private static IEnumerator InvokeRepeatingWithParams(System.Action method, float delay, float repeatRate = -1f)
        {
            yield return new WaitForSeconds(delay);

            if (repeatRate < 0f)
            {
                method();
            }
            else if (repeatRate == 0f)
            {
                while (true)
                {
                    method();
                    yield return null;
                }
            }
            else
            {
                var w = new WaitForSeconds(repeatRate);
                while (true)
                {
                    method();
                    yield return w;
                }
            }
        }
    }
}

