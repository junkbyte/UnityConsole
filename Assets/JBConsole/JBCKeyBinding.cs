using UnityEngine;
using System;
using System.Collections.Generic;

public class JBCKeyBinding : MonoBehaviour
{
	readonly List<Binding> downBindings = new List<Binding>();
	//readonly List<Binding> upBindings = new List<Binding>();
	readonly List<Binding> holdBindings = new List<Binding>();

	bool showingMap;
	
	void Update ()
	{
		KeyCode[] keys;
		bool matched;
		for(var i = downBindings.Count - 1; i >= 0; i--)
		{
			keys = downBindings[i].keysCombo;
			for(var j = keys.Length - 1; j >= 0; j--)
			{
				if(Input.GetKeyDown(keys[j]))
				{
					matched = true;
					for(var k = keys.Length - 1; k >= 0; k--)
					{
						if(!Input.GetKey(keys[k]))
						{
							matched = false;
							break;
						}
					}
					if(matched)
					{
						downBindings[i].Call();
						i = -1;
						break;
					}
				}
			}
		}
		/*
		for(var i = upBindings.Count - 1; i >= 0; i--)
		{
			matched = true; 
			keys = upBindings[i].keysCombo;
			for(var j = keys.Length - 1; j >= 0; j--)
			{
				if(Input.GetKeyUp(keys[j]))
				{
					matched = true;
					for(var k = keys.Length - 1; k >= 0; k--)
					{
						if(!Input.GetKey(keys[k]))
						{
							matched = false;
							break;
						}
					}
					if(matched)
					{
						upBindings[i].Call();
						i = -1;
						break;
					}
				}
			}
			if(matched)
			{
				upBindings[i].Call();
			}
		}
		*/
		for(var i = holdBindings.Count - 1; i >= 0; i--)
		{
			matched = true; 
			keys = holdBindings[i].keysCombo;
			for(var j = keys.Length - 1; j >= 0; j--)
			{
				if(!Input.GetKey(keys[j]))
				{
					matched = false;
				}
			}
			if(matched)
			{
				holdBindings[i].Call();
				break;
			}
		}
	}

	bool wasConsoleVisibleBeforeToggle;
	public void ToggleShowBindings()
	{
		if(!showingMap || !JBConsole.instance.Visible)
		{
			wasConsoleVisibleBeforeToggle = JBConsole.instance.Visible;
			ShowBindings();
		}
		else
		{
			HideBindings();
			JBConsole.instance.Visible = wasConsoleVisibleBeforeToggle;
		}
	}

	public void ShowBindings()
	{
		showingMap = true;
		JBConsole.instance.Visible = true;
		JBConsole.instance.Focus(DrawBindings);
	}

	public void HideBindings()
	{
		if(showingMap)
		{
			showingMap = false;
			JBConsole.instance.Defocus();
		}
	}

	void DrawBindings(float width, float height, float scale = 1)
	{
		GUILayoutOption maxwidthscreen = GUILayout.MaxWidth(width);
		var console = JBConsole.instance;

		var closebtnname = "CLOSE KEY BINDINGS MAP";
		if(GUILayout.Button(closebtnname, console.style.MenuStyle))
		{
			HideBindings();
		}
		List<Binding> sortCopy;
		if(downBindings.Count > 0)
		{
			sortCopy = new List<Binding>(downBindings);
			sortCopy.Sort((a, b) => a.ComboString.CompareTo(b.ComboString));
			foreach(var binding in sortCopy)
			{
				GUILayout.Label(binding.ComboString, maxwidthscreen);
			}
		}
		if(holdBindings.Count > 0)
		{
			sortCopy = new List<Binding>(holdBindings);
			sortCopy.Sort((a, b) => a.ComboString.CompareTo(b.ComboString));
			foreach(var binding in sortCopy)
			{
				GUILayout.Label("HOLD " + binding.ComboString, maxwidthscreen);
			}
		}
		if(GUILayout.Button(closebtnname, console.style.MenuStyle))
		{
			HideBindings();
		}
	}
	
	Binding FindBinding(List<Binding> existingBindings, KeyCode[] keysCombo)
	{
		Binding binding;
		KeyCode[] keys;
		var len = keysCombo.Length;
		bool found;
		for(var i = existingBindings.Count - 1; i >= 0; i--)
		{
			binding = existingBindings[i];
			keys = binding.keysCombo;
			if(keys.Length == len)
			{
				found = true;
				for(var j = keys.Length - 1; j >= 0; j--)
				{
					if(keys[j] != keysCombo[j])
					{
						found = false;
						break;
					}
				}
				if(found)
				{
					return binding;
				}
			}
		}
		return null;
	}

	Binding FindOrCreateBinding(List<Binding> existingBindings, KeyCode[] keysCombo)
	{
		var binding = FindBinding(existingBindings, keysCombo);
		if(binding == null)
		{
			binding = new Binding(keysCombo);
			existingBindings.Add(binding);

			if(keysCombo.Length > 1)
			{
				existingBindings.Sort((a, b) => a.keysCombo.Length - b.keysCombo.Length);
			}
		}
		return binding;
	}

	void Unbind(List<Binding> existingBindings, Action callback)
	{
		Binding binding;
		for(var i = existingBindings.Count - 1; i >= 0; i--)
		{
			binding = existingBindings[i];
			if(binding.callback == callback)
			{
				existingBindings.RemoveAt(i);
			}
		}
	}

	public static void BindDown(KeyCode[] keysCombo, Action callback, string description = null)
	{
		var instance = Instance;
		if(instance)
		{
			instance.FindOrCreateBinding(instance.downBindings, keysCombo).Set(callback, description);
		}
	}

	/*
	public static void BindUp(KeyCode[] keysCombo, Action callback, string description = null)
	{
		var instance = Instance;
		if(instance)
		{
			instance.FindOrCreateBinding(instance.upBindings, keysCombo).Set(callback, description);
		}
	}
	*/

	public static void BindHold(KeyCode[] keysCombo, Action callback, string description = null)
	{
		var instance = Instance;
		if(instance)
		{
			instance.FindOrCreateBinding(instance.holdBindings, keysCombo).Set(callback, description);
		}
	}

	public static void UnbindDown(Action callback)
	{
		var instance = Instance;
		if(instance)
		{
			instance.Unbind(instance.downBindings, callback);
		}
	}

	/*
	public static void UnbindUp(Action callback)
	{
		var instance = Instance;
		if(instance)
		{
			instance.Unbind(instance.upBindings, callback);
		}
	}
	*/
	
	public static void UnbindHold(Action callback)
	{
		var instance = Instance;
		if(instance)
		{
			instance.Unbind(instance.holdBindings, callback);
		}
	}
	
	public static void Unbind(Action callback)
	{
		var instance = Instance;
		if(instance)
		{
			instance.Unbind(instance.downBindings, callback);
			//instance.Unbind(instance.upBindings, callback);
			instance.Unbind(instance.holdBindings, callback);
		}
	}

	static JBCKeyBinding _instance;
	static JBCKeyBinding Instance
	{
		get
		{
			if(!_instance && JBConsole.Exists)	
			{
				_instance = JBConsole.instance.RegisterPlugin<JBCKeyBinding>();
			}
			return _instance;
		}
	}

	class Binding
	{
		public KeyCode[] keysCombo;
		public Action callback;
		public string desc;
		public Binding(KeyCode[] keysCombo)
		{
			this.keysCombo = keysCombo;
		}

		public void Set(Action callback, string desc)
		{
			this.callback = callback;
			this.desc = desc;
		}

		public void Call()
		{
			if(callback != null)
			{
				callback();
			}
		}

		string comboString;
		public string ComboString
		{
			get
			{
				if(comboString == null)
				{
					comboString = "";
					var l = keysCombo.Length - 1;
					for(var i = 0; i <= l; i++)
					{
						comboString += keysCombo[i];
						if(i < l) comboString += "+";
					}
					if(!string.IsNullOrEmpty(desc))
					{
						comboString += " = " + desc;
					}
				}
				return comboString;
			}
		}
	}
}