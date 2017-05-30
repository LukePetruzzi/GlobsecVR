using UnityEngine;
using System.Collections;

//An object with a collider and a shield material
public class ShieldObject : MonoBehaviour
{
	public bool useObjectName;				//Toggle this on if you'd like to use the name of this gameobject to look up the shield material
	public bool includeChildren;			//Toggle this on if you'd like to add this object to all valid children objects as well
	public string shieldMaterialName;		//Use this shield material name, if useObjectName is not checked
	public ShieldMaterial shieldMaterial;	//The material used by this object. Taken from the list in ShieldManager

	void Awake()
	{
		Initialize ();
	}

	public void Initialize()
	{
		//Set the name we will use for looking up the shield material
		if (useObjectName)
		{
			shieldMaterialName = gameObject.name;
		}

		//If we are including children, recursively go through all children and all it to all valid objects, unless it already has a ShieldObject component
		if (includeChildren)
		{
			foreach (Transform child in transform)
			{
				if (child.GetComponent<ShieldObject> () == null)
				{
					child.gameObject.AddComponent<ShieldObject> ();
					child.gameObject.GetComponent<ShieldObject> ().shieldMaterialName = shieldMaterialName;
					child.gameObject.GetComponent<ShieldObject> ().useObjectName = false;
					child.gameObject.GetComponent<ShieldObject> ().includeChildren = true;
					child.gameObject.GetComponent<ShieldObject> ().Initialize ();
				}
			}
		}

		//If this object doesn't have a collider, destroy this component, because it won't get picked up by the detector
		if (GetComponent<Collider> () == null)
		{
			Destroy (this);
		}
		else
		{
			//When the ShieldManager populate event is raised, find and assign the correct material
			ShieldManager.PopulateEvent += AssignShieldMaterial;
		}
	}

	//Find and assign the correct shield material
	public void AssignShieldMaterial()
	{
		shieldMaterial = ShieldManager.instance.shieldMaterials.Find (s => s.name == shieldMaterialName);
	}
}
