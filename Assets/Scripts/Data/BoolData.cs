using UnityEngine;

public class BoolData
{
	private readonly string _playerPreferenceKey;

	public BoolData(string playerPreferenceKey)
    {
		_playerPreferenceKey = playerPreferenceKey;
	}

	public bool Get()
	{
		int pref = PlayerPrefs.GetInt(_playerPreferenceKey);
		return pref == 1;
	}

	public void Set(bool value)
	{
		PlayerPrefs.SetInt(_playerPreferenceKey, value ? 1 : 0);
		PlayerPrefs.Save();
	}
}