using UnityEngine;

public class Challenge 
{
	public static string pp_CompletedStatusAddOn = "Completed Status";
	public string Id;
	public string DisplayText;
	public string Description;

	public Challenge(
		string id, 
		string displayText, 
		string description
		)
	{
		Id = id;
		DisplayText = displayText;
		Description = description;
	}
	
	public bool LoadCompletedStatus() 
	{
		return PlayerPrefs.GetString(Id + pp_CompletedStatusAddOn) == "COMPLETED";
	}

	public static bool GetCompletedStatus(string id) 
	{
		return PlayerPrefs.GetString(id + pp_CompletedStatusAddOn) == "COMPLETED";
	}

	public static void SetCompletedStatus(string id) 
	{
		PlayerPrefs.SetString(id + pp_CompletedStatusAddOn, "COMPLETED");
		PlayerPrefs.Save();
	}

}
