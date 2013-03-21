using System.Diagnostics;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(JBConsole))]
public class JBCInspector : MonoBehaviour {


    JBConsole console;
    private ConsoleLog focusedLog;

    Vector2 scrollPosition;

	void Start ()
    {
        console = GetComponent<JBConsole>();
        console.OnLogSelectedHandler = OnLogSelectedHandler;
	}

    void Update ()
    {
	
	}

    private void OnLogSelectedHandler(ConsoleLog log)
    {
        focusedLog = log;
        console.Focus(DrawGuiBodyHandler);
    }

    private void DrawGuiBodyHandler(float width, float height, float scale)
    {
        GUILayoutOption maxwidthscreen = GUILayout.MaxWidth(width);

        if (GUILayout.Button("Back", console.style.MenuStyle))
        {
            console.Defocus();
        }
        else
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, maxwidthscreen);
            GUILayout.Label(focusedLog.message, console.style.GetStyleForLogLevel(focusedLog.level), maxwidthscreen);


            string stack = "";
            int linenum;
            foreach (StackFrame stackFrame in focusedLog.stackTrace.GetFrames())
            {
                linenum = stackFrame.GetFileLineNumber();
                var filename = Path.GetFileNameWithoutExtension(stackFrame.GetFileName());
                stack += filename + ":\t" + stackFrame.GetMethod() + (linenum > 0 ? " @ " + linenum : "") + "\n";
            }
            GUILayout.Label(stack, maxwidthscreen);

            if (focusedLog.references != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("References: ");
                foreach (var weakref in focusedLog.references)
                {
                    if (weakref.IsAlive) GUILayout.Label(weakref.Target.ToString());
                    else GUILayout.Label("(dead link)");
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }
    }
}
