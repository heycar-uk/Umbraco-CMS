﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Security;
using Umbraco.Core.Configuration;
using Umbraco.Core.Security;

namespace Umbraco.Core.Security
{
    internal static class MembershipProviderExtensions
    {
        public static MembershipUserCollection FindUsersByName(this MembershipProvider provider, string usernameToMatch)
        {
            int totalRecords = 0;
            return provider.FindUsersByName(usernameToMatch, 0, int.MaxValue, out totalRecords);
        }

        public static MembershipUserCollection FindUsersByEmail(this MembershipProvider provider, string emailToMatch)
        {
            int totalRecords = 0;
            return provider.FindUsersByEmail(emailToMatch, 0, int.MaxValue, out totalRecords);
        }

        public static MembershipUser CreateUser(this MembershipProvider provider, string username, string password, string email)
        {
            MembershipCreateStatus status;
            var user = provider.CreateUser(username, password, email, null, null, true, null, out status);
            if (user == null)
                throw new MembershipCreateUserException(status);
            return user;
        }

        /// <summary>
        /// Method to get the Umbraco Members membership provider based on it's alias
        /// </summary>
        /// <returns></returns>
        public static MembershipProvider GetMembersMembershipProvider()
        {
            if (Membership.Providers[Constants.Conventions.Member.UmbracoMemberProviderName] == null)
            {
                throw new InvalidOperationException("No membership provider found with name " + Constants.Conventions.Member.UmbracoMemberProviderName);
            }
            return Membership.Providers[Constants.Conventions.Member.UmbracoMemberProviderName];
        }

        /// <summary>
        /// Method to get the Umbraco Users membership provider based on it's alias
        /// </summary>
        /// <returns></returns>
        public static MembershipProvider GetUsersMembershipProvider()
        {
            if (Membership.Providers[UmbracoSettings.DefaultBackofficeProvider] == null)
            {
                throw new InvalidOperationException("No membership provider found with name " + UmbracoSettings.DefaultBackofficeProvider);
            }
            return Membership.Providers[UmbracoSettings.DefaultBackofficeProvider];
        }

        public static MembershipUser GetCurrentUser(this MembershipProvider membershipProvider)
        {
            var username = membershipProvider.GetCurrentUserName();
            return membershipProvider.GetUser(username, true);
        }

        /// <summary>
        /// Just returns the current user's login name (just a wrapper).
        /// </summary>
        /// <param name="membershipProvider"></param>
        /// <returns></returns>
        public static string GetCurrentUserName(this MembershipProvider membershipProvider)
        {
            if (HostingEnvironment.IsHosted)
            {
                HttpContext current = HttpContext.Current;
                if (current != null)
                    return current.User.Identity.Name;
            }
            IPrincipal currentPrincipal = Thread.CurrentPrincipal;
            if (currentPrincipal == null || currentPrincipal.Identity == null)
                return string.Empty;
            else
                return currentPrincipal.Identity.Name;
        }

        /// <summary>
        /// Returns true if the provider specified is a built-in Umbraco users provider
        /// </summary>
        /// <param name="membershipProvider"></param>
        /// <returns></returns>
        public static bool IsUmbracoUsersProvider(this MembershipProvider membershipProvider)
        {
            return (membershipProvider is IUsersMembershipProvider);
        }

        /// <summary>
        /// Returns true if the provider specified is a built-in Umbraco membership provider
        /// </summary>
        /// <param name="membershipProvider"></param>
        /// <returns></returns>
        public static bool IsUmbracoMembershipProvider(this MembershipProvider membershipProvider)
        {
            return (membershipProvider is UmbracoMembershipProviderBase);
        }

        //TODO: Add role provider checks too

        public static UmbracoMembershipProviderBase AsUmbracoMembershipProvider(this MembershipProvider membershipProvider)
        {
            return (UmbracoMembershipProviderBase)membershipProvider;
        }

    }
}
