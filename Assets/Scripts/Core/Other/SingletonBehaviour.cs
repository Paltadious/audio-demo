using UnityEngine;

namespace Core.Other
{
    public class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    SingletonBehaviour<T> t = FindObjectOfType<SingletonBehaviour<T>>();
                    if (t != null)
                    {
                        instance = t.GetComponent<T>();
                    }
                }

                return instance;
            }
        }

        private static T instance;

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
            }
            else
            {
                Destroy(this);
            }
        }
    }
}