using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class RegularReplace
{
	private static List<string> EventNames = new List<string>();
	private static void GetEventNames()
	{
		EventNames.Clear();
		List<string> allLuaPath = GetFilePaths(Application.dataPath);
		for (int index = 0; index < allLuaPath.Count; ++index)
		{			
			string absPath = allLuaPath[index];
			GetAndReplaceLuaEventName(absPath);
		}
	}

	private static void AddEventName(string eventName)
	{
		if (!EventNames.Contains(eventName))
		{
			EventNames.Add(eventName);
		}
	}

	private static List<string> SingleEventNames = new List<string>();
	private static Dictionary<string, List<string>> MultiEventNames = new Dictionary<string, List<string>>();
	private static void OrganizeEventNames()
	{
		SingleEventNames.Clear();
		MultiEventNames.Clear();
		for (int index = 0; index < EventNames.Count; ++index)
		{
			string eventName = EventNames[index];
			if (!eventName.Contains("."))
			{
				SingleEventNames.Add(eventName);
			}
			else
			{
				string[] splits = eventName.Split('.');
				if (splits.Length >= 2)
				{
					string table = splits[0];
					string name = eventName.Replace(table + ".", "");
					if (!MultiEventNames.ContainsKey(table))
						MultiEventNames[table] = new List<string>();
					MultiEventNames[table].Add(name);
				}
				else
				{
					Debug.LogError("splits length is illegal, eventName = " + eventName);
				}
			}
		}
	}	

	private static string GetCallEventName(string eventName)
	{
		return "EventName." + eventName;
	}

	private static string[] Keys = { "EventManager.AddEvent",
								   "EventManager.RemoveEvent",
								   "EventManager.DispatchEvent"};
	private static void GetAndReplaceLuaEventName(string absPath)
	{
		bool isChanged = false;
		List<string> str_list = new List<string>();
		StreamReader sr = new StreamReader(absPath);
		while (true)
		{
			string lineResult = sr.ReadLine();		
			if (lineResult == null)
				break;

			string line = lineResult;
			for (int index = 0; index < Keys.Length; ++index)
			{
				string key = Keys[index];
				if (line.Contains(key))
				{
					int keyIndex = line.IndexOf(key);
					if (keyIndex > 0)
						line = line.Remove(0, keyIndex);

					line = line.Replace(key, "");
					line = line.Trim();

					string[] splits = line.Split(',');
					if (splits.Length > 0)
					{
						string eventName = splits[0].Replace("(", "");
						eventName = eventName.Trim();
						if (eventName.Contains("\""))
						{
							eventName = eventName.Replace("\"", "");
							AddEventName(eventName);
							lineResult = lineResult.Replace("\"" + eventName + "\"", GetCallEventName(eventName));
							isChanged = true;
						}
					}
					break;
				}
			}

			str_list.Add(lineResult);
		}

		sr.Dispose();
		sr.Close();
		if (isChanged)
			WriteAllLinesBetter(absPath, str_list.ToArray());
	}

	[UnityEditor.MenuItem("Assets/==>RegularReplace")]
	private static void DoRegularReplace()
	{
		GetEventNames();
		
		string absPath = Application.dataPath + @"\StreamingAssets\Script\lua\Script\EventName.lua";
		StreamReader sr = new StreamReader(absPath);
		List<string> str_list = new List<string>();

		while (true)
		{
			string line = sr.ReadLine();
			if (line == null)
				break;

			str_list.Add(line);
		}

		str_list.Add("");
		str_list.Add("");

		OrganizeEventNames();

		//MultiEventNames
		foreach (var pair in MultiEventNames)
		{
			string table = pair.Key;
			List<string> listName = pair.Value;
			str_list.Add(string.Format("{0} =", table));
			str_list.Add("{");

			for (int index = 0; index < listName.Count; ++index)
			{
				string name = listName[index];
				string eventName = "\"" + table + "." + name + "\"";
				str_list.Add("\t" + name + " = " + eventName + ",");
			}

			str_list.Add("};");
		}

		//SingleEventNames
		for (int index = 0; index < SingleEventNames.Count; ++index)
		{
			string eventName = SingleEventNames[index];
			string result = eventName + " = " + "\"" + eventName + "\"" + ";";
			str_list.Add(result);
		}

		sr.Dispose();
		sr.Close();
		//File.WriteAllLines(absPath, str_list.ToArray(), new UTF8Encoding(false));//no bom
		WriteAllLinesBetter(absPath, str_list.ToArray());
	}

	#region Tools
	public static void WriteAllLinesBetter(string path, params string[] lines)
	{
		if (path == null)
			throw new ArgumentNullException("path");
		if (lines == null)
			throw new ArgumentNullException("lines");

		using (var stream = File.OpenWrite(path))
		using (StreamWriter writer = new StreamWriter(stream))
		{
			if (lines.Length > 0)
			{
				for (int i = 0; i < lines.Length - 1; i++)
				{
					writer.WriteLine(lines[i]);
				}
				writer.Write(lines[lines.Length - 1]);
			}
		}
	}
	#endregion

	#region Lua
	private static List<string> GetFilePaths(string path)
	{
		return GetFilePaths(new DirectoryInfo(path));
	}
	private static List<string> GetFilePaths(DirectoryInfo folder)
	{
		List<string> pathList = new List<string>();
		DirectoryInfo[] dirInfo = folder.GetDirectories();
		if (dirInfo.Length <= 0) return pathList;

		for (int i = 0; i < dirInfo.Length; i++)
		{
			DirectoryInfo folderInfo = dirInfo[i];
			pathList.AddRange(GetFilePaths(folderInfo));
			FileInfo[] fileInfo = folderInfo.GetFiles("*.lua");
			for (int j = 0; j < fileInfo.Length; j++)
			{
				FileInfo fileItem = fileInfo[j];
				pathList.Add(fileItem.DirectoryName + "\\" + fileItem.Name);
			}
		}
		return pathList;
	}
	#endregion
}