using Unity.Netcode;

/// <summary>
/// 로컬 플레이어의 생성을 전제로 하는 Class
/// </summary>
public class LocalDependantBehaviour : NetworkBehaviour
{
    override public void OnNetworkSpawn()
    {
        if (true)   // 로컬 플레이어가 이미 존재하는지 판단하는 로직으로 교체
        {
            OnLocalInitialized();
        }
        else
        {
            // 로컬 플레이어가 생성되었을 때 호출되는 delegate에 OnLocalInitialized() 추가
        }
    }

    virtual public void OnLocalInitialized()
    {

    }
}
