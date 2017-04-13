using UnityEngine;
using System.Collections;

public class ChangeScene : MonoBehaviour 
{

	float Counter = 5.0f;
	public string NewScene;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		Counter -= Time.deltaTime;
		if(Counter <= 0.0f)
		{
			UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(NewScene);
			Counter = 100000.0f;
		}
	}
}
