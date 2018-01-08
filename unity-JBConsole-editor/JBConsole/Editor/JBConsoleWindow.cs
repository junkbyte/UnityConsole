using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using com.spaceape.jbconsole;

public class JBConsoleWindow : EditorWindow
{
	const string ASSETPATH_FONTREFERENCE = "Assets/Resources/JBConsoleFontReference.asset";
	private Font font;

	void Start () {
	
	}
	
	[MenuItem ("Window/JunkByte Console")]
    static void OpenJunkByteConsoleWindow()
	{
    	EditorWindow.GetWindow(typeof(JBConsoleWindow), false, "JB Console");
    }

	private void OnEnable()
	{
		TryInstallDefaultFontReference();
	}

	private static void TryInstallDefaultFontReference()
	{
		var fontReference = AssetDatabase.LoadAssetAtPath<JBConsoleFontReference>(ASSETPATH_FONTREFERENCE);
		if (!fontReference)
		{
			InstallFontReference();
		}
	}

	[MenuItem("SpaceApe/JBConsole/Install Font Reference")]
	static void InstallFontReference()
	{
		var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Plugins/Nuget/SpaceApe.JBConsole/font/RobotoMono-Regular.ttf");
		if (font)
		{
			var fontReference = ScriptableObject.CreateInstance<JBConsoleFontReference>();
			var useMonoSpace = EditorUtility.DisplayDialog("JBConsole Monospace Font",
				"Do you want to use the JBConsole monospace font?\n\t"
				+ (font.fontNames != null && font.fontNames.Length > 0 ? font.fontNames[0] : font.name)
				+ "\nIt would be added linked to your resources folder and available in the game.",
				"Yes, thanks so much!",
				"No, I will do it myself/ignore");
			if (useMonoSpace)
			{
				fontReference.font = font;
			}
			AssetDatabase.CreateAsset(fontReference, ASSETPATH_FONTREFERENCE);
			if (!useMonoSpace)
			{
				Selection.activeObject = fontReference;
			}
		}
	}

	void Update ()
	{
		Repaint();
    }
	
	void OnGUI()
	{
		if(!Application.isPlaying)
		{
			GUILayout.Label("Console will be available during play mode.");
			return;
		}
		if(JBConsole.Exists)
		{
			JBConsole console = JBConsole.instance;
			if(console.Visible)
			{
				GUILayout.Label("JunkByte Console Visible in game.");
				if(GUILayout.Button("Show"))
				{
					console.Visible = false;
				}
			}
			else
			{
				if (font && console.Font != font)
				{
					console.SetFont(font);
				}
				console.DrawGUI(position.width, position.height, 1, true);
			}
		}
		else
		{
			GUILayout.Label("Console not initialized...");
			if(GUILayout.Button("Initialize"))
			{
				JBConsole.Start();
				JBConsole.instance.Visible = true;
			}
		}
	}
}
