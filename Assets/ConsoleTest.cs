using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class ConsoleTest : MonoBehaviour {

    JBConsole console;

	float delay;
	int count;

    private bool spam;

	void Start ()
    {
		JBConsole.Start();
		
		JBCToggleOnKey.RegisterToConsole();
		JBCVisibleOnPress.RegisterToConsole();
	    JBCEmail.RegisterToConsole("luaye@junkbyte.com", "Test subject");
		
        JBLogger.Info("Test Info");
        JBLogger.Debug("Test", "test Debug");
        JBLogger.Debug("Test", "test Debug repeat");
        JBLogger.Debug("Test", "test Debug repeat");
        JBLogger.Debug("Test", "test Debug repeat");
        JBLogger.Debug("Test", "test Debug repeat");
        JBLogger.Warn("Test", "Warn");
        JBLogger.Warn("Test", "Warn");
        JBLogger.Warn("Test", "Warn Abcd and numbers:", 1, 2, 3, 4, 5);
        JBLogger.Warn("Test", "Warn <b>BOLD HERE</b>");
        JBLogger.Error("Test", "Error");
        JBLogger.InfoCh("myChannel", "Test info on myChannel");
        JBLogger.WarnCh("myChannel 2", "Test warn on myChannel 2");
        JBLogger.FatalCh("myChannel 2", "Test fatal on myChannel 2");

        JBLogger.Info("Console: ", JBConsole.instance);

	    var list = new List<string>() {"bla bla", "and blaaa"};
        JBLogger.Info(list);
        var arr = list.ToArray();
        JBLogger.Info("arr:", arr);

        JBConsole.AddMenu("spam", OnSpamMenuClicked);
	}

    private void OnSpamMenuClicked()
    {
        spam = !spam;
    }


    void Update()
    {
        {
			delay += Time.deltaTime;
            if (delay >= 0.25f && spam)
			{
				delay = 0;
				JBLogger.Log(ConsoleLevel.Info, "Test " + count + " - " + Random.value);
				count ++;
			}
            
        }
	}
}
