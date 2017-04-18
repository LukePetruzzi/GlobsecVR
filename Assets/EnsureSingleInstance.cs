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
        }
        else
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
    }

}
