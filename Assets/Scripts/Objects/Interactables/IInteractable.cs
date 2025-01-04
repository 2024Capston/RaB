public interface IInteractable
{
    public bool IsInteractable(PlayerController player);

    public void StartInteraction(PlayerController player);

    public void StopInteraction(PlayerController player);
}