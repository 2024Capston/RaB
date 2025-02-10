namespace RaB.Connection
{
    internal class ClientConnectedState : OnlineState
    {
        public override void Enter() { }

        public override void Exit() { }

        public override void OnClientDisconnect()
        {
            ConnectionManager.Instance.ChangeState(ConnectionManager.Instance.Offline);
        }
    }
}