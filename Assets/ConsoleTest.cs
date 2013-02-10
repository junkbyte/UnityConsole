using UnityEngine;
using System.Collections;

public class ConsoleTest : MonoBehaviour {

    JBConsole console;

    float time;

	// Use this for initialization
	void Start ()
    {
        JBConsole.Add(ConsoleLevel.Info, "Test Info");
        JBConsole.Add(ConsoleLevel.Debug, "Test", "test Debug");
        JBConsole.Add(ConsoleLevel.Debug, "Test", "test Debug");
        JBConsole.Add(ConsoleLevel.Warn, "Test", "Warn");
        JBConsole.Add(ConsoleLevel.Warn, "Test", "Warn");
        JBConsole.Add(ConsoleLevel.Warn, "Test", "Warn");
        JBConsole.Add(ConsoleLevel.Warn, "Test", "Warn");
        JBConsole.Add(ConsoleLevel.Error, "Test", "Error");
        JBConsole.AddCh("myChannel", "Test myChannel");
        JBConsole.AddCh("myChannel 2", "Test myChannel 2");
	}
	
	// Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if (time < 3f)
        {
            JBConsole.Add(ConsoleLevel.Info, "Test " + Random.value);
        }
	}
}
