using UnityEngine;
using System.Collections;

public class ConsoleTest : MonoBehaviour {

    JBConsole console;

    float time;

	// Use this for initialization
	void Start ()
    {
        console = GetComponent<JBConsole>();

        console.Add(ConsoleLevel.Info, "Test Info");
        console.Add(ConsoleLevel.Debug, "Test", "test Debug");
        console.Add(ConsoleLevel.Debug, "Test", "test Debug");
        console.Add(ConsoleLevel.Warn, "Test", "Warn");
        console.Add(ConsoleLevel.Warn, "Test", "Warn");
        console.Add(ConsoleLevel.Warn, "Test", "Warn");
        console.Add(ConsoleLevel.Warn, "Test", "Warn");
        console.Add(ConsoleLevel.Error, "Test", "Error");
        console.AddCh("myChannel", "Test myChannel");
        console.AddCh("myChannel 2", "Test myChannel 2");
	}
	
	// Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if (time < 3f)
        {
            console.Add(ConsoleLevel.Info, "Test " + Random.value);
        }
	}
}
