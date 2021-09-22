using UnityEngine;
using UnityEngine.UI;

public class AchievementInfo : MonoBehaviour 
{
    public Sprite IncompleteTexture;
    public Sprite CompleteTexture;

	public bool IsCompleted { get; private set; }
	public string ID;

    public Button Button;
    public Text TitleText;
    public Text DescriptionText;

    [SerializeField]
    private Image _completionImage;

	public void SetCompletedStatusAndTexture(bool completed)
	{
		IsCompleted = completed;

		if(IsCompleted)
            _completionImage.sprite = CompleteTexture;
		else
            _completionImage.sprite = IncompleteTexture;
	}
}
