using UnityEngine;
using System.Collections;

//A radioactive object
public class RadioactiveSourceObject : MonoBehaviour
{
	// source activity [Bq = disintegrations/sec]
	public float A = 0.5e9f;

	// photon yield [photons/disintegration]
	public float[] Y =   {0.7f , 0.2f , 0.1f};

	// photon energies [MeV]
	public float[] E =   {0.1f , 1.0f , 2.5f};

	// mass-energy absorption coefficient of absorber [cm^2/g]
	public float[] uep = {0.2f  , 0.02f , 0.03f};

	// effective buildup factor from nucleonica; if all 1's then calculation neglects this effect
	public float[] B =   {1.0f , 1.0f , 1.0f};

	public float distanceToDetector = 0;
}
