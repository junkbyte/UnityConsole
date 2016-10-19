using System;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;

[RequireComponent(typeof(JBConsole))]
public class JBPasswordEntry : MonoBehaviour
{
	const string SAVED_PASSWORD_KEY = "JB_Last_Password";

    public string Question;
    public string Password;
	public bool SaveLastPassword = true;
	public bool Md5Hashed;

    private JBConsole console;
    private bool accepted;
    private bool entering;
    private string textEntered = "";

    void Start()
    {
        console = GetComponent<JBConsole>();
        console.OnVisiblityChanged += OnVisiblityChanged;
    }

	public bool Accepted
	{
		get { return string.IsNullOrEmpty(Password) || accepted || GetSavedPassword() == Password; }
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
			var enteredhash = Md5Hashed ? Hash(textEntered) : textEntered;
            if (GUILayout.Button(" Cancel ", GUILayout.Height(height)))
            {
                entering = false;
            }
			else if (enteredhash == Password)
            {
                accepted = true;
				SetSavedPassword(enteredhash);
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

    public static JBPasswordEntry RegisterHashedToConsole(string md5, string question = null)
    {
        if (JBConsole.Exists)
        {
            var comp = JBConsole.instance.RegisterPlugin<JBPasswordEntry>();
			comp.Password = md5;
            comp.Question = question;
			comp.Md5Hashed = true;
            return comp;
        }
        return null;
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

	const string SALT = "*lu*";
	
	public static string Hash(string str)
	{
		str = SALT + str + SALT;
		byte[] bytes = new byte[str.Length * sizeof(char)];
		System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
		var md5 = MD5.Create();
		bytes = md5.ComputeHash(bytes);
		var result = new StringBuilder(bytes.Length * 2);
		for (int i = 0; i < bytes.Length; i++)
		{
			result.Append(bytes[i].ToString("x2"));
		}
		return result.ToString();
	}
}