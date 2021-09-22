using System;
using UnityEngine;

public class EnumData<T> where T : struct, Enum
{
	private readonly string _playerPreferenceKey;

	public EnumData(string playerPreferenceKey)
    {
		_playerPreferenceKey = playerPreferenceKey;
	}

	public T Get()
	{
		return (T)(object)PlayerPrefs.GetInt(_playerPreferenceKey);
	}

	public void Set(T enumValue)
	{
		PlayerPrefs.SetInt(_playerPreferenceKey, (int)(object)enumValue);
		PlayerPrefs.Save();
	}
}