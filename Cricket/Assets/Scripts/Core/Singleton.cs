using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : class
{
    public static T Instance;

    [SerializeField]
    private bool shouldDestroyOnLoad = false;

    protected void Awake()
    {
        if (this is T val)
        {
            if (Instance == null)
            {
                Instance = val;
                if (!shouldDestroyOnLoad)
                {
                    Object.DontDestroyOnLoad(base.gameObject);
                }

                OnSuccessfulAwake();
            }
            else if (Instance != val)
            {
                Object.Destroy(base.gameObject);
            }
        }
        else
        {
            Debug.Log("You did not set an appropriate type for this singleton. See examples for details.");
            Object.Destroy(base.gameObject);
        }
    }

    protected virtual void OnSuccessfulAwake()
    {
    }

    protected virtual void OnDestroy()
    {
        if (this is T val && Instance == val)
        {
            Instance = null;
        }
    }
}