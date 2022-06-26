using System;
using UnityEngine;

public class ColorData
{
	public readonly string PlayerPreferenceKey;

	public ColorData(string playerPreferenceKey)
    {
		PlayerPreferenceKey = playerPreferenceKey;
	}

    public Color Get()
    {
		Color color = Color.white;
		if (PlayerPrefs.GetString(PlayerPreferenceKey).Length > 0)
		{
			string[] colorSplit = PlayerPrefs.GetString(PlayerPreferenceKey).Split(',');

			color = new Color(
				r: Convert.ToSingle(colorSplit[0]),
				g: Convert.ToSingle(colorSplit[1]),
				b: Convert.ToSingle(colorSplit[2])
				);
		}

		return color;
	}

	public void Set(Color value)
	{
		PlayerPrefs.SetString(PlayerPreferenceKey, $"{value.r},{value.g} ,{value.b}");
		PlayerPrefs.Save();
	}
}