using SteamKit2;
using System;
using System.Threading.Tasks;

namespace SteamMakeOnline
{
    public class Account
    {
        public Account(string username, string password, string guardCode)
        {
            Username = username;
            Password = password;
            GuardCode = guardCode;
        }

        public string Username { get; }
        public string Password { get; }
        public string GuardCode { get; set; }
    }
    public class SteamMakeOnline
    {
        public event EventHandler<Account> OnHasDisconnected;

        private readonly Account _account;

        public SteamMakeOnline(Account account)
        {
            _account = account;
        }

        SteamClient _steamClient;
        CallbackManager _callbackManager;
        SteamUser _steamUser;
        SteamFriends _steamFriends;

        EPersonaState _personaState;

        bool _isRunning;

        public void Init(EPersonaState personaState)
        {
            _personaState = personaState;
            SteamLogIn();
        }

        void SteamLogIn()
        {
            _steamClient = new SteamClient();
            _callbackManager = new CallbackManager(_steamClient);
            _steamUser = _steamClient.GetHandler<SteamUser>();
            _steamFriends = _steamClient.GetHandler<SteamFriends>();

            _callbackManager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            _callbackManager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);
            _callbackManager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            _callbackManager.Subscribe<SteamUser.AccountInfoCallback>(OnAccountInfo);

            _isRunning = true;

            _steamClient.Connect();

            while (_isRunning)
            {
                _callbackManager.RunWaitCallbacks();
            }
        }

        void OnConnected(SteamClient.ConnectedCallback callback)
        {
            _steamUser.LogOn(new SteamUser.LogOnDetails
            {
                Username = _account.Username,
                Password = _account.Password,
                TwoFactorCode = _account.GuardCode
            });
        }
        void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            OnHasDisconnected?.Invoke(this, _account);
        }
        void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                Console.WriteLine($"Unable to log in to Steam: {callback.Result}\n");
                _isRunning = false;
                return;
            }
            Console.WriteLine($"{_account.Username} successfully logged in!");
        }
        void OnAccountInfo(SteamUser.AccountInfoCallback callback)
        {
            _steamFriends.SetPersonaState(_personaState);
        }
    }
}
