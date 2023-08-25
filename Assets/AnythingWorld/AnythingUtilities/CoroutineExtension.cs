using System;
using System.Collections;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif
using UnityEngine;

namespace AnythingWorld.Utilities
{
    public static class CoroutineExtension
    {
        private static CoroutineAnchor Anchor
        {
            get
            {
                if (GameObject.FindObjectOfType<CoroutineAnchor>())
                {
                    return GameObject.FindObjectOfType<CoroutineAnchor>();
                }
                else
                {
                    var anchor = new GameObject("CoroutineAnchor").AddComponent<CoroutineAnchor>();
                    anchor.hideFlags = HideFlags.HideInHierarchy;
                    return anchor;
                }
            }
        }

        public static void StopCoroutines()
        {
        }


        /// <summary>
        /// Starts coroutine depending on context of engine (editor or runtime)
        /// </summary>
        /// <param name="enumerator"></param>
        /// <param name="owner">Owner MonoBehaviour script.</param>
        public static void StartCoroutine(IEnumerator enumerator, MonoBehaviour owner)
        {
#if UNITY_EDITOR
            EditorCoroutineUtility.StartCoroutine(enumerator, owner);
#else
        owner.StartCoroutine(enumerator);
#endif
        }

        public static void StartCoroutine(IEnumerator enumerator)
        {
#if UNITY_EDITOR
            EditorCoroutineUtility.StartCoroutineOwnerless(enumerator);
#else
            Anchor.StartCoroutine(enumerator);
#endif
        }


        public static IEnumerator WaitForSeconds(float seconds)
        {
#if UNITY_EDITOR
            yield return new EditorWaitForSeconds(seconds);
#else
        yield return new WaitForSeconds(seconds);
#endif

        }
#if UNITY_EDITOR
        public static void StartEditorCoroutine(IEnumerator enumerator, object owner)
        {
            EditorCoroutineUtility.StartCoroutine(enumerator, owner);

        }
        public static void StartEditorCoroutine(IEnumerator enumerator)
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(enumerator);

        }
#endif

        public static IEnumerator WaitThen(float seconds, Action firstDo, Action whenDone)
        {
            firstDo.Invoke();
#if UNITY_EDITOR
            yield return new EditorWaitForSeconds(seconds);
#else
        yield return new WaitForSeconds(seconds);
#endif
            whenDone?.Invoke();
        }

    }
}

