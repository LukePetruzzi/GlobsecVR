using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRTK;

public class CustomSceneTeleportController : MonoBehaviour {

    
    public VRTK_DestinationMarker pointer;
    public VRTK_ControllerEvents controllerEvents;
    public MyNetworkedPlayerController networkedController;

    private void OnEnable()
    {
        pointer.DestinationMarkerEnter += CheckSceneTeleport;
        pointer.DestinationMarkerExit += DisableCanvas;
        //SceneManager.sceneLoaded += WaitForControllerInput;
    }

    private void OnDisable()
    {
        pointer.DestinationMarkerEnter -= CheckSceneTeleport;
        pointer.DestinationMarkerExit -= DisableCanvas;

    }

    // Use this for initialization
    void Start () { 
        
	}
	
	// Update is called once per frame
	void Update () {
        if (networkedController == null && GameObject.Find("PlayerBody_localPlayer") != null)
        {
            networkedController = GameObject.Find("PlayerBody_localPlayer").GetComponent<MyNetworkedPlayerController>();
        }
	}

    void WaitForControllerInput(Scene s, LoadSceneMode y)
    {
        StartCoroutine(Wait());
    }
    IEnumerator Wait()
    {
        pointer.enabled = false;
        yield return new WaitForSeconds(2);
        pointer.enabled = true;
    }

    void CheckSceneTeleport(object sender, DestinationMarkerEventArgs e)
    {
        // check the target
        if (e.target.gameObject.tag == "SceneTeleportDestination")
        {
            // scale up the canvas
            Transform t = e.target.gameObject.GetComponentInChildren<Canvas>().transform;
            StartCoroutine(LerpScale(t, t.localScale, Vector3.one));

            if (e.target.gameObject.name == "BuildingC")
            {
                // user pull sthe trigger to teleport
                if (controllerEvents.triggerPressed)
                {
                    // use the network controller to change the scene
                    networkedController.ChangeScene("Bunker_C");
                }
            }
            if (e.target.gameObject.name == "BuildingB")
            {
                // user pull sthe trigger to teleport
                if (controllerEvents.triggerPressed)
                {
                    // use the network controller to change the scene
                    networkedController.ChangeScene("Bunker_B");
                }
            }
            if (e.target.gameObject.name == "BuildingA")
            {
                // user pull sthe trigger to teleport
                if (controllerEvents.triggerPressed)
                {
                    // use the network controller to change the scene
                    networkedController.ChangeScene("Bunker_A");
                }
            }
            if (e.target.gameObject.name == "DoorToMainRoom")
            {
                // user pull sthe trigger to teleport
                if (controllerEvents.triggerPressed)
                {
                    if (GameObject.Find("IBX") != null)
                    {
                        // turn off the meshes
                        foreach (Renderer rend in GameObject.Find("IBX").GetComponentsInChildren<Renderer>())
                        {
                            rend.enabled = false;
                        }
                    }

                    // use the network controller to change the scene
                    networkedController.ChangeScene("MainSceneReturn");
                }
            }
            if (e.target.gameObject.name == "WeaponToInspect")
            {
                if (controllerEvents.triggerPressed)
                {
                    if (GameObject.Find("IBX") != null)
                    {
                        // turn on the meshes
                        foreach (Renderer rend in GameObject.Find("IBX").GetComponentsInChildren<Renderer>())
                        {
                            rend.enabled = true;
                        }

                        // turn the power off for the next scene
                        networkedController.NetworkTurnIBXOff();
                        Debug.Log("TURNED OFF IBX POWER");
                    }

                    // use the network controller to change the scene
                    networkedController.ChangeScene("IBXScene");
                }
            }
        }
    }



    //[ClientRpc]
    //private void RpcNetworkedChangeScene()
    //{
    //    GameObject.Find("NetworkManager").GetComponent<NetworkManager>().ServerChangeScene("IBXScene");
    //}

    void DisableCanvas(object sender, DestinationMarkerEventArgs e)
    {
        if (e.target.gameObject.tag == "SceneTeleportDestination")
        {
            // scale down the canvas
            Transform t = e.target.gameObject.GetComponentInChildren<Canvas>().transform;
            StartCoroutine(LerpScale(t, t.localScale, Vector3.zero));
        }
    }

    IEnumerator LerpScale(Transform t, Vector3 InitialScale, Vector3 FinalScale)
    {
        float progress = 0;

        while (progress <= 1)
        {
            t.localScale = Vector3.Lerp(InitialScale, FinalScale, progress);
            progress += Time.deltaTime * 5;
            yield return null;
        }

        t.localScale = FinalScale;
    }

    static public GameObject getChildGameObject(GameObject fromGameObject, string withName)
    {
        Transform[] ts = fromGameObject.transform.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in ts) if (t.gameObject.name == withName) return t.gameObject;
        return null;
    }
}
