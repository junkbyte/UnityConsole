using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConsoleTest : MonoBehaviour {

    JBConsole console;

    float time;
	float delay;
	int count;

	void Start ()
    {
		JBConsole.Start();
		
		JBCToggleOnKey.RegisterToConsole();
		JBCVisibleOnPress.RegisterToConsole();
		
        JBLogger.Info("Test Info");
        JBLogger.Debug(ConsoleLevel.Debug, "Test", "test Debug");
        JBLogger.Debug(ConsoleLevel.Debug, "Test", "test Debug");
        JBLogger.Warn(ConsoleLevel.Warn, "Test", "Warn");
        JBLogger.Warn(ConsoleLevel.Warn, "Test", "Warn");
        JBLogger.Warn(ConsoleLevel.Warn, "Test", "Warn");
        JBLogger.Warn(ConsoleLevel.Warn, "Test", "Warn");
        JBLogger.Error(ConsoleLevel.Error, "Test", "Error");
        JBLogger.InfoCh("myChannel", "Test myChannel");
        JBLogger.WarnCh("myChannel 2", "Test myChannel 2");
		
		JBLogger.Info (new List<string>(){"bla bla", "and blaaa"});
	}
	
	
    void Update()
    {
        time += Time.deltaTime;
        //if (time < 20f)
        {
			delay += Time.deltaTime;
			if(delay >= 0.25f)
			{
				delay = 0;
				JBLogger.Log(ConsoleLevel.Info, "Test " + count + " - " + Random.value);
				count ++;
			}
            
        }
	}
}
