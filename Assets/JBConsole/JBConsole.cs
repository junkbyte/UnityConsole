using UnityEngine;
using System.Collections.Generic;
using System;

public delegate void MenuHandler();

public class JBConsole : MonoBehaviour
{
	
	private static JBConsole _instance;
	
	public static JBConsole Start()
	{
		if(_instance == null)
		{
			GameObject go = new GameObject("JBConsole");
			_instance = go.AddComponent<JBConsole>();
		}
		return _instance;
	}
	
	public static JBConsole instance
	{
		get
		{
			return _instance;
		}
	}
	
	public static bool exists
	{
		get { return _instance != null; }
	}
	
	private JBLogger logger;

    public bool visible = false;
    public int menuItemWidth = 100;
	public KeyCode toggleKey = KeyCode.BackQuote;
	
    string[] levels;
    string[] topMenu;
    Dictionary<int, MenuHandler> customMenuHandlers;

    ConsoleLevel viewingLevel = ConsoleLevel.Debug;
    string[] viewingChannels;

    int currentTopMenuIndex = -1;
	string[] customMenus;
    string[] currentTopMenu;
    string[] currentSubMenu;
    SubMenuHandler subMenuHandler;
	
	List<ConsoleLog> cachedLogs;

    string searchTerm = "";

	bool autoScrolling = true;
	
	Rect autoscrolltogglerect = new Rect(0, 0, 100, 22);

    Vector2 scrollPosition;
	
	int stateHash;
		
	public JBConsole()
	{
		logger = JBLogger.instance;
	}
	
	void Awake ()
	{
		if(_instance == null) _instance = this;
		
        levels = Enum.GetNames(typeof(ConsoleLevel));
        topMenu = currentTopMenu = Enum.GetNames(typeof(ConsoleMenu));

        customMenus = new string[0];
        customMenuHandlers = new Dictionary<int, MenuHandler>();
	}
	
	void Update ()
    {
		if(logger.stateHash != stateHash)
		{
			stateHash = logger.stateHash;
			clearCache();
		}
		if(Input.GetKeyDown(toggleKey))
		{
			visible = !visible;
		}
	}
	
    public static void AddMenu(string name, MenuHandler callback)
	{
		instance.AddCustomMenu(name, callback);
	}
	
    public static void RemoveMenu(string name)
	{
		instance.RemoveCustomMenu(name);
	}

	
    public void AddCustomMenu(string name, MenuHandler callback)
    {
		RemoveMenu(name);
        JBConsoleUtils.AddToStringArray(ref customMenus, name);
        customMenuHandlers[customMenus.Length - 1] = callback;
    }

    public void RemoveCustomMenu(string name)
    {
		for(int i = customMenus.Length - 1; i >= 0; i--)
		{
			if(customMenus[i] == name)
			{
				customMenus = JBConsoleUtils.StringsWithoutIndex(customMenus, i);
				customMenuHandlers.Remove(i);
				return;
			}
		}
    }

    void OnMenuSelection(int index)
    {
        if (currentTopMenuIndex == index)
        {
            index = -1;
        }
        currentSubMenu = null;
        switch (index)
        {
            case (int)ConsoleMenu.Channels:
                UpdateChannelsSubMenu();
                subMenuHandler = OnChannelClicked;
                break;
            case (int)ConsoleMenu.Levels:
                UpdateLevelsSubMenu();
                subMenuHandler = OnLevelClicked;
                break;
            case (int)ConsoleMenu.Search:
                searchTerm = "";
                break;
            case (int)ConsoleMenu.Menu:
                currentSubMenu = customMenus;
                subMenuHandler = OnCustomMenuClicked;
                break;
            case (int)ConsoleMenu.Hide:
                visible = !visible;
                return;
        }
        currentTopMenuIndex = index;
        currentTopMenu = SelectedStateArrayIndex(topMenu, index, true);
    }

    void OnChannelClicked(int index)
    {
        string channel = logger.Channels[index];
        if (channel == JBLogger.allChannelsName)
        {
            viewingChannels = null;
        }
        else if (viewingChannels != null)
        {
            if (Array.IndexOf(viewingChannels, channel) >= 0)
            {
                if (viewingChannels.Length > 1) viewingChannels = JBConsoleUtils.StringsWithoutString(viewingChannels, channel);
                else viewingChannels = null;
            }
            else
            {
                JBConsoleUtils.AddToStringArray(ref viewingChannels, channel);
            }
        }
        else
        {
            viewingChannels = new string[] { channel };
        }
        UpdateChannelsSubMenu();
        clearCache();
    }

    void OnLevelClicked(int index)
    {
        viewingLevel = (ConsoleLevel)Enum.GetValues(typeof(ConsoleLevel)).GetValue(index);
        UpdateLevelsSubMenu();
        clearCache();
    }

    void OnCustomMenuClicked(int index)
    {
        if (customMenuHandlers.ContainsKey(index))
        {
            customMenuHandlers[index]();
        }
    }

    void UpdateChannelsSubMenu()
    {
		var channels = logger.Channels.ToArray();
        currentSubMenu = new string[channels.Length];
        Array.Copy(channels, currentSubMenu, channels.Length);
        if (viewingChannels == null)
        {
            SelectedStateArrayIndex(currentSubMenu, 0, false);
        }
        else
        {
            string channel;
            for (int i = channels.Length - 1; i >= 0; i--)
            {
                channel = channels[i];
                if (Array.IndexOf(viewingChannels, channel) >= 0)
                {
                    SelectedStateArrayIndex(currentSubMenu, i, false);
                }
            }
        }
    }

    void UpdateLevelsSubMenu()
    {
        currentSubMenu = SelectedStateArrayIndex(levels, (int)viewingLevel, true);
    }

    string[] SelectedStateArrayIndex(string[] array, int index, bool copy)
    {
        if (index < 0) return array;
        string[] result;
        if (copy)
        {
            result = new string[array.Length];
            Array.Copy(array, result, array.Length);
        }
        else result = array;
        result[index] = "["+ result[index]+"]";
        return result;
    }
	
	void OnGUI ()
	{
        if (!visible) return;
		/*
        float screenScale = Screen.width / 320f;
	    Matrix4x4 scaledMatrix = Matrix4x4.Scale(Vector3.one * screenScale);
	    GUI.matrix = scaledMatrix;
         */
		DrawGUI(Screen.width, Screen.height);
	}
	
	public void DrawGUI(float width, float height)
	{
        GUILayoutOption maxwidthscreen = GUILayout.MaxWidth(width);

        GUILayout.BeginVertical("box");

        //GUILayout.BeginHorizontal();
		//GUILayout.FlexibleSpace();
        int selection = GUILayout.Toolbar(-1, currentTopMenu, GUILayout.MinWidth(280), GUILayout.MaxWidth(380));
        if (selection >= 0)
        {
            OnMenuSelection(selection);
        }
		//GUILayout.EndHorizontal();

        if (currentSubMenu != null)
        {
			if(currentSubMenu.Length == 0)
			{
				GUILayout.Label("No Custom Menus...");
			}
			else
			{
				selection = GUILayout.SelectionGrid(-1, currentSubMenu, (int)width / menuItemWidth);
	            if (selection >= 0 && subMenuHandler != null)
	            {
	                subMenuHandler(selection);
	            }
			}
        }

        if (currentTopMenuIndex == (int)ConsoleMenu.Search)
        {
            GUI.SetNextControlName("SearchTF");
            string newTerm = GUILayout.TextField(searchTerm);
			if(newTerm != searchTerm)
			{
            	searchTerm = newTerm.ToLower();
				clearCache();
			}
            
            GUI.FocusControl("SearchTF");
        }
		if(autoScrolling)
		{
			//if(cachedLogs == null)
			{
				scrollPosition.y = float.MaxValue;
			}
			
			Vector2 newPosition = GUILayout.BeginScrollView(scrollPosition, maxwidthscreen);
			
			if(scrollPosition.y != float.MaxValue && scrollPosition.y != newPosition.y)
			{
				// User scrolled... TODO
				scrollPosition.y = float.MaxValue;
			}
			else
			{
				scrollPosition = newPosition;
			}
			
			if (cachedLogs == null)
	        {
	            CacheBottomOfLogs(width, height);
	        }
	        for (int i = cachedLogs.Count - 1; i >= 0; i--)
	        {
				PrintLog(cachedLogs[i], maxwidthscreen);
	        }
			GUILayout.EndScrollView();
		}
        else
		{
			scrollPosition = GUILayout.BeginScrollView(scrollPosition, maxwidthscreen);
	        int len = logger.Logs.Count;
			ConsoleLog log;
			for (int i = 0; i < len; i++)
			{
				log = logger.Logs[i];
				if (ShouldShow(log))
				{
					PrintLog(log, maxwidthscreen);
				}
			}
			GUILayout.EndScrollView();
		}
		
		if(!autoScrolling || scrollPosition.y > 0)
		{
			autoscrolltogglerect.x = width - autoscrolltogglerect.width;
			autoscrolltogglerect.y = height - autoscrolltogglerect.height;
			if(GUI.Toggle(autoscrolltogglerect, autoScrolling, "Auto scroll") != autoScrolling)
			{
				autoScrolling = !autoScrolling;
				if(!autoScrolling)
				{
					scrollPosition.y = float.MaxValue;
				}
			}
		}
		
        GUILayout.EndVertical();
		
		if(GUI.tooltip.Length > 0)
		{
			GUI.Label(new Rect(0, Screen.height - Input.mousePosition.y + 10, Screen.width, 100), GUI.tooltip);
		}
	}
	
	void PrintLog(ConsoleLog log, GUILayoutOption maxwidthscreen)
	{
		if(log.repeats > 0)
		{
			GUILayout.Label(log.repeats + "x " +log.content.text, maxwidthscreen);
		}
		else
		{
			GUILayout.Label(log.content, maxwidthscreen);
		}
	}
		
	bool ShouldShow(ConsoleLog log)
	{
		return (log.level >= viewingLevel 
			&& (viewingChannels == null || Array.IndexOf(viewingChannels, log.channel) >= 0)
	        && (searchTerm == "" || log.content.text.ToLower().Contains(searchTerm)));
	}
	
	void CacheBottomOfLogs(float width, float height)
	{
		//TODO, avoid needing to create new list.
		cachedLogs = new List<ConsoleLog>();
		List<ConsoleLog> logs = logger.Logs;
		ConsoleLog log;
		for(int i = logs.Count - 1; i >= 0 && height > 0; i--)
		{
			log = logs[i];
			if (ShouldShow(log))
			{
				cachedLogs.Add(log);
				height -= log.GetHeightForWidth(width);
			}
		}
	}
	
	public void clearCache(){
		cachedLogs = null;
	}
}

delegate void SubMenuHandler(int index);

public enum ConsoleMenu
{
    Channels,
    Levels,
    Search,
    Menu,
    Hide
}