using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

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

    // IBX controller to network those interactions
    public IBX_Controller ibxController;


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

        // turn on everything the local player needs
        if (isLocalPlayer)
        {
            // move the camerarig to where the player spawned
            GameObject rig = GameObject.Find("[CameraRig]");
            rig.transform.position = this.gameObject.transform.position;
            rig.transform.rotation = this.gameObject.transform.rotation;

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
        NetworkServer.SpawnWithClientAuthority(vrHeadObj, this.connectionToClient);
        NetworkServer.SpawnWithClientAuthority(vrLeftCtrl, this.connectionToClient);
        NetworkServer.SpawnWithClientAuthority(vrRightCtrl, this.connectionToClient);

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

        // find the IBX controller
        if (SceneManager.GetActiveScene().name == "IBXScene" && ibxController == null && GameObject.Find("IBXCanvas") != null)
        {
            Debug.Log("ID OF THE NetworkedIBXController: " + GameObject.Find("IBXCanvas").GetComponent<IBX_Controller>().GetInstanceID());
            ibxController = GameObject.Find("IBXCanvas").GetComponent<IBX_Controller>();
        }

        //sync pos on network
         ControllerPositionSync();
    }


    //sync position on VR controller objects so that VR player movemnts/action can be viewd by normal user
    //[Command]
    public void ControllerPositionSync()
    {
        if (vrHeadObj == null)
        {
            Debug.Log("THE NETWORKED HEAD OBJECT IS NULL");
        }
        if (headObjSource == null)
        {
            Debug.Log("THE HEAD OBJECT SOURCE IS NULL");
        }

        // THIS IS NECESSARY FOR THE CLIENTS. THEY DON'T HAVE THE REFERENCE TO THE OBJECT WHEN IT'S CREATED LIKE THE SERVER DOES. 
        // THIS FIXES THE REFERENCE ONCE THE OBJECT HAS BEEN MOVED AS A CHILD
        if (vrHeadObj == null)
        {
            vrHeadObj = getChildGameObject(this.gameObject, "NetworkTrackedHead");
        }
        if (vrLeftCtrl == null)
        {
            vrLeftCtrl = getChildGameObject(this.gameObject, "NetworkTrackedLeftController");
        }
        if (vrRightCtrl == null)
        {
            vrRightCtrl = getChildGameObject(this.gameObject, "NetworkTrackedRightController");
        }

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

    // SCENE CHANGING FUNCTIONS **************************************************************************************************************************************
    public void ChangeScene(string sceneName)
    {
        //SceneManager.LoadScene("IBXScene");
        if (isServer)
        {
            SceneManager.LoadScene(sceneName);
            GameObject.Find("NetworkManager").GetComponent<NetworkManager>().ServerChangeScene(sceneName);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
            CmdInvokeSceneChange(sceneName);
        }
    }

    [Command]
    private void CmdInvokeSceneChange(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        GameObject.Find("NetworkManager").GetComponent<NetworkManager>().ServerChangeScene(sceneName);
    }

    //private IEnumerator WaitToLoad()
    //{
    //    yield return new WaitForSeconds(3);
    //}

    // IBX CONTROLLER FUNCTIONS **************************************************************************************************************************************

    public void NetworkTurnPowerOn()
    {
        checkIBXController();
        if (isServer)
        {
            // do it for the server
            ibxController.TurnPowerOn();
            // do it on all clients
            RpcTurnPowerOn();
        }
        else
        {
            Debug.Log("Calling Command from Client");
            CmdTurnPowerOn();
        }
    }
    [ClientRpc]
    public void RpcTurnPowerOn()
    {
        Debug.Log("RPC is running now");
        checkIBXController();
        ibxController.TurnPowerOn();
    }
    [Command]
    public void CmdTurnPowerOn()
    {
        Debug.Log("Command Running on Server");
        ibxController.TurnPowerOn();
        RpcTurnPowerOn();
    }

    public void NetworkTurnVoltageOn()
    {
        checkIBXController();
        if (isServer)
        {
            ibxController.TurnVoltageOn();
            RpcTurnVoltageOn();
        }
        else
        {
            CmdTurnVoltageOn();
        }
    }
    [ClientRpc]
    public void RpcTurnVoltageOn()
    {
        checkIBXController();
        ibxController.TurnVoltageOn();
    }
    [Command]
    public void CmdTurnVoltageOn()
    {
        ibxController.TurnVoltageOn();
        RpcTurnVoltageOn();
    }

    public void NetworkTurnIBXOff()
    {
        checkIBXController();
        if (isServer)
        {
            ibxController.TurnIBXOff();
            RpcTurnIBXOff();
        }
        else
        {
            CmdTurnIBXOff();
        }
    }
    [ClientRpc]
    public void RpcTurnIBXOff()
    {
        checkIBXController();
        ibxController.TurnIBXOff();
    }
    [Command]
    public void CmdTurnIBXOff()
    {
        ibxController.TurnIBXOff();
        RpcTurnIBXOff();
    }

    public void NetworkCalibrate()
    {

    }

    public void NetworkInspect()
    {

    }

    private void checkIBXController()
    {
        if (ibxController == null)
        {
            ibxController = GameObject.Find("IBXCanvas").GetComponent<IBX_Controller>();
        }
    }


    static public GameObject getChildGameObject(GameObject fromGameObject, string withName)
    {
        Transform[] ts = fromGameObject.transform.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in ts) if (t.gameObject.name == withName) return t.gameObject;
        return null;
    }
}