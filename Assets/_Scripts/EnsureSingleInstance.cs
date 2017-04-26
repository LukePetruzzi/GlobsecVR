using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnsureSingleInstance : MonoBehaviour {

    static EnsureSingleInstance instance;

    // EVENTS
    private void Awake()
    {
        if (instance)
        {
            Destroy(this.gameObject);
           // Debug.Log("Destryed new instance");
        }
        else
        {
            instance = this;
           // Debug.Log("Saved original instance");
            //DontDestroyOnLoad(gameObject);
        }
    }

}
