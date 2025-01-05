namespace RaB.Connection
{
    internal abstract class OnlineState : ConnectionState
    {
        public override void OnTransportFailure()
        {   
            ConnectionManager.Instance.ChangeState(ConnectionManager.Instance.Offline);
        }

        public override void OnUserRequestedShutdown()
        {
            ConnectionManager.Instance.ChangeState(ConnectionManager.Instance.Offline);
        }
    }
}