using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//A raycast hit of a shield object between a source and detector
[System.Serializable]
public class ShieldHit
{
	public Vector3 hitPoint;
	public float distanceFromDetector;
	public bool facingDetector;	//Used for sorting bidirectional raycast hits

	public ShieldHit(Vector3 newHitPoint, float newDistanceFromDetector, bool newFacingDetector)
	{
		hitPoint = newHitPoint;
		facingDetector = newFacingDetector;
		distanceFromDetector = newDistanceFromDetector;
	}
}

//A shield object that is in between a source and detector. Contains ALL raycast hits on the object
[System.Serializable]
public class InterveningShield
{
	public RadioactiveSourceObject radioactiveSource;
	public ShieldObject shieldObject;
	public List<ShieldHit> hitPoints;
	public float totalThickness;

	public InterveningShield(ShieldObject newShieldObject, RadioactiveSourceObject newRadioactiveSource)
	{
		shieldObject = newShieldObject;
		hitPoints = new List<ShieldHit> ();
		totalThickness = 0;
		radioactiveSource = newRadioactiveSource;
	}
}

//The gamma detector. Detects radiation levels from radioactive sources from a point in space, taking into account distance and shield objects
public class GammaDetector : MonoBehaviour
{
	public IEnumerator currentDetectionRoutine;				//The coroutine that constantly runs to detect radiation
	public IEnumerator currentClickRoutine;					//The coroutine that constantly runs to create the clicking sound
	public List<InterveningShield> interveningShields;		//A list of all shield objects between the sources and the detector
	private int layerMask;									//A mask of collision layers for detecting shield objects
	public bool metersToCentimeters = true;					//Convert meters to centimeters? Usually use this in Unity to exaggerate units.
	private float clickDelta;								//The time between clicks
	private float clickRandomizer;							//A random value sampled in determining the time between clicks
	public const float k = 5.263e-6f;						//A constant, K
	public AudioSource[] audioSources;						//An array of all audiosources used to play the click sound in rapid succession
	private int currentAudioSourceIndex;					//The index of the audio source currently playing the click sound. Incremented, or wrapped back to zero every click.
	public GameObject geigerNeedle;							//The needle object on the geiger counter that moves up and down depending on radiation levels
	public float minClickSpeed;								//The radation levels necessary to set the geiger needle to its lowest point
	public float maxClickSpeed;								//The radation levels necessary to set the geiger needle to its highest point
	public float backgroundRadiation;

	public void Update()
	{
        ////Used wand input to toggle the detector on and off
        //if (wandInput.item_state == 1)
        //{
        //    if (canClick)
        //    {
        //        ToggleDetecting();
        //        canClick = false;
        //    }
        //}
        //if (wandInput.item_state == 0)
        //{
        //    canClick = true;
        //}


    }

    //Start detecting/clicking
    public void StartDetecting()
	{
		//If it's detecting, stop it so the rountines don't double up
		StopDetecting ();

		//Cache and start the detection routine
		currentDetectionRoutine = DetectionRoutine ();
		StartCoroutine (currentDetectionRoutine);

		//Cache and start the clicking routine
		currentClickRoutine = ClickRoutine ();
		StartCoroutine (currentClickRoutine);
	}

	//Stop detecting/clicking
	public void StopDetecting()
	{
		//Stop and clear the detection routine
		if (currentDetectionRoutine != null)
		{
			StopCoroutine (currentDetectionRoutine);
			currentDetectionRoutine = null;
		}

		//Stop and clear the click routine
		if (currentClickRoutine != null)
		{
			StopCoroutine (currentClickRoutine);
			currentClickRoutine = null;
		}
    }

	//If currently detecting, stop. If not, start.
	public void ToggleDetecting()
	{
		if (currentDetectionRoutine != null)
		{
			StopDetecting ();
		}
		else
		{
			StartDetecting ();
		}
	}

	//RaycastAll doesn't work with nonconvex mesh colliders (why, Unity??), so here is a custom one!
	public static RaycastHit[] RaycastAllNonConvex(Ray ray, float maxDistance, int mask)
	{
		List<RaycastHit> hits = new List<RaycastHit> ();
		bool clear = false;
		RaycastHit hit;

		//While there are no more objects to hit between the origin and the maxDistance...
		while (!clear)
		{
            //...Raycast over and over from RIGHT in front of the point it last hit
			if (maxDistance > 0 && Physics.Raycast (ray, out hit, maxDistance, mask))
			{
				ray.origin = hit.point + (ray.direction * 0.00001f);
				maxDistance -= hit.distance;
				hits.Add (hit);
			}
			else
			{
				clear = true;
			}
		}

		return hits.ToArray ();
	}

	//Coroutine that loops, detecting radiation and taking into account distance and shield objects
	public IEnumerator DetectionRoutine()
	{
		while (true)
		{
			Vector3 tempDetectorPosition = transform.position;
			RadioactiveSourceObject tempRadioactiveSource;
			RaycastHit[] tempRaycastHits;
			RaycastHit tempRaycastHit;
			Ray tempRay = new Ray();
			Vector3 tempSourcePosition;
			ShieldObject tempShieldObject;
			InterveningShield tempInterveningShield;
			float totalClicksPerSecond = 0;

			for (int r = 0; r < RadioactiveSourceManager.instance.radioactiveSources.Count; r++)
			{
				//Initialize radioactive source data
				interveningShields = new List<InterveningShield> ();
				tempRadioactiveSource = RadioactiveSourceManager.instance.radioactiveSources [r];
				tempSourcePosition = tempRadioactiveSource.transform.position;

				//Raycast from detector to source
				for(int i = 0; i < 2; i++)
				{
					//Kind of wack, but basically do this twice, the first time go detector => source, the second time go source => detector
					if (i == 0)
					{
						tempRay = new Ray (tempDetectorPosition, (tempSourcePosition - tempDetectorPosition).normalized);
					}
					else if (i == 1)
					{
						tempRay = new Ray (tempSourcePosition, -tempRay.direction);	//Just flip the direction since it's saved from the previous loop! One less Sqrt
					}

					//Use the ray to raycastall using custom method, collecting all the hits
					tempRaycastHits = RaycastAllNonConvex (tempRay, Vector3.Distance(tempSourcePosition, tempDetectorPosition), layerMask);

					for (int t = 0; t < tempRaycastHits.Length; t++)
					{
						tempRaycastHit = tempRaycastHits [t];
						tempShieldObject = tempRaycastHit.transform.GetComponent<ShieldObject> ();

						//Only use this hit if the object we hit has a ShieldObject component on it
						if (tempShieldObject != null)
						{
							//For each hitpoint, if the shield we hit has not already been added to our list of shields, add it
							//Also, add the hitpoint to that entry in our list of shields

							float distanceToDetector = Vector3.Distance (tempRaycastHit.point, tempDetectorPosition);

							tempInterveningShield = interveningShields.Find (s => s.shieldObject == tempShieldObject);
							if (tempInterveningShield != null)
							{
								tempInterveningShield.hitPoints.Add (new ShieldHit(tempRaycastHit.point, distanceToDetector, i == 0));
							}
							else
							{
								tempInterveningShield = new InterveningShield (tempShieldObject, tempRadioactiveSource);

								tempInterveningShield.hitPoints.Add (new ShieldHit(tempRaycastHit.point, distanceToDetector,  i == 0));
								interveningShields.Add (tempInterveningShield);
							}
						}
					}
				}

				ShieldHit tempHitPoint = null;
				ShieldHit previousPoint = null;
				float tempThickness;
				bool volumeEntered;

				//Loop through all shield, sorting them by distance,
				//then use the facingDirector bool to figure out when the volumes start and end,
				//then add up the thickness of the volumes, and use their shield material values to figure out the total attenutaion
				for (int i = 0; i < interveningShields.Count; i++)
				{
					tempInterveningShield = interveningShields [i];
					int numHitPoints = tempInterveningShield.hitPoints.Count;

					tempInterveningShield.hitPoints.Sort ((x, y) => x.distanceFromDetector.CompareTo (y.distanceFromDetector));
					tempThickness = 0;
					volumeEntered = false;

					for (int h = 0; h < numHitPoints; h++)
					{
						tempHitPoint = tempInterveningShield.hitPoints [h];
						if (previousPoint == null)
						{
							previousPoint = tempHitPoint;
						}

						if (tempHitPoint.facingDetector)
						{
							if (volumeEntered)
							{
								tempThickness += tempHitPoint.distanceFromDetector - previousPoint.distanceFromDetector;
							}
							else
							{
								tempInterveningShield.totalThickness += tempThickness;

								volumeEntered = true;
								tempThickness = 0;
							}
						}
						else
						{
							volumeEntered = false;
							tempThickness += tempHitPoint.distanceFromDetector - previousPoint.distanceFromDetector;
						}

						previousPoint = tempHitPoint;
					}

					tempInterveningShield.totalThickness += tempThickness * (metersToCentimeters ? 100 : 1);
				}

				//Add up the total radation into totalClicksPerSecond
				totalClicksPerSecond += CalculateClicksPerSecond(tempRadioactiveSource);
			}

			totalClicksPerSecond += backgroundRadiation;

			clickDelta = -(1f / totalClicksPerSecond) * Mathf.Log (clickRandomizer);
			float angle = Mathf.Clamp (totalClicksPerSecond, minClickSpeed, maxClickSpeed) * Random.Range (0.97f, 1.02f);
			angle = Map.map (angle, minClickSpeed, maxClickSpeed, 10, 140); 
			geigerNeedle.transform.localRotation = Quaternion.Lerp(geigerNeedle.transform.localRotation, Quaternion.Euler(new Vector3(0, angle, 0)), Time.deltaTime * 6f); ;
			yield return null;
		}
	}

	//Loop constantly, using the delay timer, to play the click sound at a certain rate, with random variation
	public IEnumerator ClickRoutine()
	{
		while (true)
		{
			//Change the randomizer value every loop
			clickRandomizer = Random.value;

			float delayTimer = 0;
			while (delayTimer < 1)
			{
				delayTimer += Time.deltaTime / Mathf.Max(0.025f, clickDelta);

				yield return null;
			}

			PlayClickSound ();
		}
	}

	public void PlayClickSound()
	{
		audioSources [currentAudioSourceIndex].Stop ();
		audioSources [currentAudioSourceIndex].Play ();

		currentAudioSourceIndex++;
		if (currentAudioSourceIndex >= audioSources.Length)
		{
			currentAudioSourceIndex = 0;
		}
	}

	//Using a radioactive source, and our list of intervening materials, and also the Log formula for randomizing the click timing,
	//Figure out how long between each clicks depending on the levels of radiation
	//Dose caluclations are in here too, just commented out. Not sure what they're for.
	public float CalculateClicksPerSecond(RadioactiveSourceObject radioactiveSource)
	{
		InterveningShield tempInterveningShield;
		float attenuationFactor = 0;
		ShieldMaterial tempShieldMaterial;
		//float dose = 0;
		float clicksPerSecond = 0;
		float distance = Vector3.Distance (transform.position, radioactiveSource.transform.position) * (metersToCentimeters ? 100 : 1);

		for (int e = 0; e < radioactiveSource.E.Length; e++)
		{
			attenuationFactor = 0;

			for (int i = 0; i < interveningShields.Count; i++)
			{
				tempInterveningShield = interveningShields [i];

				if (tempInterveningShield.radioactiveSource == radioactiveSource)
				{
					tempShieldMaterial = tempInterveningShield.shieldObject.shieldMaterial;
					attenuationFactor += tempShieldMaterial.attenuationCoefficients [e] * tempShieldMaterial.density * tempInterveningShield.totalThickness;
				}
			}

			//dose += radioactiveSource.Y [e] * radioactiveSource.E[e] * radioactiveSource.uep[e] * radioactiveSource.B[e] * Mathf.Exp(-attenuationFactor);
			clicksPerSecond += radioactiveSource.Y [e] * Mathf.Exp (-attenuationFactor);
		}

		//dose = k * radioactiveSource.A * dose / (distance * distance);
		clicksPerSecond = radioactiveSource.A * clicksPerSecond / (4f * Mathf.PI * distance * distance);

		return clicksPerSecond;
	}

    void Awake()
    {
        // set up all the audio sources with the clickyclick
        GameObject Audio = new GameObject("Audio");
        Audio.transform.SetParent(this.transform);
        
        audioSources = new AudioSource[20];
        for (int i = 0; i < audioSources.Length; i++)
        {
            audioSources[i] = new GameObject("AudioSource").AddComponent<AudioSource>();
            audioSources[i].clip = Resources.Load("GeigerClick") as AudioClip;
            audioSources[i].playOnAwake = false;
            audioSources[i].volume = 0.6f;
            audioSources[i].spatialBlend = 1;
            audioSources[i].transform.SetParent(Audio.transform);

        }
    }

	void Start()
	{
		//Set up the collision layer mask
		layerMask = (1 << LayerMask.NameToLayer("Shield"));
		//Begin detecting/clicking
		StartDetecting();
	}

    private void OnEnable()
    {
        //for (int i = 0; i < audioSources.Length; i++)
        //{
        //    audioSources[i].mute = false;
        //}
        ToggleDetecting();
    }
    private void OnDisable()
    {
        //for (int i = 0; i < audioSources.Length; i++)
        //{
        //    audioSources[i].mute = true;
        //}
        ToggleDetecting();
        geigerNeedle.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
    }
}
