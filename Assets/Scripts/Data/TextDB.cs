using UnityEngine;
using System.Collections;
using System.IO;
using LitJson;

public class TextDB
{
	public static string[] CardDlg_Error = new string[2];
	public static string[] CardDlg_strKeyin = new string[2];
	public static string[] CardDlg_SysPassword = new string[2];

	public static void LoadFile()
	{
		using(TextReader streamReader = 
		      new StreamReader(Application.streamingAssetsPath + "/Lanuages.txt"))
		{
			ReadText(streamReader);
		}

	}

	static void ReadText(TextReader textReader)
	{
		JsonData data = JsonMapper.ToObject(textReader.ReadToEnd());
		CardDlg_SysPassword[0] = data["CardDlg"]["strSysPassword"][0].ToString();
		CardDlg_SysPassword[1] = data["CardDlg"]["strSysPassword"][1].ToString();
	}

}
