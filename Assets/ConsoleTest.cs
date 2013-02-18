using UnityEngine;
using System.Collections;

public class ConsoleTest : MonoBehaviour {

    JBConsole console;

    float time;
	int count;

	void Start ()
    {
		JBConsole console = JBConsole.Start();
		
		JBCToggleOnKey.RegisterToConsole();
		JBCVisibleOnPress.RegisterToConsole();
		
        JBLogger.Add(ConsoleLevel.Info, "Test Info");
        JBLogger.Add(ConsoleLevel.Debug, "Test", "test Debug");
        JBLogger.Add(ConsoleLevel.Debug, "Test", "test Debug");
        JBLogger.Add(ConsoleLevel.Warn, "Test", "Warn");
        JBLogger.Add(ConsoleLevel.Warn, "Test", "Warn");
        JBLogger.Add(ConsoleLevel.Warn, "Test", "Warn");
        JBLogger.Add(ConsoleLevel.Warn, "Test", "Warn");
        JBLogger.Add(ConsoleLevel.Error, "Test", "Error");
        JBLogger.AddCh("myChannel", "Test myChannel");
        JBLogger.AddCh("myChannel 2", "Test myChannel 2");
	}
	
	
    void Update()
    {
        time += Time.deltaTime;
        if (time < 3f)
        {
            JBLogger.Add(ConsoleLevel.Info, "Test " + count + " - " + Random.value);
			count ++;
        }
	}
}
