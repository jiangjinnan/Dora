using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using System;
using System.Linq.Expressions;

namespace Dora.AspNetCore.Authentication
{
    public class DoraOAuthOptions<TUser>: OAuthOptions
    {      
        public DoraOAuthOptions<TUser> MapJsonKey<TProperty>(string claimType, Expression<Func<TUser,TProperty>> propertyAccessor)
        {
            ClaimActions.MapJsonKey(claimType, ToCamelCase((propertyAccessor.Body as MemberExpression).Member.Name));
            return this;
        }
        private static string ToCamelCase(string name) => $"{((char)char.ToLowerInvariant(name[0]))}{name.Substring(1)}";
    }
}