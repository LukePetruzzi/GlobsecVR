using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Holds a list of all the radioactive sources in the scene
public class RadioactiveSourceManager : MonoBehaviour
{
	public static RadioactiveSourceManager instance;			//Singleton instance
	public List<RadioactiveSourceObject> radioactiveSources;	//List of radioactive sources

	void Awake()
	{
		//Assign singleton
		instance = this;
	}

	void Start()
	{
		//Populate on start
		Populate ();
	}

	public void Populate()
	{
		//Find all radioactive source objects in the scene, populate the list with them
		radioactiveSources = new List<RadioactiveSourceObject> (GameObject.FindObjectsOfType<RadioactiveSourceObject> ());
	}
}
