using System;
using System.Text;
using UnityEngine;

public delegate string JBCEmailFormatter(ConsoleLog log);
public delegate string JBCEmailPostFormatter(string body);

[RequireComponent(typeof(JBConsole))]
public class JBCEmail : MonoBehaviour
{
	JBConsole console;

    public string To;
    public string Subject = "Console Log";

    public JBCEmailFormatter Formatter;
    public JBCEmailPostFormatter PostFormatter;

	public bool Inverted;
	
	void Awake ()
	{
		console = GetComponent<JBConsole>();
        if (console != null) console.Menu.Add("Email", SendEmail);
	}

    void OnDestroy()
    {
		if(console) console.Menu.Remove("Email");
    }

    public void SendEmail()
    {
        lock (JBLogger.instance)
        {

            if (Formatter == null) Formatter = DefaultFormatter;

			var str = new StringBuilder();

			if(Inverted)
			{
				var logs = JBLogger.instance.Logs;
				for(var i = logs.Count - 1; i >= 0; i--)
				{
					str.Append(Formatter(logs[i]));
				}
			}
			else
			{
				foreach (ConsoleLog log in JBLogger.instance.Logs)
				{
					str.Append(Formatter(log));
				}
			}

			var body = str.ToString();
			if (PostFormatter != null) body = PostFormatter(body);

            var to = string.IsNullOrEmpty(To) ? "" : To;
            body = "mailto:" + to + "?" + URLPart("subject=", Subject) + URLPart("&body=", body);

            Application.OpenURL(body);
        }
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
        return log.Time.ToString("hh:mm:ss.fff") + " - " + log.channel + " " + log.level + " " + log.message + "\n";
    }

    public static JBCEmail RegisterToConsole(string to = null, string subject = null)
	{
		if(JBConsole.Exists)
		{
            var jbcemail = JBConsole.instance.RegisterPlugin<JBCEmail>();
		    if (string.IsNullOrEmpty(to) == false) jbcemail.To = to;
		    if (string.IsNullOrEmpty(subject) == false) jbcemail.Subject = subject;
		    return jbcemail;
		}
		return null;
	}
}