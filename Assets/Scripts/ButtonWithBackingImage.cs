using UnityEngine;
using UnityEngine.UI;

public class ButtonWithBackingImage : Button
{
    public Image BackingImage;

    public ColorBlock BackingImageColors;
    
    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        Color buttonColor;
        Color backingImageColor;
        switch (state)
        {
            case SelectionState.Normal:
                buttonColor = colors.normalColor;
                backingImageColor = BackingImageColors.normalColor;
                break;
            case SelectionState.Highlighted:
                buttonColor = colors.highlightedColor;
                backingImageColor = BackingImageColors.highlightedColor;
                break;
            case SelectionState.Pressed:
                buttonColor = colors.pressedColor;
                backingImageColor = BackingImageColors.pressedColor;
                break;
            case SelectionState.Disabled:
                buttonColor = colors.disabledColor;
                backingImageColor = BackingImageColors.disabledColor;
                break;
            case SelectionState.Selected:
                buttonColor = colors.selectedColor;
                backingImageColor = BackingImageColors.selectedColor;
                break;
            default:
                buttonColor = Color.black;
                backingImageColor = Color.black;
                break;
        }

        if (gameObject.activeInHierarchy)
        {
            switch (transition)
            {
                case Transition.ColorTint:
                    ColorTween(buttonColor * colors.colorMultiplier, backingImageColor * BackingImageColors.colorMultiplier, instant);
                    break;
            }
        }
    }

    private void ColorTween(Color targetColor, Color targetBackingImageColor, bool instant)
    {
        if (this.targetGraphic != null)
        {
            image.CrossFadeColor(targetColor, (!instant) ? colors.fadeDuration : 0f, true, true);
        }

        if (BackingImage != null)
        {
            BackingImage.CrossFadeColor(targetBackingImageColor, (!instant) ? BackingImageColors.fadeDuration : 0f, true, true);
        }
    }
}