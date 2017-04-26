using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class IBX_Controller : MonoBehaviour
{
    public MyNetworkedPlayerController networkedController;
    public VRTK_ControllerEvents controllerEventsL, controllerEventsR;
    VRTK_UIPointer pointerR;
    VRTK_UIPointer pointerL;

    public GameObject readyLight, busyLight1, busyLight2, busyLight3, presentT1Light, presentT2Light, presentT3Light, matchLight1, matchLight2, matchLight3, noMatchLight;
    public Color bluishColor;
    
    private bool isCalibrated = false;


    static IBX_Controller instance;


    private void Start()
    {
        // get the lights
        getLights();
    }

    // get all the lights
    private void getLights()
    {
        readyLight = GameObject.Find("ReadyLight").gameObject;
        busyLight1 = GameObject.Find("BusyLight1").gameObject;
        busyLight2 = GameObject.Find("BusyLight2").gameObject;
        busyLight3 = GameObject.Find("BusyLight3").gameObject;
        presentT1Light = GameObject.Find("PresentT1Light").gameObject;
        presentT2Light = GameObject.Find("PresentT2Light").gameObject;
        presentT3Light = GameObject.Find("PresentT3Light").gameObject;
        noMatchLight = GameObject.Find("NoMatchLight").gameObject;
        matchLight1 = GameObject.Find("MatchLight1").gameObject;
        matchLight2 = GameObject.Find("MatchLight2").gameObject;
        matchLight3 = GameObject.Find("MatchLight3").gameObject;
    }

    private void Update()
    {
        if (pointerL == null && GameObject.Find("LeftController") != null)
        {
            // set up pointer and listeners
            pointerL = GameObject.Find("LeftController").GetComponent<VRTK_UIPointer>();
            pointerL.UIPointerElementClick += ButtonClicked; // click button listener
        }
        if (pointerR == null && GameObject.Find("RightController") != null)
        {
            pointerR = GameObject.Find("RightController").GetComponent<VRTK_UIPointer>();
            pointerR.UIPointerElementClick += ButtonClicked; // click button listener
        }
        if (networkedController == null && GameObject.Find("PlayerBody_localPlayer") != null)
        {
            networkedController = GameObject.Find("PlayerBody_localPlayer").GetComponent<MyNetworkedPlayerController>();

            // find the left and right controllers
            controllerEventsL = GameObject.Find("LeftController").GetComponent<VRTK_ControllerEvents>();
            controllerEventsR = GameObject.Find("RightController").GetComponent<VRTK_ControllerEvents>();
        }
    }


    private void ButtonClicked(object sender, UIPointerEventArgs e)
    {

        if (e.currentTarget.name == "PowerButton")
        {
            if (readyLight == null)
            {
                //getLights();
                Debug.Log("HOUSTON...");
            }
            if (this.readyLight == null)
            {
                Debug.Log("THIS HOUSTON...");
            }
            // IBX is off, turn the IBX on
            if (!readyLight.GetComponentInChildren<Light>().enabled)
            {
                networkedController.NetworkTurnPowerOn();
            }
            else // turn the IBX off (turn off all lights)
            {
                networkedController.NetworkTurnIBXOff();
            }
        }
        else if (e.currentTarget.name == "VoltageButton" && readyLight.GetComponentInChildren<Light>().enabled)
        {
            networkedController.NetworkTurnVoltageOn();
        }
        else if (e.currentTarget.name == "CalibrateButton" && IBXisReady())
        {
            networkedController.NetworkCalibrate();
        }
        else if (e.currentTarget.name == "InspectButton" && IBXisReady() && isCalibrated)
        {
            networkedController.NetworkInspect();
        }
        
    }

    public void TurnPowerOn()
    {
        readyLight.GetComponentInChildren<Light>().color = Color.yellow;
        readyLight.GetComponentInChildren<Light>().enabled = true;
    }

    public void TurnVoltageOn()
    {
        // set readylight to correct color
        readyLight.GetComponentInChildren<Light>().color = bluishColor;
        readyLight.GetComponentInChildren<Light>().enabled = true;
    }

    private bool IBXisReady()
    {
        if (readyLight.GetComponentInChildren<Light>().enabled && readyLight.GetComponentInChildren<Light>().color == bluishColor)
            return true;
        else
            return false;
    }

    
    public void TurnIBXOff()
    {
        readyLight.GetComponentInChildren<Light>().enabled = false;
        busyLight1.GetComponentInChildren<Light>().enabled = false;
        busyLight2.GetComponentInChildren<Light>().enabled = false;
        busyLight3.GetComponentInChildren<Light>().enabled = false;
        presentT1Light.GetComponentInChildren<Light>().enabled = false;
        presentT2Light.GetComponentInChildren<Light>().enabled = false;
        presentT3Light.GetComponentInChildren<Light>().enabled = false;
        noMatchLight.GetComponentInChildren<Light>().enabled = false;
        matchLight1.GetComponentInChildren<Light>().enabled = false;
        matchLight2.GetComponentInChildren<Light>().enabled = false;
        matchLight3.GetComponentInChildren<Light>().enabled = false;

        isCalibrated = false;
    }

    public IEnumerator Calibrate()
    {
        // turn off other lights that may be on during this time
        noMatchLight.GetComponentInChildren<Light>().enabled = false;
        matchLight2.GetComponentInChildren<Light>().enabled = false;
        busyLight1.GetComponentInChildren<Light>().enabled = true;
        presentT2Light.GetComponentInChildren<Light>().enabled = false;

        yield return new WaitForSeconds(3);
        busyLight1.GetComponentInChildren<Light>().enabled = false;
        // now that it's calibrated, show we have the template stored and ready to go
        presentT2Light.GetComponentInChildren<Light>().enabled = true;
        isCalibrated = true;
    }

    public IEnumerator Inspect()
    {
        noMatchLight.GetComponentInChildren<Light>().enabled = false;
        matchLight2.GetComponentInChildren<Light>().enabled = false;
        busyLight3.GetComponentInChildren<Light>().enabled = true;

        yield return new WaitForSeconds(5);

        // only do this if ibx is still on after the waiting
        if (IBXisReady())
        {
            RaycastHit hit;
            int mask = (1 << LayerMask.NameToLayer("Shield"));

            // cast the ray out 2 meters
            if (Physics.Raycast(this.transform.position, -this.transform.right, out hit, 2f, mask))
            {
                // check that the tag on the 
                if (hit.transform.CompareTag("isMatchWarhead"))
                {
                    matchLight2.GetComponentInChildren<Light>().enabled = true;
                }
                else
                {
                    noMatchLight.GetComponentInChildren<Light>().enabled = true;
                }
            }
            else
            {
                noMatchLight.GetComponentInChildren<Light>().enabled = true;
            }

            Debug.DrawRay(this.transform.position, -this.transform.right * 2f, Color.red, 1f);

            // turn off the busy light
            busyLight3.GetComponentInChildren<Light>().enabled = false;
        }
    }
}