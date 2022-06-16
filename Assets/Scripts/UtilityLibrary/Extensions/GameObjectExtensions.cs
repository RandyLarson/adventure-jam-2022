using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Extensions
{
    public static class GameObjectExtensions
    {
        public static Vector3 GetPosition(this GameObject src)
        {
            if (src != null && src.transform != null)
                return src.transform.position;
            else
                return Vector3.zero;
        }

        public static bool GetComponent<S, T>(this S src, out T foundComponent)
            where S : Component
            where T : Component
        {
            foundComponent = null;

            if (src == null)
                return false;

            foundComponent = src.GetComponent<T>();
            return foundComponent != null;
        }

        public static bool GetComponent<T>(this GameObject src, out T foundComponent) where T : Component
        {
            foundComponent = null;

            if (src == null)
                return false;

            foundComponent = src.GetComponent<T>();
            return foundComponent != null;
        }

        public static T GetComponentInChildrenExcludingSelf<T>(this GameObject src, bool immediateChildrenOnly) where T : Component
        {
            T foundComponent = null;

            if (src == null)
                return null;

            for (int i = 0; i < src.transform.childCount; i++)
            {
                if (immediateChildrenOnly)
                    foundComponent = src.transform.GetChild(i).GetComponent<T>();
                else 
                    foundComponent = src.transform.GetChild(i).GetComponentInChildren<T>();

                if (foundComponent != null)
                    break;
            }

            return foundComponent;
        }

        public static bool GetComponentInChildren<T>(this GameObject src, out T foundComponent) where T : Component
        {
            foundComponent = null;

            if (src == null)
                return false;

            foundComponent = src.GetComponentInChildren<T>();
            return foundComponent != null;
        }

        public static bool GetComponents<T>(this GameObject src, out IEnumerable<T> foundComponents) where T : Component
        {
            foundComponents = null;

            if (src == null)
                return false;

            foundComponents = src.GetComponents<T>();
            return foundComponents != null;
        }


        public static bool GetInterface<T>(this GameObject src, out T wishedFace) where T : class
        {
            wishedFace = null;
            if (src == null)
                return false;

            wishedFace = src.GetComponent(typeof(T)) as T;
            return wishedFace != null;
        }


        public static bool GetInterfaceInChildren<T>(this GameObject src, out T wishedFace) where T : class
        {
            wishedFace = null;
            if (src == null)
                return false;

            wishedFace = src.GetComponentInChildren(typeof(T)) as T;
            return wishedFace != null;
        }

        public static bool GetInterfaceInParents<T>(this GameObject src, out T wishedFace) where T : class
        {
            wishedFace = null;
            if (src == null)
                return false;

            wishedFace = src.GetComponentInParent(typeof(T)) as T;
            return wishedFace != null;
        }

        public static T GetInterface<T>(this GameObject src) where T : class
        {
            if (src == null)
                return null;

            T wishedFace = src.GetComponent(typeof(T)) as T;
            return wishedFace;
        }

        public static IEnumerable<T> GetInterfaces<T>(this GameObject src) where T : class
        {
            if (src == null)
                return new T[0];

            var matchingComponents = src.GetComponents(typeof(T));
            var asFaces = matchingComponents
                .OfType<T>()
                .ToArray();

            return asFaces;
        }

        public static IEnumerable<T> GetInterfacesInChildren<T>(this GameObject src) where T : class
        {
            if (src == null)
                return new T[0];

            var matchingComponents = src.GetComponentsInChildren(typeof(T));
            var asFaces = matchingComponents
                .Where(cmp => cmp != src)
                .OfType<T>()
                .ToArray();

            return asFaces;
        }
        public static IEnumerable<T> GetInterfacesInParents<T>(this GameObject src) where T : class
        {
            if (src == null)
                return new T[0];

            var matchingComponents = src.GetComponentsInParent(typeof(T));
            var asFaces = matchingComponents
                .Where(cmp => cmp != src)
                .OfType<T>()
                .ToArray();

            return asFaces;
        }

        public static void SafeSetActive<T>(this T[] target, bool value) where T : MonoBehaviour
        {
            if (target == null)
                return;

            for (int i = 0; i < target.Length; i++)
            {
                target[i].gameObject.SetActive(value);
            }
        }

        public static void SafeSetActive(this GameObject[] target, bool value)
        {
            if (target == null)
                return;

            for (int i = 0; i < target.Length; i++)
            {
                target[i].SetActive(value);
            }
        }

        public static void SafeSetActive<T>(this T target, bool value) where T : MonoBehaviour
        {
            if (target == null || target.gameObject == null)
                return;

            target.gameObject.SetActive(value);
        }

        public static void SafeDestroy(this GameObject toDestroy)
        {
            if (toDestroy == null)
                return;

            GameObject.Destroy(toDestroy);
        }

        public static void SafeDestroy<T>(this T toDestroy) where T : MonoBehaviour
        {
            if (toDestroy == null || toDestroy.gameObject == null)
                return;

            GameObject.Destroy(toDestroy.gameObject);
        }

        public static void SafeSetActive(this GameObject target, bool value)
        {
            if (target == null)
                return;

            target.SetActive(value);
        }

        public static bool SafeIsActive(this GameObject target)
        {
            return target != null && target.activeSelf;
        }

        public static bool IsValidGameobject(this MonoBehaviour target)
        {
            return target != null && target.gameObject != null;
        }
        
        public static bool IsValidGameobject(this GameObject target)
        {
            return target != null && target.gameObject != null;
        }

        public static bool SafeInstantiate(this GameObject toInstantiate, Vector2 position, out GameObject instantiated, float? autoDestructIn = null, Quaternion? rotation = null)
        {
            instantiated = default;

            if (toInstantiate == null)
                return false;

            instantiated = UnityEngine.Object.Instantiate(toInstantiate, position, rotation != null ? rotation.Value : toInstantiate.transform.rotation);
            if (autoDestructIn != null && autoDestructIn.Value > 0)
                GameObject.Destroy(instantiated, autoDestructIn.Value);

            return true;
        }

        public static GameObject GetTopmostParent(this GameObject srcObject)
        {
            if (srcObject == null)
                return null;

            GameObject topParent = srcObject;
            while (topParent.transform.parent != null)
            {
                topParent = topParent.transform.parent.gameObject;
            }

            return topParent;
        }

        static public T Instantiate<T>(T unityObject, System.Action<T> beforeAwake = null) where T : UnityEngine.Object
        {
            //Find prefab gameObject
            var gameObject = unityObject as GameObject;
            var component = unityObject as Component;

            if (gameObject == null && component != null)
                gameObject = component.gameObject;

            //Save current prefab active state
            var isActive = false;
            if (gameObject != null)
            {
                isActive = gameObject.activeSelf;
                //Deactivate
                gameObject.SetActive(false);
            }

            //Instantiate
            var obj = UnityEngine.Object.Instantiate(unityObject) as T;
            if (obj == null)
                throw new System.Exception("Failed to instantiate Object " + unityObject);

            //This function will be executed before awake of any script inside
            if (beforeAwake != null)
                beforeAwake(obj);

            //Revert prefab active state
            if (gameObject != null)
                gameObject.SetActive(isActive);

            //Find instantiated GameObject
            gameObject = obj as GameObject;
            component = obj as Component;

            if (gameObject == null && component != null)
                gameObject = component.gameObject;

            //Set active state to prefab state
            if (gameObject != null)
                gameObject.SetActive(isActive);

            return obj;
        }

        public static void Invoke(this MonoBehaviour mb, Action f, float delay)
        {
            if (mb != null && mb.isActiveAndEnabled)
                mb.StartCoroutine(InvokeRoutine(f, delay));
        }
    
        private static IEnumerator InvokeRoutine(System.Action f, float delay)
        {
            yield return new WaitForSeconds(delay);
            f();
        }
    }
}
