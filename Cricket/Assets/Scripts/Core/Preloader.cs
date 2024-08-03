using UnityEngine;

public class Preloader : Singleton<Preloader>
{
    [SerializeField] private GameObject[] singletons;

    protected override void OnSuccessfulAwake()
    {
        foreach (var singleton in singletons)
        {
            Instantiate(singleton);
        }
    }
}
