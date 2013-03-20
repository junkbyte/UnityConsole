using System;
using UnityEngine;

public delegate string JBCEmailFormatter(ConsoleLog log);

public class JBCEmail : MonoBehaviour
{
	JBConsole console;

    public string To;
    public string Subject = "Console Log";

    public JBCEmailFormatter Formatter;

	
	void Start ()
	{
        if (Formatter == null)
        {
            Formatter = DefaultFormatter;
        }

		console = GetComponent<JBConsole>();
        console.AddCustomMenu("Email", SendEmail);
	}

    void OnDestroy()
    {
        console.RemoveCustomMenu("Email");
    }

    public void SendEmail()
    {
        string body = "";
        foreach (ConsoleLog log in JBLogger.instance.Logs)
        {
            body += Formatter(log);
        }
        var to = string.IsNullOrEmpty(To) ? "" : To;
        body = "mailto:" + to + "?" + URLPart("subject=", Subject) + URLPart("&body=", body);
        Application.OpenURL(body);
    }

    string URLPart(string prefix, string body)
    {
        if (string.IsNullOrEmpty(body))
        {
            return "";
        }
        return prefix + WWW.EscapeURL(body).Replace("+", "%20");
    }

    private string DefaultFormatter(ConsoleLog log)
    {
        return log.channel + " " + log.level + " " + log.content.text + "\n";
    }

    public static JBCEmail RegisterToConsole(string to = null, string subject = null)
	{
		if(JBConsole.exists)
		{
            var jbcemail = JBConsole.instance.gameObject.AddComponent<JBCEmail>();
		    if (string.IsNullOrEmpty(to) == false) jbcemail.To = to;
		    if (string.IsNullOrEmpty(subject) == false) jbcemail.Subject = subject;
		    return jbcemail;
		}
		return null;
	}
}