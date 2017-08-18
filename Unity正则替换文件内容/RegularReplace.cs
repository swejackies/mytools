using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class RegularReplace 
{
	[UnityEditor.MenuItem("Assets/==>RegularReplace/protolist")]
	private static void RegularReplace_protolist()
	{
		string absPath = @"E:\p3_client\client\Assets\StreamingAssets\Script\lua\protolist.lua";
		StreamReader sr = new StreamReader(absPath);
		bool isRead = true;
		List<string> str_list = new List<string>();

		while (isRead)
		{
			string line = sr.ReadLine();
			if(line == null)
				isRead = false;

			if (!isRead)
				break;

			//Regex r = new Regex("message[ ]*=[ ]*\"[a-z 0-9 _]*.[a-z 0-9 _]*\"", RegexOptions.IgnoreCase);
			//Regex r = new Regex("file[ ]*=[ ]*\"[a-z 0-9 _]*.[a-z 0-9 _]*\"[ ]*,", RegexOptions.IgnoreCase);
			Regex r = new Regex("message[ ]*=[ ]*\"[a-z 0-9 _]*.[a-z 0-9 _]*\"[ ]*,", RegexOptions.IgnoreCase);

			Match m = r.Match(line);
			while (m.Success)
			{
				//line = AddFunc(line, m);
				line = RemoveString(line, m);

				m = m.NextMatch();
			}

			str_list.Add(line);
		}

		sr.Dispose();
		sr.Close();
		File.WriteAllLines(absPath, str_list.ToArray(), System.Text.Encoding.UTF8);
	}

	private static string AddFunc(string line, Match m)
	{
		if (m.Groups.Count <= 0)
			return line;

		Group value = m.Groups[0];
		string[] splits = value.Value.Split('.');

		string pbName = splits[0].Replace("\"", "");
		pbName = pbName.Substring(pbName.IndexOf("=") + 1).Trim();
		pbName = pbName + "_pb";

		string funcName = splits[1].Replace("\"", "");

		string result = "func = " + pbName + "." + funcName;
		return line.Replace("}", ", " + result + "}");
	}

	private static string RemoveString(string line, Match m)
	{
		if (m.Groups.Count <= 0)
			return line;

		Group value = m.Groups[0];
		return line.Replace(value.Value, "");
		return "";
	}
}