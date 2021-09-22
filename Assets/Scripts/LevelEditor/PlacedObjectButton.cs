using UnityEngine;
using UnityEngine.UI;

public class PlacedObjectButton : MonoBehaviour 
{
	public Image Image;
	public Image BackingImage;
	public ButtonWithBackingImage Button;

	public void Deactivate()
	{
		Button.onClick.RemoveAllListeners();
	}
}
