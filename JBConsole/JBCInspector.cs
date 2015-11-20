using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEngine;

[RequireComponent(typeof(JBConsole))]
public class JBCInspector : MonoBehaviour {


    JBConsole console;
    private ConsoleLog focusedLog;
    private object focusedObject;

    Vector2 scrollPosition;

    int inheritanceLevel = 0;

	void Start ()
    {
        console = GetComponent<JBConsole>();
        console.OnLogSelectedHandler += OnLogSelectedHandler;


        // testing
	    //focusedObject = console;
        //console.Focus(DrawGuiBodyHandler);
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
            if (focusedObject != null)
            {
                focusedObject = null;
            }
            else
            {
                focusedLog = null;
                console.Defocus();
            }
        }

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, maxwidthscreen);

        if (focusedObject != null)
        {
            DrawObjectInspection();
        }
        else if (focusedLog != null)
        {
            DrawLogInspection(maxwidthscreen);
        }

        GUILayout.EndScrollView();
    }

    private void DrawLogInspection(GUILayoutOption maxwidthscreen)
    {
		GUILayout.Label(focusedLog.GetUnityLimitedMessage(), console.style.GetStyleForLogLevel(focusedLog.level), maxwidthscreen);


        string stack = "";
        if (focusedLog.stackTrace != null)
        {
            int linenum;
            foreach (StackFrame stackFrame in focusedLog.stackTrace.GetFrames())
            {
                linenum = stackFrame.GetFileLineNumber();
                var filename = Path.GetFileNameWithoutExtension(stackFrame.GetFileName());
                stack += filename + ":\t" + stackFrame.GetMethod() + (linenum > 0 ? " @ " + linenum : "") + "\n";
            }
        }
        else
        {
            stack = "Stack trace disabled";
        }
        
        GUILayout.Label(stack, maxwidthscreen);

        if (GUILayout.Button("Copy", console.style.MenuStyle))
        {
            var str = focusedLog.message + "\n" + stack;
            Type T = typeof(GUIUtility);
            PropertyInfo systemCopyBufferProperty = T.GetProperty("systemCopyBuffer", BindingFlags.Static | BindingFlags.NonPublic);
            systemCopyBufferProperty.SetValue(null, str, null);
        }

        if (focusedLog.references != null)
        {
            GUILayout.Label("References: ");
            foreach (var weakref in focusedLog.references)
            {
                if (weakref.IsAlive)
                {
                    if (GUILayout.Button(weakref.Target.GetType().Name))
                    {
                        focusedObject = weakref.Target;
                    }
                }
                else GUILayout.Label("(dead link)");
            }
        }
    }

    private void DrawObjectInspection()
    {
        var type = focusedObject.GetType();

        var typeThreshold = GetTypeAtDepth(type, inheritanceLevel);

        GUIStyle headerstyle = console.style.GetStyleForLogLevel(ConsoleLevel.Warn);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Inheritance level: " + typeThreshold, headerstyle);
        if (GUILayout.Button("+"))
        {
            if (typeThreshold.BaseType != null) inheritanceLevel++;
        }
        if (GUILayout.Button("-"))
        {
            if (typeThreshold != type) inheritanceLevel--;
        }
        GUILayout.EndHorizontal();


        object value;
        GUILayout.Label("Fields: ", headerstyle);
        foreach (var field in type.GetFields())
        {
            if (ShouldShowDeclaredType(type, field.DeclaringType) == false)
                continue;
            value = field.GetValue(focusedObject);
            if(DrawFieldValue(field.Name + ":" + field.FieldType, value)) return;
        }

        GUILayout.Label("Properties: ", headerstyle);
        foreach (var property in type.GetProperties())
        {
            if (ShouldShowDeclaredType(type, property.DeclaringType) == false)
                continue;
            value = property.GetValue(focusedObject, null);
            if (DrawFieldValue(property.Name + ":" + property.PropertyType, value)) return;
        }
        GUILayout.Label("Methods: ", headerstyle);
        foreach (var method in type.GetMethods())
        {
            if (ShouldShowDeclaredType(type, method.DeclaringType) == false)
                continue;
            GUILayout.Label(method.ToString());
        }
        GUILayout.Label("Events: ", headerstyle);
        foreach (var e in type.GetEvents())
        {
            if (ShouldShowDeclaredType(type, e.DeclaringType) == false)
                continue;
            GUILayout.Label(e.ToString());
        }
    }

    bool DrawFieldValue(string prefix, object value)
    {
        string str;
        if (value != null)
        {
            str = value.ToString();
            if (str.Length > 30) str += " ...";

            GUILayout.BeginHorizontal();

            GUILayout.Label(prefix + " = ");
            if (GUILayout.Button(str))
            {
                focusedObject = value;
                return true;
            }

            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.Label(prefix + " = " + value);
        }
        return false;
    }

    bool ShouldShowDeclaredType(System.Type currentType, System.Type compairingType)
    {
        int level = 0;
        while (inheritanceLevel >= level && currentType != null)
        {
            if (currentType == compairingType)
            {
                return true;
            }
            level++;
            currentType = currentType.BaseType;
        }
        return false;
    }

    System.Type GetTypeAtDepth(System.Type currentType, int depth)
    {
        while (depth > 0)
        {
            depth--;
            if (currentType.BaseType != null) currentType = currentType.BaseType;
            else return currentType;
        }
        return currentType;
    }
}
