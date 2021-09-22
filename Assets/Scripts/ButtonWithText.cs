using UnityEngine;
using UnityEngine.UI;

public class ButtonWithText : Button
{
    public Text text;

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        Color color;
        switch (state)
        {
            case SelectionState.Normal:
                color = colors.normalColor;
                break;
            case SelectionState.Highlighted:
                color = colors.highlightedColor;
                break;
            case SelectionState.Pressed:
                color = colors.pressedColor;
                break;
            case SelectionState.Disabled:
                color = colors.disabledColor;
                break;
            case SelectionState.Selected:
                color = colors.selectedColor;
                break;
            default:
                color = Color.black;
                break;
        }
        if (gameObject.activeInHierarchy)
        {
            switch (transition)
            {
                case Transition.ColorTint:
                    ColorTween(color * colors.colorMultiplier, instant);
                    break;
            }
        }
    }

    private void ColorTween(Color targetColor, bool instant)
    {
        if (this.targetGraphic != null)
        {
            image.CrossFadeColor(targetColor, (!instant) ? colors.fadeDuration : 0f, true, true);
        }

        if (text != null)
        {
            text.CrossFadeColor(targetColor, (!instant) ? this.colors.fadeDuration : 0f, true, true);
        }
    }
}