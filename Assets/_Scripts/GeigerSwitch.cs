using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class GeigerSwitch : VRTK_InteractableObject {

    public override void StartUsing(GameObject usingObject)
    {
        base.StartUsing(usingObject);

        // turn on the Geiger counter script
        this.GetComponent<GammaDetector>().enabled = true;
    }

    public override void StopUsing(GameObject previousUsingObject)
    {
        base.StopUsing(previousUsingObject);

        // turn off the Geiger counter script
        this.GetComponent<GammaDetector>().enabled = false;
    }
}
