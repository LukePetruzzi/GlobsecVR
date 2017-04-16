using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class IBX_Controller : MonoBehaviour
{
    GameObject readyLight, busyLight1, busyLight2, busyLight3, presentT1Light, presentT2Light, presentT3Light, noMatchLight, matchLight1, matchLight2, matchLight3;
    Color bluishColor;
    VRTK_UIPointer pointerR;
    VRTK_UIPointer pointerL;
    bool isCalibrated = false;

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

        bluishColor = presentT2Light.GetComponentInChildren<Light>().color;

        turnIBXOff();
    }


    // Use this for initialization
    void Start()
    {
        // get all the lights
        getLights();

        Debug.Log("SETTING UP!!");

        // set up pointer and listeners
        pointerL = GameObject.Find("LeftController").GetComponent<VRTK_UIPointer>();
        pointerL.UIPointerElementEnter += ButtonClicked; // click button listener
        pointerR = GameObject.Find("RightController").GetComponent<VRTK_UIPointer>();
        pointerR.UIPointerElementEnter += ButtonClicked; // click button listener
    }

    private void ButtonClicked(object sender, UIPointerEventArgs e)
    {
        if (e.currentTarget.name == "PowerButton")
        {
            // IBX is off, turn the IBX on
            if (!readyLight.GetComponentInChildren<Light>().enabled)
            {
                readyLight.GetComponentInChildren<Light>().color = Color.yellow;
                readyLight.GetComponentInChildren<Light>().enabled = true;
            }
            else // turn the IBX off (turn off all lights)
            {
                turnIBXOff();
            }
        }
        else if (e.currentTarget.name == "VoltageButton" && readyLight.GetComponentInChildren<Light>().enabled)
        {
            // set readylight to correct color
            readyLight.GetComponentInChildren<Light>().color = bluishColor;
            readyLight.GetComponentInChildren<Light>().enabled = true;
        }
        else if (e.currentTarget.name == "CalibrateButton" && IBXisReady())
        {
            StartCoroutine(Calibrate());
        }
        else if (e.currentTarget.name == "InspectButton" && IBXisReady() && isCalibrated)
        {
            StartCoroutine(Inspect());
        }
    }

    private bool IBXisReady()
    {
        if (readyLight.GetComponentInChildren<Light>().enabled && readyLight.GetComponentInChildren<Light>().color == bluishColor)
            return true;
        else
            return false;
    }

    private void turnIBXOff()
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

    IEnumerator Calibrate()
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

    IEnumerator Inspect()
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
            if (Physics.Raycast(this.transform.position, this.transform.forward, out hit, 2f, mask))
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
            // Debug.DrawRay(this.transform.position, this.transform.forward * 2f, Color.red, 1f);

            // turn off the busy light
            busyLight3.GetComponentInChildren<Light>().enabled = false;
        }
    }
}