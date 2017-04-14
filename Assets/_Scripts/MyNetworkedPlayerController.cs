using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MyNetworkedPlayerController : NetworkBehaviour {

    //source gameobjects head, left and right controller object
    public GameObject rightContSource;
    public GameObject leftContSource;
    public GameObject headObjSource;




    //prefabs to assign head, left controller, and right controller
    public GameObject vrHeadObjPrefab;
    public GameObject vrLeftCtrlPrefab;
    public GameObject vrRightCtrlPrefab;

    GameObject vrHeadObj;
    GameObject vrLeftCtrl;
    GameObject vrRightCtrl;


    public override void OnStartLocalPlayer()
    {
        GetComponent<NetworkAnimator>().SetParameterAutoSend(0, true);
    }

    public override void PreStartClient()
    {
        GetComponent<NetworkAnimator>().SetParameterAutoSend(0, true);
    }
    // Use this for initialization
    void Start () {

        // enable the animator if client or server alike
        this.gameObject.GetComponent<Animator>().enabled = true;
        this.gameObject.GetComponent<IKControl>().enabled = true;

        // turn on everything the local player needs
        if (isLocalPlayer)
        {
            this.gameObject.GetComponent<PlayerController>().enabled = true;
            this.gameObject.GetComponent<UpdateBodyPosition>().enabled = true;

            // turn off the head
            getChildGameObject(this.gameObject, "Body").GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            getChildGameObject(this.gameObject, "default").GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            getChildGameObject(this.gameObject, "Eyelashes").GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            getChildGameObject(this.gameObject, "Eyewear").GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            getChildGameObject(this.gameObject, "Hats").GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;


            // Instantiate head and controllers
            CmdInstantiteHeadAndController();



            // change name so it's obvious whos local in the inspector
            gameObject.name = gameObject.name.Replace("(Clone)", "") + "_localPlayer";

        }
        else // is NOT the local player... just leave
        {

            gameObject.name = gameObject.name.Replace("(Clone)", "") + "_clientPlayer";

            return;
        }
	}

    //Instantiate on start head and vr controller object so that it can be view by normal players
    [Command]
    void CmdInstantiteHeadAndController()
    {
        // instantiate the objects using the prefabs created
        vrHeadObj = (GameObject)Instantiate(vrHeadObjPrefab);
        vrHeadObj.GetComponent<MySpawnController>().parentNetId = this.netId;
        

        vrLeftCtrl = (GameObject)Instantiate(vrLeftCtrlPrefab);
        vrLeftCtrl.GetComponent<MySpawnController>().parentNetId = this.netId;


        vrRightCtrl = (GameObject)Instantiate(vrRightCtrlPrefab);
        vrRightCtrl.GetComponent<MySpawnController>().parentNetId = this.netId;


        // spawn the objects for the clients
        NetworkServer.Spawn(vrHeadObj);
        NetworkServer.Spawn(vrLeftCtrl);
        NetworkServer.Spawn(vrRightCtrl);
    }

    // Update is called once per frame
    void Update () {
        
        // only want to be updating on local players. This handles commands work as they should
        if (!isLocalPlayer)
        {
            return;
        }

        // find the local hardware of the head and controllers if they haven't been found yet
        if (headObjSource == null || leftContSource == null || rightContSource == null)
        {
 
            this.headObjSource = GameObject.Find("Camera (eye)");

            // the controllers are actually finding the controller points on the hands I want for IK
            this.leftContSource = GameObject.Find("leftHandTarget");
            this.rightContSource = GameObject.Find("rightHandTarget");
        }

        //sync pos on network
        CmdControllerPositionSync();
    }


    //sync position on VR controller objects so that VR player movemnts/action can be viewd by normal user
    [Command]
    public void CmdControllerPositionSync()
    {
        // head transform update
        if (headObjSource != null)
        {
            vrHeadObj.transform.position = headObjSource.transform.position;
            vrHeadObj.transform.rotation = headObjSource.transform.rotation;
        }
        // left controller transform update
        if (leftContSource != null)
        {
            vrLeftCtrl.transform.position = leftContSource.transform.position;
            vrLeftCtrl.transform.rotation = leftContSource.transform.rotation;
        }
        // right controller transform update
        if (rightContSource != null)
        {
            vrRightCtrl.transform.position = rightContSource.transform.position;
            vrRightCtrl.transform.rotation = rightContSource.transform.rotation;
        }
    }

    static public GameObject getChildGameObject(GameObject fromGameObject, string withName)
    {
        Transform[] ts = fromGameObject.transform.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in ts) if (t.gameObject.name == withName) return t.gameObject;
        return null;
    }
}