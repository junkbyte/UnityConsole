using UnityEngine;
using UnityEditor;
using System.Collections;

public class JBConsoleWindow : EditorWindow
{
	
	JBConsole console;
	
	void Start () {
	
	}
	
	[MenuItem ("Window/JunkByte Console")]
    static void OpenJunkByteConsoleWindow()
	{
    	EditorWindow.GetWindow(typeof(JBConsoleWindow), false, "JB Console");
    }
	
    void Update ()
	{
		if(JBConsole.instance != null)
		{
			console = JBConsole.instance;
		}
		Repaint();
    }
	
	void OnGUI()
	{
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
				console.DrawGUI(position.width, position.height);
			}
		}
	}
}
