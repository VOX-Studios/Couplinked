using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class Level
{
	public LevelData LevelData;

	public List<ObjectData> Data => LevelData.Data;
	public int MaxScore => LevelData.MaxScore;
	public byte NumberOfRows => LevelData.NumberOfRows;

	public Level(string id)
	{
		Load(id);
	}

	public Level()
	{
		LevelData = new LevelData();
		LevelData.Data = new List<ObjectData>();
		LevelData.Name = "";
		LevelData.Id = "";
		LevelData.NumberOfRows = 3;
	}

	public int RateScore(int score)
	{
		if(MaxScore == 0)
            return 5;

		float percentage = (float)score / MaxScore;

		if(percentage >= .6f)
			return 5;
		else if(percentage >= .5f)
			return 4;
		else if(percentage >= .4f)
			return 3;
		else if(percentage >= .25f)
			return 2;
		else if(percentage > 0)
			return 1;

        return 0;
	}

	public static int RateEfficiencyScore(int score, int potentialScore)
	{
		if(score == 0 || potentialScore == 0)
            return 0;

		float percentage = (float)score / potentialScore;
		
		if(percentage >= .6f)
			return 5;
		else if(percentage >= .5f)
			return 4;
		else if(percentage >= .4f)
			return 3;
		else if(percentage >= .25f)
			return 2;

        return 1;
	}
	
	public void Load(string id)
	{
		//Get the data
		string strLoadData = PlayerPrefs.GetString(id);

		CopyFromLoad(ref strLoadData);
	}

	/// <summary>
	/// For campaign levels.
	/// </summary>
	/// <param name="filePath">File path.</param>
	public void LoadFromTextFile(string filePath)
	{
		//Get the data
		string strLoadData = Resources.Load<TextAsset>(filePath).text;

		CopyFromLoad(ref strLoadData);
		LevelData.Id = LevelData.Name;

		//TODO: make this configurable
		LevelData.NumberOfRows = 3;
	}

	private void CopyFromLoad(ref string strLoadData)
	{
		if(!string.IsNullOrEmpty(strLoadData))
		{
			LevelData = JsonUtility.FromJson<LevelData>(strLoadData);
		}
	}


	public void ExportCampaignLevel()
	{
		#if UNITY_EDITOR && !UNITY_WEBPLAYER
		
		LevelData.MaxScore = 0;

		for (int i = 0; i < Data.Count; i++)
		{
			switch (Data[i].ObjectType)
			{
				case ObjectTypeEnum.Hit1:
				case ObjectTypeEnum.Hit2:
					LevelData.MaxScore += 10;
					break;
				case ObjectTypeEnum.HitSplit1:
				case ObjectTypeEnum.HitSplit2:
					LevelData.MaxScore += 20;
					break;
			}
		}

		string levelDataJson = JsonUtility.ToJson(LevelData);

		File.WriteAllText($@"C:\Couplinked\Couplinked\Assets\Resources\CampaignLevelData\{LevelData.Name}.txt", levelDataJson);
		#endif
	}

	public int CalculateMaxScore()
	{
		LevelData.MaxScore = 0;
		
		for(int i = 0; i < Data.Count; i++)
		{
			
			switch(Data[i].ObjectType)
			{
				case ObjectTypeEnum.Hit1:
				case ObjectTypeEnum.Hit2:
					LevelData.MaxScore += 10;
					break;
				case ObjectTypeEnum.HitSplit1:
				case ObjectTypeEnum.HitSplit2:
					LevelData.MaxScore += 20;
					break;
			}
		}

		return MaxScore;
	}

	public int CalculatePotentialRingsCollected()
	{
		int potentialRingsCollected = 0;
		
		for(int i = 0; i < Data.Count; i++)
		{
			
			switch(Data[i].ObjectType)
			{
				case ObjectTypeEnum.Hit1:
				case ObjectTypeEnum.Hit2:
					potentialRingsCollected++;
					break;
				case ObjectTypeEnum.HitSplit1:
				case ObjectTypeEnum.HitSplit2:
					potentialRingsCollected += 2;
					break;
			}
		}
		
		return potentialRingsCollected;
	}

	public void Save(DataManager dataManager)
	{
		//Set the name for better memory management when looking up levels
		dataManager.SetCustomLevelName(LevelData.Id, LevelData.Name);

		LevelData levelData = new LevelData();

		levelData.Id = LevelData.Id;
		levelData.Name = LevelData.Name;
		levelData.Data = new List<ObjectData>();
		levelData.NumberOfRows = LevelData.NumberOfRows;

		for(int i = 0; i < Data.Count; i++)
		{
			var objectData = new ObjectData()
			{
				Time = Data[i].Time,
				ObjectRow = Data[i].ObjectRow,
				ObjectType = Data[i].ObjectType
			};

			levelData.Data.Add(objectData);
			
			switch(Data[i].ObjectType)
			{
				case ObjectTypeEnum.Hit1:
				case ObjectTypeEnum.Hit2:
					levelData.MaxScore += 10;
					break;
				case ObjectTypeEnum.HitSplit1:
				case ObjectTypeEnum.HitSplit2:
					levelData.MaxScore += 20;
					break;
			}
		}

		string levelDataJson = JsonUtility.ToJson(levelData);

		//show me the data
		Debug.Log(levelDataJson);

		//Add it to player prefs
		dataManager.SetCustomLevelData(LevelData.Id, levelDataJson);

        //if the level ID didn't exist before, add it on
        List<string> levelIds = new List<string>(dataManager.GetCustomLevelIds());
		if(!levelIds.Contains(LevelData.Id))
		{
			levelIds.Add(LevelData.Id);
			dataManager.SetCustomLevelIds(levelIds);
		}
	}
}
