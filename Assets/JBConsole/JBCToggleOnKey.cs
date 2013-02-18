using UnityEngine;
using System.Collections;

public class JBCToggleOnKey : MonoBehaviour {
	
	public KeyCode key = KeyCode.BackQuote;
	
	JBConsole console;
	
	void Start ()
	{
		console = GetComponent<JBConsole>();
	}
	
	void Update ()
	{
		if(Input.GetKeyDown(key))
		{
			console.visible = !console.visible;
		}
	}
	
	public static JBCToggleOnKey RegisterToConsole()
	{
		if(JBConsole.exists)	
		{
			return JBConsole.instance.gameObject.AddComponent<JBCToggleOnKey>();
		}
		return null;
	}
}
