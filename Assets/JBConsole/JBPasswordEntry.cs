using System;
using UnityEngine;

[RequireComponent(typeof(JBConsole))]
public class JBPasswordEntry : MonoBehaviour
{
	const string SAVED_PASSWORD_KEY = "JB_Last_Password";

    public string Question;
    public string Password;
	public bool SaveLastPassword = true;

    private JBConsole console;
    private bool accepted;
    private bool entering;
    private string textEntered = "";

    void Start()
    {
        console = GetComponent<JBConsole>();
        console.OnVisiblityChanged += OnVisiblityChanged;
    }

    private void OnVisiblityChanged()
    {
        if (console.Visible && !accepted && !string.IsNullOrEmpty(Password))
        {
			var last = GetSavedPassword();
			if(last == Password)
			{
				entering = false;
				accepted = true;
			}
			else
			{
				console.Visible = false;
				entering = true;
			}
        }
    }

    void Update()
    {
        if (entering && accepted)
        {
            textEntered = "";
            entering = false;
            console.Visible = true;
        }
    }

    void OnGUI()
    {
        if (entering)
        {
            var height = 50;
            GUILayout.BeginHorizontal(console.style.BoxStyle);
            if (!string.IsNullOrEmpty(Question)) GUILayout.Label(Question, GUILayout.Height(height));

            textEntered = GUILayout.PasswordField (textEntered, '*', GUILayout.MaxWidth(Screen.width / 2f), GUILayout.Height(height));

            if (GUILayout.Button(" Cancel ", GUILayout.Height(height)))
            {
                entering = false;
            }
            else if (textEntered == Password)
            {
                accepted = true;
				SetSavedPassword(textEntered);
            }
            GUILayout.EndHorizontal();
        }
	}
	
	string GetSavedPassword()
	{
		if(SaveLastPassword) return PlayerPrefs.GetString(SAVED_PASSWORD_KEY, null);
		return null;
	}
	
	void SetSavedPassword(string v)
	{
		if(SaveLastPassword) PlayerPrefs.SetString(SAVED_PASSWORD_KEY, v); // md5 would increase security, but for what purpose...
	}

    public static JBPasswordEntry RegisterToConsole(string password, string question = null)
    {
        if (JBConsole.Exists)
        {
            var comp = JBConsole.instance.RegisterPlugin<JBPasswordEntry>();
            comp.Password = password;
            comp.Question = question;
            return comp;
        }
        return null;
    }
}