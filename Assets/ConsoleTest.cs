using UnityEngine;
using System.Collections;

public class ConsoleTest : MonoBehaviour {

    JBConsole console;

    float time;
	int count;

	// Use this for initialization
	void Start ()
    {
		JBConsole.InitIfRequired();
		
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
	
	// Update is called once per frame
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
