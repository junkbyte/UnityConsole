using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;


public class JBCustomMenu
{
	MenuItem root = new MenuItem("root");

	MenuItem currentMenu;
	string[] currentLinks;

	public JBCustomMenu ()
	{
		currentMenu = root;
		root.Children = new List<MenuItem>();
	}
	
	public void Add(string name, Action callback)
	{
		if(string.IsNullOrEmpty(name)) return;
		root.AddPath(GetPath(name), 0, callback);
		currentLinks = null;
	}
	
	public void Remove(string name)
	{
		if(string.IsNullOrEmpty(name)) return;
		root.RemovePath(GetPath(name), 0);
		currentLinks = null;
	}

	public void PopToRoot()
	{
		currentLinks = null;
		currentMenu = root;
	}

	public string[] GetCurrentMenuLink()
	{
		if(currentLinks == null)
		{
			currentLinks = currentMenu.GetLinks();
		}
		return currentLinks;
	}
	
	public void OnCurrentLinkClicked(int index)
	{
		var newMenu = currentMenu.OnChildClicked(index);
		if(newMenu != null)
		{
			currentMenu = newMenu;
			currentLinks = null;
		}
		else
		{
			PopToRoot();
		}
	}

	string[] GetPath(string name)
	{
		return Regex.Split(name, @"\s*\/\s*");
	}







	class MenuItem
	{

		public string Name;
		public Action Callback;
		public List<MenuItem> Children;

		MenuItem parent;

		public MenuItem(string name)
		{
			Name = name;
		}
		
		public void AddPath(string[] path, int pathIndex, Action callback)
		{
			var part = path[pathIndex];
			var child = FindChild(part);
			if(child == null)
			{
				child = new MenuItem(part);
				AddChild(child);
			}

			if(PathIndexHasChild(path, pathIndex))
			{
				child.AddPath(path, pathIndex + 1, callback);
			}
			else
			{
				child.SetCallback(callback);
			}
		}
		
		public void RemovePath(string[] path, int pathIndex)
		{
			var part = path[pathIndex];
			var child = FindChild(part);
			if(child != null)
			{
				if(PathIndexHasChild(path, pathIndex))
				{
					child.RemovePath(path, pathIndex + 1);
					if(child.Children == null || child.Children.Count == 0)
					{
						Children.Remove(child);
					}
				}
				else
				{
					Children.Remove(child);
				}
			}
		}

		bool PathIndexHasChild(string[] path, int pathIndex)
		{
			return path.Length > pathIndex + 1;
		}
		
		MenuItem FindChild(string n)
		{
			return Children != null ? Children.Find(m => m.Name == n) : null;
		}
		
		void SetCallback(Action callback)
		{
			Callback = callback;
			Children = null;
		}

		void AddChild(MenuItem child)
		{
			Callback = null;
			if(Children == null) Children = new List<MenuItem>();
			Children.Add(child);
			child.parent = this;
		}
		
		public string GetLinkName()
		{
			if(Children != null && Children.Count > 0)
			{
				return Name + " /";
			}
			return Name;
		}

		public string[] GetLinks()
		{
			var links = new List<string>();
			if(parent != null) links.Add ("<");
			foreach(var item in Children)
			{
				links.Add(item.GetLinkName());
			}
			return links.ToArray();
		}
		
		public MenuItem OnChildClicked(int index)
		{
			if(parent != null) index--;
			if(index < 0 || index >= Children.Count) return parent;

		    try
		    {
                var child = Children[index];
                if (child.Callback != null)
                {
                    child.Callback();
                    return this;
                }
                else
                {
                    return child;
                }
		    }
		    catch (Exception)
            {
		        return parent;
		    }
			
		}
	}
}