using System;
using System.Threading.Tasks;
using Android.App;
using Android.Gms.Auth.Api;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.OS;
using Android.Support.V7.App;

namespace xbridge.android.Modules
{

    internal class ResultCallback : Java.Lang.Object, IResultCallback
    {
        public Action<Java.Lang.Object> Action;

        public void OnResult(Java.Lang.Object result)
        {
            Action(result);
        }

    }
    internal class ConnectionCallbacks : Java.Lang.Object, GoogleApiClient.IConnectionCallbacks
    {
        public Action<Bundle> Action;


        public void OnConnected(Bundle connectionHint)
        {
            Action(connectionHint);
        }

        public void OnConnectionSuspended(int cause)
        {

        }


    }
    internal class ConnectionFailedListener : Java.Lang.Object, GoogleApiClient.IOnConnectionFailedListener
    {
        public void OnConnectionFailed(ConnectionResult result)
        {
            throw new Exception("Connection failed: " + result.ToString());
        }
    }

    public class Account
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "email")]
        public string Email;
        [Newtonsoft.Json.JsonProperty(PropertyName = "id")]
        public string ID;
        [Newtonsoft.Json.JsonProperty(PropertyName = "displayName")]
        internal string DisplayName;
        [Newtonsoft.Json.JsonProperty(PropertyName = "token")]
        public string Token;
    }

    public class Auth
    {
        private XBridge Bridge;
        private AppCompatActivity Activity;
        private GoogleApiClient ApiClient;
        private string clientId = null;
        bool email = false;


        public Auth(XBridge xbridge)
        {
            this.Bridge = xbridge;
            this.Activity = (Bridge.Adapter as XBridgeAndroidAdapter).Activity;
            //this.ClientId = clientId;
        }

        public string ServerClientId(object v)
        {
            if(v is string)
                return clientId = (string)v;
            return clientId = null;
        }

        public bool Email(bool? v)
        {
            if (v == null)
                return email;
            return email = (bool)v;
        }


        public Task<object> Connect() {

            var tcs = new TaskCompletionSource<object>();
            var builder = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn);
            if (clientId != null)
                builder.RequestIdToken(clientId);
            if (email)
                builder.RequestEmail();
            GoogleSignInOptions gso = builder.Build();

            ApiClient = new GoogleApiClient.Builder(Activity).EnableAutoManage(Activity,  new ConnectionFailedListener()).AddApi(Android.Gms.Auth.Api.Auth.GOOGLE_SIGN_IN_API, gso).Build();
            ApiClient.RegisterConnectionCallbacks(new ConnectionCallbacks
            {
                Action = (bundle) =>
                {
                    if (tcs != null)
                    {
                        tcs.SetResult(true);
                    }
                    tcs = null;
                }
            });
            return tcs.Task;
        }

        public void Disconnect() {
            ApiClient.Disconnect();
            ApiClient.Dispose();
            ApiClient = null;
        }

        public async Task<object> Revoke()
        {
            await MaybeConnect();
            var result = await Android.Gms.Auth.Api.Auth.GoogleSignInApi.RevokeAccess(ApiClient);
            return result.Status.IsSuccess;
        }

        public async Task<object> SignedIn() {
            await MaybeConnect();
            var opr = Android.Gms.Auth.Api.Auth.GoogleSignInApi.SilentSignIn(ApiClient);
            if (opr.IsDone)
            {
                // If the user's cached credentials are valid, the OptionalPendingResult will be "done"
                // and the GoogleSignInResult will be available instantly.
                var result = opr.Get() as GoogleSignInResult;
                return ParseSignInResult(result);
            }
            else
            {

                var tcs = new TaskCompletionSource<object>();
                // If the user has not previously signed in on this device or the sign-in has expired,
                // this asynchronous branch will attempt to sign in the user silently.  Cross-device
                // single sign-on will occur in this branch.
                opr.SetResultCallback(new ResultCallback { Action = (result) => {
                    try
                    {
                        var gsir = result as GoogleSignInResult;
                        tcs.SetResult(ParseSignInResult(gsir));
                    }catch(Exception err)
                    {
                        tcs.SetException(err);
                    }
                } });
                return await tcs.Task;
            }
        }

        private async Task MaybeConnect()
        {
            if (ApiClient == null)
            {
                await Connect();
            }
        }

        private object ParseSignInResult(GoogleSignInResult result)
        {
            if (!result.IsSuccess)
            {
                if (result.Status.StatusCode == CommonStatusCodes.SignInRequired)
                    return null;
                throw new Exception(CommonStatusCodes.GetStatusCodeString(result.Status.StatusCode));
            }
            var a = result.SignInAccount;
            return new Account
            {
                ID = a.Id,
                Email = a.Email,
                DisplayName = a.DisplayName,
                Token = a.IdToken
            };
        }

        public async Task<object> SignOut()
        {
            await MaybeConnect();
            var result = await Android.Gms.Auth.Api.Auth.GoogleSignInApi.SignOut(ApiClient);
            return result.Status.IsSuccess;
        }

        public async Task<object> SignIn()
        {
            await MaybeConnect();
            var signInIntent = Android.Gms.Auth.Api.Auth.GoogleSignInApi.GetSignInIntent(ApiClient);
            var actResult = await Bridge.GetModule<Core>().GetActivityResult(signInIntent);
            var result = Android.Gms.Auth.Api.Auth.GoogleSignInApi.GetSignInResultFromIntent(actResult.Data);
            return ParseSignInResult(result);
        }
    }
}
