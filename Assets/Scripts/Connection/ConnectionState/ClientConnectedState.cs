namespace RaB.Connection
{
    internal class ClientConnectedState : OnlineState
    {
        public override void Enter()
        {
            UIManager.Instance.CloseAllOpenUI();
        }

        public override void Exit()
        {
            
        }

        public override void OnClientDisconnect(ulong _)
        {
            ConnectionManager.Instance.ChangeState(ConnectionManager.Instance.Offline);
        }
    }
}