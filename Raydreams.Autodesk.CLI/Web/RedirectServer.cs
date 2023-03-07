using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using Raydreams.Autodesk.Model;
using Raydreams.Web;
using Raydreams.Autodesk.IO;

namespace Raydreams.Autodesk.CLI.Web
{
    /// <summary>In memory web server to handle callbacks</summary>
    public class MoreServer : MiniServer
    {
        public MoreServer(int port) : base(port, Path.Combine( IOHelpers.DesktopPath, "www") )
        {
            // add more custom routes here
            this.AddSpecialRoute("/json", ServeJSONExample);
            this.AddSpecialRoute("/token", ServeToken);
            this.AddSpecialRoute("/forge-callback", ServeToken);
        }

        /// <summary>Custom events for the Autodesk Forge Login</summary>
        public event Action<string?>? AuthRedirect;

        /// <summary>Handler for using an Autodesk Redirect</summary>
        protected virtual void OnAuthRedirect(string? code)
        {
            if (this.AuthRedirect != null)
                this.AuthRedirect(code);
        }

        /// <summary>Example on how to add custom routes</summary>
        /// <param name="ctx"></param>
        protected void ServeJSONExample(HttpListenerContext ctx)
        {
            var jsonObj = new { Timestamp = DateTimeOffset.UtcNow, Message = "My Special Object" };

            // add a event callback to your own code here

            // echo the Proxy JSON response to the browser window - not really necessary to work
            base.ServeJSON(ctx.Response, JsonConvert.SerializeObject(jsonObj));
        }

        /// <summary>Example callback from OAuth with an access token</summary>
        /// <param name="ctx"></param>
        protected void ServeToken(HttpListenerContext ctx)
        {
            // get the access token from the code param
            string? code = ctx.Request.QueryString.Get("code");

            if ( String.IsNullOrWhiteSpace(code) )
                code = "no code";

            // add a event callback to your own code here to set the access token in your app
            this.OnAuthRedirect(code);

            // send something to the browser window so they know it worked
            this.ServeSimpleHTML(ctx.Response, $"<p>Your Access Token Is</p><p>{code}</p>");
            //this.ServeSimpleHTML(ctx.Response, "<p>auth code retrieved &amp; written</p>");
        }
    }

}
