using Greatbone.Core;

namespace Greatbone.Sample
{
    public abstract class AbstService : WebService<Token>
    {
        internal readonly Admin[] admins;

        public AbstService(WebServiceContext sc) : base(sc)
        {
            admins = JsonUtility.FileToArray<Admin>(sc.GetFilePath("$admins.json"));
        }

        protected override void Challenge(WebActionContext ac)
        {
            string ua = ac.Header("User-Agent");
            if (ua.Contains("MicroMessenger")) // weixin
            {
                // redirect the user to weixin authorization page
                ac.ReplyRedirect("https://open.weixin.qq.com/connect/oauth2/authorize?appid=APPID&redirect_uri=REDIRECT_URI&response_type=code&scope=SCOPE&state=STATE#wechat_redirect");
            }
            else if (ua.Contains("Mozila")) // browser
            {
                string loc = "singon" + "?orig=" + ac.Uri;
                ac.SetHeader("Location", loc);
                ac.Reply(303); // see other - redirect to signon url
            }
            else // non-browser
            {
                ac.SetHeader("WWW-Authenticate", "Bearer");
                ac.Reply(401); // unauthorized
            }
        }
    }
}