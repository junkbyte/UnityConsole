using UnityEngine;
using System.Collections;
using System;

public class JBConsoleUtils
{
    public static void AddToStringArray(ref string[] strings, string str)
    {
        Array.Resize(ref strings, strings.Length + 1);
        strings[strings.Length - 1] = str;
    }

    public static string[] StringsWithoutString(string[] strings, string str)
    {
        int index = Array.IndexOf(strings, str);
        return StringsWithoutIndex(strings, index);
    }

    public static string[] StringsWithoutIndex(string[] strings, int index)
    {
        string[] result;
        if (index >= 0)
        {
            result = new string[strings.Length - 1];
            Array.Copy(strings, 0, result, 0, index);
            Array.Copy(strings, index + 1, result, index, strings.Length - index - 1);
        }
        else
        {
            result = new string[strings.Length];
            Array.Copy(strings, result, strings.Length);
        }
        return result;
    }
}
