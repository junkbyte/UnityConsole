using System;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;

[RequireComponent(typeof(JBConsole))]
public class JBEmailEntry : MonoBehaviour
{
	const string SAVED_EMAIL_KEY = "JB_Last_Email";

    private string textEntered;
    private JBConsole console;
    
    void Start()
    {
        console = GetComponent<JBConsole>();
        textEntered = GetSavedEmail();
    }        

    void OnGUI()
    {
        var height = 50;
        GUILayout.BeginHorizontal(console.style.BoxStyle);
        GUILayout.Label("Enter Email Address", GUILayout.Height(height));

        textEntered = GUILayout.TextField(textEntered, GUILayout.MaxWidth(Screen.width / 2f), GUILayout.Height(height));
		if (GUILayout.Button(" Cancel ", GUILayout.Height(height)))
		{
			Destroy(this);
		}
        if (GUILayout.Button(" Ok ", GUILayout.Height(height)))
        {
            SetEmail(textEntered);
            Destroy(this);
        }     
	}
	
	public static string GetSavedEmail()
	{
		return PlayerPrefs.GetString(SAVED_EMAIL_KEY, null);
	}
	
	static void SetEmail(string v)
	{
		PlayerPrefs.SetString(SAVED_EMAIL_KEY, v); // md5 would increase security, but for what purpose...
	}
    
}