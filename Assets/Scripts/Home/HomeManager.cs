using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;


public class HomeManager : SingletonBehavior<HomeManager>
{
    private UserGameData _userGameData;
    public UserGameData UserGameData => _userGameData;

    public HomeUIController HomeUIController { get; private set; }

    private void Start()
    {
        // TODO : GameManager로 위임한다.
        _userGameData = UserDataManager.Instance.GetUserData<UserGameData>();
    }

    private void OnEnable()
    {
        // 로비가 생성되면 해당 델리게이트를 호출한다.
        SteamMatchmaking.OnLobbyCreated += LobbyCreated;
        SteamMatchmaking.OnLobbyEntered += LobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested += GameLobbyJoinRequested;
    }

    private void OnDisable()
    {
        SteamMatchmaking.OnLobbyCreated -= LobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= LobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested -= GameLobbyJoinRequested;
    }

    /// <summary>
    /// Lobby를 생성하는 메소드
    /// </summary>
    public async void CreateLobby()
    {
        await SteamMatchmaking.CreateLobbyAsync(2);
    }

    protected override void Init()
    {
        _isDestroyOnLoad = true;
        base.Init();
    }

    private void LobbyCreated(Result result, Lobby lobby)
    {
        if (result == Result.OK)
        {
            lobby.SetPublic();
            lobby.SetJoinable(true);

            NetworkManager.Singleton.StartHost();
        }
    }

    private void LobbyEntered(Lobby lobby)
    {
        // TODO : 다른 객체가 현재 Lobby를 들고 있을 것!
        // GameManager.Instance.CurrentLobby = lobby;
        // LobbyID.text = lobby.Id.ToString();
        Logger.Log("We entered Lobby");

        // Loading UI를 띄운다.
        BaseUIData baseUIData = new BaseUIData();
        UIManager.Instance.OpenUI<LoadingUI>(baseUIData);

        // Host일 때는 LoadScene 메소드를 통해 Lobby Scene로 들어갈 수 있게 한다.
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(SceneType.Lobby.ToString(), UnityEngine.SceneManagement.LoadSceneMode.Single);
            return;

        }

        // Client일 때는 Host의 Scene으로 자동이동하므로 Loading UI 상태에서 대기한다.
        NetworkManager.Singleton.gameObject.GetComponent<FacepunchTransport>().targetSteamId = lobby.Owner.Id;
        NetworkManager.Singleton.StartClient();
    }

    private async void GameLobbyJoinRequested(Lobby lobby, SteamId steamId)
    {
        await lobby.Join();
    }
}

