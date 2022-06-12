using UnityEngine;
using UnityEngine.UI;

public class LevelInfo : MonoBehaviour
{
	public Sprite IncompleteTexture;
	public Sprite OneStarTexture;
	public Sprite TwoStarTexture;
	public Sprite ThreeStarTexture;
	public Sprite FourStarTexture;
	public Sprite FiveStarTexture;
	public Sprite LockedTexture;

	public bool IsLocked;
	public string Name;
	public string Id;
	public LevelTypes levelType;
	public int MaxScore;
	public int PlayerScore;
	public int PlayerScoreRating;

	public enum LevelTypes : byte
	{
		Campaign,
		Custom
	}

	public void SetTextureBasedOnRating(int rating)
	{
		switch(rating)
		{
			case 0:
				GetComponent<Image>().sprite = IncompleteTexture;
				break;
			case 1:
				GetComponent<Image>().sprite = OneStarTexture;
				break;
			case 2:
				GetComponent<Image>().sprite = TwoStarTexture;
				break;
			case 3:
				GetComponent<Image>().sprite = ThreeStarTexture;
				break;
			case 4:
				GetComponent<Image>().sprite = FourStarTexture;
				break;
			case 5:
				GetComponent<Image>().sprite = FiveStarTexture;
				break;
		}
	}
}
