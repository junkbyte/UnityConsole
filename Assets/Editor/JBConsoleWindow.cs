using UnityEngine;
using UnityEditor;
using System.Collections;

public class JBConsoleWindow : EditorWindow {

	void Start () {
	
	}
	
	[MenuItem ("Window/JunkByte Console")]
    static void OpenJunkByteConsoleWindow()
	{		
    	EditorWindow.GetWindow(typeof(JBConsoleWindow), false, "JB Console");
    }
	
	void OnGUI()
	{
		if(!Application.isPlaying)
		{
			GUILayout.Label("JunkByte Console will run in play mode.");
			return;
		}
		JBConsole console = JBConsole.instance;
		if(console != null)
		{
			if(console.visible)
			{
				GUILayout.Label("JunkByte Console visible in game.");
				if(GUILayout.Button("Show"))
				{
					console.visible = false;
				}
			}
			else
			{
				console.DrawGUI();
			}
			
		}
	}
}
