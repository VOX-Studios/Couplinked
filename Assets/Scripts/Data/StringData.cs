using UnityEngine;

public class StringData
{
	private readonly string _playerPreferenceKey;

	public StringData(string playerPreferenceKey)
    {
		_playerPreferenceKey = playerPreferenceKey;
	}

	public string Get()
	{
		return PlayerPrefs.GetString(_playerPreferenceKey);
	}

	public void Set(string value)
	{
		PlayerPrefs.SetString(_playerPreferenceKey, value);
		PlayerPrefs.Save();
	}
}