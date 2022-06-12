using System;
using System.Collections.Generic;

[Serializable]
public class LevelData
{
	public string Id;
	public string Name;
	public byte NumberOfRows;
	public List<ObjectData> Data;
	public int MaxScore;	
}
