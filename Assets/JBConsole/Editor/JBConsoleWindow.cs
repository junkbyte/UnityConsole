using UnityEngine;
using UnityEditor;
using System.Collections;

public class JBConsoleWindow : EditorWindow
{
	
	void Start () {
	
	}
	
	[MenuItem ("Window/JunkByte Console")]
    static void OpenJunkByteConsoleWindow()
	{
    	EditorWindow.GetWindow(typeof(JBConsoleWindow), false, "JB Console");
    }
	
    void Update ()
	{
		Repaint();
    }
	
	void OnGUI()
	{
		if(JBConsole.exists)
		{
			JBConsole console = JBConsole.instance;
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
		else
		{
			GUILayout.Label("Console not initialized...");
			if(GUILayout.Button("Initialize"))
			{
				JBConsole.Start();
				JBConsole.instance.visible = true;
			}
		}
	}
}
