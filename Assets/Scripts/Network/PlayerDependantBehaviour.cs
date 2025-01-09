using Unity.Netcode;

/// <summary>
/// 로컬 플레이어의 생성을 전제로 하는 Class
/// </summary>
public class PlayerDependantBehaviour : NetworkBehaviour
{
    override public void OnNetworkSpawn()
    {
        if (PlayerController.LocalPlayer)
        {
            OnPlayerInitialized();
        }
        else
        {
            PlayerController.LocalPlayerCreated += OnPlayerInitialized;
        }
    }

    public override void OnNetworkDespawn()
    {
        PlayerController.LocalPlayerCreated -= OnPlayerInitialized;
    }

    virtual public void OnPlayerInitialized()
    {

    }
}
