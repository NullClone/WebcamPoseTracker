using UnityEngine;

namespace WPT
{
    [ExecuteAlways]
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = (T)FindAnyObjectByType(typeof(T));

                    if (instance == null)
                    {
                        Debug.Log($"There is no {typeof(T).Name} in the scene.");
                    }
                }

                return instance;
            }
        }

        public virtual void Update()
        {
            if (this != Instance)
            {
                DestroyImmediate(this);

                return;
            }
        }
    }
}