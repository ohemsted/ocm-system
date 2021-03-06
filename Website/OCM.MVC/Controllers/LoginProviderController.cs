﻿using Microsoft.Web.WebPages.OAuth;
using OCM.API.Common;
using OCM.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OCM.MVC.Controllers
{
    public class LoginProviderController : Controller
    {

        #region Cookie Helpers
        public static void UpdateCookie(HttpResponseBase response, string cookieName, string cookieValue)
        {
            if (response.Cookies.AllKeys.Contains(cookieName))
            {
                response.Cookies[cookieName].Value = cookieValue;
            }
            else
            {
                var cookie = new HttpCookie(cookieName, cookieValue);
                cookie.Expires = DateTime.Now.AddMonths(2);
                response.Cookies.Add(new HttpCookie(cookieName, cookieValue));
            }
        }

        public static string GetCookie(HttpRequestBase request, string cookieName)
        {
            if (request.Cookies.AllKeys.Contains(cookieName))
            {
                return request.Cookies[cookieName].Value;
            }
            else
            {
                return "";
            }
        }

        public static void ClearCookie(HttpResponseBase response, string cookieName, string cookieValue)
        {
            if (response.Cookies.AllKeys.Contains(cookieName))
            {
                response.Cookies[cookieName].Value = cookieValue;
                response.Cookies[cookieName].Expires = DateTime.UtcNow.AddDays(-1);
            }
        }
        #endregion

        #region Login Workflow Handlers
        public ActionResult BeginLogin()
        {
            ViewBag.LoginProviders = OAuthWebSecurity.RegisteredClientData;
            return View();
        }

        public ActionResult LoginFailed()
        {
            TempData["LoginFailed"] = true;
            return RedirectToAction("BeginLogin");
        }

        [AllowAnonymous]
        public void LoginWithProvider(string provider)
        {
            OAuthWebSecurity.RequestAuthentication(provider, Url.Action("AuthenticationCallback"));
        }

        [AllowAnonymous]
        public ActionResult AuthenticationCallback()
        {
            try
            {
                //http://brockallen.com/2012/09/04/using-oauthwebsecurity-without-simplemembership/
                var result = OAuthWebSecurity.VerifyAuthentication();
                if (result.IsSuccessful)
                {
                    // name of the provider we just used
                    var provider = result.Provider;
                    // provider's unique ID for the user
                    var uniqueUserID = result.ProviderUserId;
                    // since we might use multiple identity providers, then 
                    // our app uniquely identifies the user by combination of 
                    // provider name and provider user id
                    var uniqueID = provider + "/" + uniqueUserID;

                    // we then log the user into our application
                    // we could have done a database lookup for a 
                    // more user-friendly username for our app
                    //FormsAuthentication.SetAuthCookie(uniqueID, false);

                    // dictionary of values from identity provider
                    var userDataFromProvider = result.ExtraData;

                    string email = null;
                    string name = null;
                    if (userDataFromProvider.ContainsKey("email")) email = userDataFromProvider["email"];
                    if (userDataFromProvider.ContainsKey("name")) name = userDataFromProvider["name"];

                    //for legacy reasons, with twitter we use the text username instead of numeric userid
                    if (provider == "twitter" && !String.IsNullOrEmpty(result.UserName)) uniqueUserID = result.UserName;

                    return ProcessLoginResult(uniqueUserID, provider, name, email);
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.ToString());
            }

            return RedirectToAction("LoginFailed");
        }

        // Legacy entry point/endpoint for oauth logins
        // GET: /LoginProvider/

        public ActionResult Index(string _mode, bool? _forceLogin, string _redirectURL, string oauth_token, string denied)
        {

            //preserve url we're returning to after sign in
            if (!String.IsNullOrEmpty(_redirectURL))
            {
                Session["_redirectURL"] = _redirectURL;
            }

            //if denied access, redirect to normal redirect url
            if (denied != null)
            {
                if (_redirectURL == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return Redirect(_redirectURL);
                }
            }

            return RedirectToAction("BeginLogin");
            /*
            //if initiating a login, attempt to authenticate with twitter
            if (_mode == "silent" && String.IsNullOrEmpty(oauth_token))
            {
                //silently initiate pass through to twitter login
                if (_forceLogin == null) _forceLogin = true;
                TwitterConsumer.StartSignInWithTwitter((bool)_forceLogin).Send();
            }
            else
            {

                if (TwitterConsumer.IsTwitterConsumerConfigured)
                {

                    if (!String.IsNullOrEmpty(_redirectURL))
                    {
                        Session["_redirectURL"] = _redirectURL;
                    }

                    string screenName = null;
                    long userId = 0;
                    if (TwitterConsumer.TryFinishSignInWithTwitter(out screenName, out userId))
                    {
                        return ProcessLoginResult(screenName, screenName, null, "Twitter");
                    }
                }
            }

            return View();
             * */
        }

        private ActionResult ProcessLoginResult(string userIdentifier, string loginProvider, string name, string email)
        {
            //TODO: move all of this logic to UserManager
            string newSessionToken = Guid.NewGuid().ToString();

            var dataModel = new OCMEntities();
            var userDetails = dataModel.Users.FirstOrDefault(u => u.Identifier.ToLower() == userIdentifier.ToLower() && u.IdentityProvider.ToLower() == loginProvider.ToLower());
            if (userDetails == null)
            {
                //create new user details   
                userDetails = new Core.Data.User();
                userDetails.IdentityProvider = loginProvider;
                userDetails.Identifier = userIdentifier;
                if (String.IsNullOrEmpty(name) && loginProvider.ToLower() == "twitter") name = userIdentifier;
                if (String.IsNullOrEmpty(name) && email != null) name = email.Substring(0, email.IndexOf("@"));

                userDetails.Username = name;
                userDetails.EmailAddress = email;
                userDetails.DateCreated = DateTime.UtcNow;
                userDetails.DateLastLogin = DateTime.UtcNow;
                userDetails.IsProfilePublic = true;

                //only update session token if new (also done on logout)
                if (String.IsNullOrEmpty(userDetails.CurrentSessionToken)) userDetails.CurrentSessionToken = newSessionToken;

                dataModel.Users.Add(userDetails);
            }
            else
            {
                //update date last logged in and refresh users details if more information provided
                userDetails.DateLastLogin = DateTime.UtcNow;

                if (userDetails.Username == userDetails.Identifier && !String.IsNullOrEmpty(name)) userDetails.Username = name;
                if (String.IsNullOrEmpty(userDetails.EmailAddress) && !String.IsNullOrEmpty(email)) userDetails.EmailAddress = email;

                //only update session token if new (also done on logout)
                if (String.IsNullOrEmpty(userDetails.CurrentSessionToken)) userDetails.CurrentSessionToken = newSessionToken;
            }

            //get whichever session token we used
            //newSessionToken = userDetails.CurrentSessionToken;

            //store updates to user
            dataModel.SaveChanges();

            LoginProviderController.PerformCoreLogin(OCM.API.Common.Model.Extensions.User.FromDataModel(userDetails));

            if (!String.IsNullOrEmpty((string)Session["_redirectURL"]))
            {
                string returnURL = Session["_redirectURL"].ToString();
                return Redirect(returnURL);
            }
            else
            {
                //nowhere specified to redirect to, redirect to home page
                return RedirectToAction("Index", "Home");
            }
        }

        public static void PerformCoreLogin(OCM.API.Common.Model.User userDetails)
        {
            string permissions = (userDetails.Permissions != null ? userDetails.Permissions : "");
            var session = System.Web.HttpContext.Current.Session;
            var response = new HttpResponseWrapper(System.Web.HttpContext.Current.Response);

            UpdateCookie(response, "IdentityProvider", userDetails.IdentityProvider);
            UpdateCookie(response, "Identifier", userDetails.Identifier);
            UpdateCookie(response, "Username", userDetails.Username);
            UpdateCookie(response, "OCMSessionToken", userDetails.CurrentSessionToken);
            UpdateCookie(response, "AccessPermissions", permissions);

            session["IdentityProvider"] = userDetails.IdentityProvider;
            session["Identifier"] = userDetails.Identifier;
            session["Username"] = userDetails.Username;
            session["UserID"] = userDetails.ID;

            if (UserManager.IsUserAdministrator(userDetails))
            {
                session["IsAdministrator"] = true;
            }

        }

        public ActionResult AppLogin(bool? redirectWithToken)
        {
            if (redirectWithToken == true)
            {
                var IdentityProvider = GetCookie(Request, "IdentityProvider");
                var Identifier = GetCookie(Request, "Identifier");
                var Username = GetCookie(Request, "Username");
                var SessionToken = GetCookie(Request, "OCMSessionToken");
                var Permissions = GetCookie(Request, "Permissions");

                return RedirectToAction("AppLogin", new
                {
                    IdentityProvider = IdentityProvider,
                    Identifier = Identifier,
                    OCMSessionToken = SessionToken,
                    Permissions = Permissions
                });
            }
            else
            {
                return View();
            }

        }

        #endregion
    }
}
