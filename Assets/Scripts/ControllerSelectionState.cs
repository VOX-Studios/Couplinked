class ControllerSelectionState
{
    public bool IsReady { get; private set; }

    public bool WasJustAdded = false;

    public int TeamSlot = -1;

    public NodePairing NodePairing;

    public void SetIsReady(bool isReady)
    {
        IsReady = isReady;
    }
}
