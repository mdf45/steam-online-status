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
        public Action OnFailure;

        public Account Account { get; private set; }

        public SteamMakeOnline(Account account)
        {
            Account = account;
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

            _steamClient = new SteamClient();
            _callbackManager = new CallbackManager(_steamClient);
            _steamUser = _steamClient.GetHandler<SteamUser>();
            _steamFriends = _steamClient.GetHandler<SteamFriends>();

            _callbackManager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            _callbackManager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);
            _callbackManager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            _callbackManager.Subscribe<SteamUser.AccountInfoCallback>(OnAccountInfo);
            
            SteamLogIn();
        }

        public void SteamLogIn()
        {
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
                Username = Account.Username,
                Password = Account.Password,
                TwoFactorCode = Account.GuardCode
            });
        }
        async void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            _isRunning = false;
            await Task.Delay(TimeSpan.FromSeconds(5));

            Failure();
        }
        void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                Console.WriteLine($"Unable to log in to Steam: {callback.Result}\n");
                _steamClient.Disconnect();
                return;
            }
            Console.WriteLine($"{Account.Username} successfully logged in!");
        }
        void OnAccountInfo(SteamUser.AccountInfoCallback callback)
        {
            _steamFriends.SetPersonaState(_personaState);
        }

        void Failure()
        {
            OnFailure?.Invoke();
        }
    }
}
