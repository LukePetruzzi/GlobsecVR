using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MySpawnController : NetworkBehaviour {

    [SyncVar]
    public NetworkInstanceId parentNetId;

    public override void OnStartClient()
    {
        // set the parent of the gameobject correctly
        GameObject parentObject = ClientScene.FindLocalObject(parentNetId);
        transform.SetParent(parentObject.transform);
        this.gameObject.name = this.gameObject.name.Substring(0, this.gameObject.name.LastIndexOf("(Clone)"));
    }
}
