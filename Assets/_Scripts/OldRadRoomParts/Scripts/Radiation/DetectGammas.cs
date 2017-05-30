// Attach script (drag and drop) to "Detector" GameObject in scene to have it detect gamma radiation

//  Initially authored by B. K. Cogswell, Princeton University, August, 2016

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class DetectGammas : MonoBehaviour
{
	// initialize constants

	//cps (counts-per-second) --> uSv/hr (microSieverts per hour) conversion factor; includes factor of 1/4*PI
	public float k = 5.263e-6f;

	// read object attenuation info from file

	//  THIS DICTIONARY BUILD SHOULD BE MOVED TO START() AND ONLY DONE ONCE AT BEGINNING OF SIMULATION LOAD

	public class ShieldDictionary {
		public Dictionary<string, Dictionary<string, List<float>>> Dict() {
			Dictionary<string, Dictionary<string, List<float>>> objects = new Dictionary<string, Dictionary<string, List<float>>>();
			string line;
			string name = "empty";
			string property = "empty";
			System.IO.StreamReader file = new System.IO.StreamReader(@"Artificial_ShieldAttenuationProperties.txt");
			while ((line = file.ReadLine()) != null) {
				if (line.StartsWith("#")) {  /* skip comment lines in file */
				} else {
					if (line.StartsWith("@")) {  /* store line string as key for later */
						string[] text1 = line.Split(' ');
						name = text1[1];
						if (!objects.ContainsKey(name)) {
							objects.Add(name, new Dictionary<string, List<float>>());
						}
					} else {  /* store line string as sub-key for later */
						if (line.StartsWith("$")) {
							string[] text2 = line.Split(' ');
							property = text2[1];
							if (!objects[name].ContainsKey(property)) {
								objects[name].Add(property, new List<float>());
							}
						} else {
							string[] text3 = line.Split(' ');
							float[] data = new float[text3.Length];
							for (int i = 0; i < text3.Length; i++) {
								data[i] = Single.Parse(text3[i]);
							}
							if (objects[name].ContainsKey(property)) {
								objects[name][property].AddRange(data);
							}
						}
					}
				}
			}  /* end read file */
			file.Close();
			return objects;
		}
	} /* end dictionary class */

	// Use this for initialization

	void Start () {

		// READING-IN OF RADIATION ATTENUATION PROPERTIES FROM EXTERNAL *.TXT FILE SHOULD APPEAR HERE

	} /* end Start */
	
	// Update is called once per frame

	void Update () {

		// access object properties dictionary

		ShieldDictionary objectList = new ShieldDictionary ();
		Dictionary<string, Dictionary<string, List<float>>> obj = objectList.Dict ();

		// get source and detector properties scripts and locations

		GameObject gammaDetector = GameObject.Find ("Detector");
		GameObject gammaSource = GameObject.Find ("Source");
		RadioactiveSource radioactiveSource = gammaSource.GetComponent<RadioactiveSource> ();
		Vector3 sourceLocation = gammaSource.transform.position;
		Vector3 detectorLocation = gammaDetector.transform.position;

		// THE FOLLOWING THICKNESS CALCULATION SHOULD BE MOVED TO A FUNCTION AND TRIGGERED
		//  WHEN THE DETECTOR IS TURNED "ON" OR "OFF" USING A WAND BUTTON PRESS;
		//  A THICKNESS DICTIONARY MUST BE PASSED TO THE DOSE AND CPS FUNCTION

		// BEGIN THICKNESS CALCULATION

		// get names and thicknesses of shield objects between source and detector:
		//   use impact points between simultaneous bidirectional raycast lines and
		//   mesh colliders attached to GameObjects to determine thickness of object
		//   traversed by virtual radiation traveling from source --> detector

		//n.b.: using a bidirectional raycast ensures that GameObjects incorrectly modeled
		//   as planes or raycasts intersecting with GameObjects on a tangent do not return
		//   invalid object thicknesses due to an odd number (rather than even number )of
		//   impact points (as would happen with a monodirectional cast).

		// get hit info for frontfaces (source --> detector direction)

		List<string> shieldList = new List<string>();
		RaycastHit[] frontfaceHits;
		List<Vector3> frontfacePoints = new List<Vector3>();
		List<string> frontfaceObjectNames = new List<string>();
		var forwardHeading = detectorLocation - sourceLocation;
		var distance = forwardHeading.magnitude * 100.0f; /* source-detector distance in cm for dose calculation */
		var rayLength = forwardHeading.magnitude;
		var towardDetector = forwardHeading / rayLength;
		frontfaceHits = Physics.RaycastAll (sourceLocation, towardDetector, rayLength);
		for (int j = 0; j < frontfaceHits.Length; j++) {
			if (!shieldList.Contains(frontfaceHits[j].transform.gameObject.name)) { /* build unique list of shield object names */
				shieldList.Add(frontfaceHits[j].transform.gameObject.name);
			}
			frontfacePoints.Add(frontfaceHits [j].point);
			frontfaceObjectNames.Add (frontfaceHits[j].transform.gameObject.name);
		}

		// get hit info for backfaces (detector --> source direction)

		RaycastHit[] backfaceHits;
		List<Vector3> backfacePoints = new List<Vector3>();
		var backwardHeading = sourceLocation - detectorLocation;
		var towardSource = backwardHeading / rayLength;
		backfaceHits = Physics.RaycastAll (detectorLocation, towardSource, rayLength);
		for (int j = 0; j < backfaceHits.Length; j++) {
			backfacePoints.Insert(0,backfaceHits [j].point); /* re-order points to match order of frontface hits received */
		}

		// store array of object thicknesses

		List<float> widths = new List<float>();
		for (int i = 0; i < frontfacePoints.Count; i++){
			widths.Add (Vector3.Distance (frontfacePoints [i], backfacePoints [i]));
		}

		// build dictionary of object widths using array of thicknesses

		float sumWidths = 0.0f;
		Dictionary<string, Dictionary<string, float>> objectThicknesses = new Dictionary<string, Dictionary<string, float>> ();
		for (int i = 0; i < frontfaceObjectNames.Count; i++) {
			if (!objectThicknesses.ContainsKey(frontfaceObjectNames[i])) {
				objectThicknesses.Add(frontfaceObjectNames[i], new Dictionary<string, float>());
				objectThicknesses[frontfaceObjectNames[i]].Add("width", new float ());
				objectThicknesses [frontfaceObjectNames [i]] ["width"] = widths [i];
			} else { /* if object already in dictionary then sum all thicknesses to get total "effective" thickness of that material */
				sumWidths = objectThicknesses [frontfaceObjectNames [i]] ["width"] + widths[i];
				objectThicknesses [frontfaceObjectNames [i]] ["width"] = sumWidths;
			}
		}

		// END THICKNESS CALCULATION

		// BEGIN DOSE AND CPS CALCULATION

		//  store number of shield objects for sub-loop of dose calculation

		int numshields = shieldList.Count;

		// calculate dose

		float temp1 = 0.0f; /* exponential decay attentuation factor due to shield objects */
		float temp2 = 0.0f; /* dose including geometric attenuation (~ 1/distance^2) and exponential attenuation (~exp^(-thickness))*/
		float temp3 = 0.0f;
		float dose = 0.0f;
		float cps = 0.0f;
		for (int j = 0; j < radioactiveSource.E.Length; j++)  { /* loop over spectral (energy) lines */
			temp1 = 0.0f;
			for (int l = 0; l < numshields; l++)  { /* loop over shield objects */
				if (!obj.ContainsKey(shieldList[l])) {
					temp1 += 0.0f;
					print ("WARNING! Object ( " + shieldList[l] + " ) not found in Shield Dictionary. Attenuation factor set to 1.0.");
				} else {
					temp1 += obj[shieldList[l]]["attenuation"][j] * obj[shieldList[l]]["density"][0] * objectThicknesses[shieldList[l]]["width"]*100.0f;
				}
			}
			temp2 += radioactiveSource.Y[j] * radioactiveSource.E[j] * radioactiveSource.uep[j] * radioactiveSource.B[j] * Mathf.Exp(-temp1);
			temp3 += radioactiveSource.Y[j] * Mathf.Exp(-temp1);
			}
		dose = k*radioactiveSource.A*temp2/(distance*distance);
		print ("Dose [uSv/hr]: " + dose*1.0008e4f);

		// calculate counts-per-second [per cm^2]

		cps = radioactiveSource.A*temp3/(4.0f * 3.14159f * distance * distance);
		print ("Counts per Second : " + cps);

		// END DOSE AND CPS CALCULATION

		// A FUNCTION THAT TAKES IN CPS AND GENERATES A CLICKING SOUND NEEDS TO BE INSERTED HERE:
		//   THE CLICK TIMING SHOULD BE POISSON DISTRIBUTED ACROSS THE INTERVAL BETWEEN CPS CALCULATIONS.
		//     MAYBE SOMETHING LIKE t = -ln(1-randomNumber)/cps, where randomNumber = [0,1].
		//   I RECOMMEND MOVING THE DOSE CALCULATION TO A FIXEDUPDATE() OF ~1-5 TIMES PER SECOND AND GENERATING
		//     CLICKS UP TO AN ADJUSTABLE MAXIMUM VALUE (maxcps).
		//   ONCE MORE THAN maxcps ARE RECEIVED THE DETECTOR SHOULD GO ALL QUIET--
		//     A REAL ONE WOULD SATURATE ITS ABILITY TO PRODUCE SOUND PULSES. A GOOD STARTING MAX VALUE IS 500 CPS.
		//   THERE SHOULD ALSO BE AN ADJUSTABLE BACKGROUND COUNT RATE (backgroundcps).
		//     TO START THIS CAN BE SET FROM 1-3 CPS.

	} /* end Update */

} /* end DetectGammas */
