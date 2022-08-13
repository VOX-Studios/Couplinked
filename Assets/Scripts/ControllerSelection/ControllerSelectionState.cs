using Assets.Scripts.ControllerSelection;

class ControllerSelectionState
{
    public bool IsReady { get; private set; }

    public bool WasJustAdded = false;

    public int TeamSlot = -1;

    public NodePairing NodePairing;

    public PlayerText PlayerText;

    public void SetIsReady(bool isReady)
    {
        IsReady = isReady;

        if(IsReady)
        {
            PlayerText.ReadyText.text = "READY";
        }
        else
        {
            PlayerText.ReadyText.text = "...";
        }

        PlayerText.ReadyText.GetComponent<TextPulse>().ShouldPulse(!IsReady);
    }
}
