#if UNITY_EDITOR
using UnityEngine;

[RequireComponent(typeof(JBConsole))]
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
			console.Visible = !console.Visible;
		}
	}
	
	public static JBCToggleOnKey RegisterToConsole()
	{
		if(JBConsole.Exists)	
		{
            return JBConsole.instance.RegisterPlugin<JBCToggleOnKey>();
		}
		return null;
	}
}
#endif