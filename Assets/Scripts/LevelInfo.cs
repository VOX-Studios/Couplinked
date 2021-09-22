using UnityEngine;
using UnityEngine.UI;

public class LevelInfo : MonoBehaviour
{
	public Sprite IncompleteTexture,
					OneStarTexture,
					TwoStarTexture,
					ThreeStarTexture,
					FourStarTexture,
					FiveStarTexture,
					LockedTexture;

	public bool IsLocked;
	public string name, Id;
	public LevelTypes levelType;
	public int MaxScore, PlayerScore, PlayerScoreRating;

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
