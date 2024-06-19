using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object Lock = new object();
    private static bool _applicationIsQuitting = false;
    protected static bool dontDestroyOnLoad = true;

    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                                 "' already destroyed on application quit. Won't create again - returning null.");
                return null;
            }

            lock (Lock)
            {
                if (_instance != null) return _instance;
                
                _instance = (T)FindObjectOfType(typeof(T));

                if (FindObjectsOfType(typeof(T)).Length > 1)
                {
                    Debug.LogError("[Singleton] Something went really wrong " +
                                   " - there should never be more than 1 singleton! " +
                                   "Reopening the scene might fix it.");
                    return _instance;
                }

                if (_instance == null)
                {
                    GameObject singleton = new GameObject();
                    _instance = singleton.AddComponent<T>();
                    singleton.name = "(singleton) " + typeof(T).ToString();

                    if (!(Application.isEditor && !Application.isPlaying) && dontDestroyOnLoad)
                        DontDestroyOnLoad(singleton);

                    Debug.Log("[Singleton] An instance of " + typeof(T) +
                              " is needed in the scene, so '" + singleton +
                              "' was created with DontDestroyOnLoad.");
                }
                else
                {
                    Debug.Log("[Singleton] Using instance already created: " +
                              _instance.gameObject.name);
                }

                return _instance;
            }
        }
    }

    private void OnDestroy()
    {
        if (dontDestroyOnLoad)
            _applicationIsQuitting = true;
        else
            _instance = null;
    }
}