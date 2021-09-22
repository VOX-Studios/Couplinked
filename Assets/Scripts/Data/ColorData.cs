using System;
using UnityEngine;

public class ColorData
{
	private readonly string _playerPreferenceKey;

	public ColorData(string playerPreferenceKey)
    {
		_playerPreferenceKey = playerPreferenceKey;
	}

    public Color Get()
    {
		Color color = Color.white;
		if (PlayerPrefs.GetString(_playerPreferenceKey).Length > 0)
		{
			string[] colorSplit = PlayerPrefs.GetString(_playerPreferenceKey).Split(',');

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
		PlayerPrefs.SetString(_playerPreferenceKey, $"{value.r},{value.g} ,{value.b}");
		PlayerPrefs.Save();
	}
}