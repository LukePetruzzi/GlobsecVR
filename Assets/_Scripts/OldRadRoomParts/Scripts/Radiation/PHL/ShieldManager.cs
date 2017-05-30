using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//A shield material, with all its values
[System.Serializable]
public class ShieldMaterial
{
	public string name;
	public float density;
	public List<float> buildupFactors;
	public List<float> attenuationCoefficients;

	public ShieldMaterial()
	{
		name = "";
		density = 0;
		buildupFactors = new List<float> ();
		attenuationCoefficients = new List<float> ();
	}

	//Copy constructor
	public ShieldMaterial(ShieldMaterial oldMaterial)
	{
		name = oldMaterial.name;
		density = oldMaterial.density;
		buildupFactors = new List<float>(oldMaterial.buildupFactors);
		attenuationCoefficients = new List<float>(oldMaterial.attenuationCoefficients);
	}
}

//A list of all the shield materials found in the txt file
public class ShieldManager : MonoBehaviour
{
	public static ShieldManager instance;				//Singleton instance
	public delegate void ActionHandler();				//A blank delegate
	public List<ShieldMaterial> shieldMaterials;		//List of all shield materials

	public static event ActionHandler PopulateEvent;	//An event raised once all materials are populated

	//A wrapper for PopulateEvent, to easily check for null
	public void RaisePopulateEvent()
	{
		if (PopulateEvent != null)
		{
			PopulateEvent ();
		}
	}

	void Awake()
	{
		//Initialized singleton instance
		instance = this;
	}
		
	void Start()
	{
		//Populate the list at start
		Populate (@"Artificial_ShieldAttenuationProperties.txt");
	}

	//Populate the list from materials in file
	public void Populate(string filePath)
	{
		//Initialize list
		shieldMaterials = new List<ShieldMaterial> ();

		//Open file
		System.IO.StreamReader file = new System.IO.StreamReader(filePath);
		string fileLine;

		//Temp ShieldMaterial object
		ShieldMaterial shieldMaterial = new ShieldMaterial ();

		//Used to check if this is the first material in the file or not
		bool initialized = false;

		//Loop through all lines
		while ((fileLine = file.ReadLine ()) != null)
		{
			//@ indicates the start of a new material
			if (fileLine.StartsWith ("@"))
			{
				//If it's not the first material in the file, add the previous one using the copy constructor
				if (initialized)
				{
					shieldMaterials.Add (new ShieldMaterial (shieldMaterial));
				}
				else
				{
					initialized = true;
				}

				//Set the name, taking off the first two characters, which are @ and a space
				shieldMaterial.name = fileLine.Substring (2, fileLine.Length - 2);
			}
			//$ indicates a value for the current material
			else if (fileLine.StartsWith ("$"))
			{
				//Add the density value to the material
				if (fileLine.Contains ("density"))
				{
					fileLine = file.ReadLine ();
					shieldMaterial.density = float.Parse (fileLine);
				}
				//Add all the buildup factors to the material
				else if (fileLine.Contains ("buildup factor"))
				{
					fileLine = file.ReadLine ();
					while (fileLine.StartsWith ("#"))
					{
						fileLine = file.ReadLine ();
					}

					if (fileLine != null)
					{
						string[] buildupValues = fileLine.Split (' ');

						for (int b = 0; b < buildupValues.Length; b++)
						{
							shieldMaterial.buildupFactors.Add (float.Parse(buildupValues [b]));
						}
					}
				}
				//Add all the attenuation coefficients to the material
				else if (fileLine.Contains ("attenuation coefficient"))
				{
					fileLine = file.ReadLine ();
					while (fileLine.StartsWith ("#"))
					{
						fileLine = file.ReadLine ();
					}

					if (fileLine != null)
					{
						string[] attenuationValues = fileLine.Split (' ');

						for (int a = 0; a < attenuationValues.Length; a++)
						{
							shieldMaterial.attenuationCoefficients.Add (float.Parse(attenuationValues [a]));
						}
					}
				}
			}
		}

		//Add the last material in the file
		if (shieldMaterial != null)
		{
			shieldMaterials.Add (new ShieldMaterial (shieldMaterial));
		}

		//Close file
		file.Close ();

		//Raise the event that signifies it's done populating
		RaisePopulateEvent ();
	}
}
