using System;
using System.Collections;
using UnityEngine;

namespace HappyPixels.EditorAddons.Extensions
{
    // This is based on https://wp.flyingshapes.com/dont-use-monobehaviour-invoke-or-how-to-properly-invoke-a-method/
    public static class MonoBehaviourExtensions
    {
        /// <summary>
        /// Invoke a method after a amount of seconds
        /// </summary>
        /// <param name="mb"></param>
        /// <param name="method"></param>
        /// <param name="delay"></param>
        public static void InvokeAfterDelay(this MonoBehaviour mb, Action method, float delay) =>
            mb.StartCoroutine(ExecuteAfterTime(method, delay));

        private static IEnumerator ExecuteAfterTime(Action theDelegate, float delay)
        {
            yield return new WaitForSeconds(delay);
            theDelegate();
        }
        
        /// <summary>
        /// Keeps repeating the invocation of a method at a given rate
        /// </summary>
        /// <param name="mb"></param>
        /// <param name="method">Method to invoke</param>
        /// <param name="delay">Delay between invocations</param>
        /// <param name="repeatRate">Repeating rate, -1 to repeat once, 0 to repeat infinitely and for repeatRate > 0 will repeat for repeatRate amount</param>
        /// <returns>an object of type <code>Coroutine</code></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Coroutine InvokeRepeating(this MonoBehaviour mb, System.Action method, float delay, float repeatRate)
        {
            if (mb == null) throw new ArgumentNullException(nameof(mb));
            if (method == null) throw new ArgumentNullException(nameof(method));
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

