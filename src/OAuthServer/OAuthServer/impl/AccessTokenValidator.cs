//using Microsoft.AspNetCore.Authentication;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Dora.OAuthServer
//{
//    /// <summary>
//    /// The default access token validator.
//    /// </summary>
//    public class AccessTokenValidator : IAccessTokenValidator
//    {
//        private readonly ISystemClock _systemClock;
//        private readonly IOAuthGrantStore _grantStore;

//        /// <summary>
//        /// Creates a new <see cref="AccessTokenValidator"/>.
//        /// </summary>
//        /// <param name="systemClock">The <see cref="ISystemClock"/> to provide standard time for expiration validation.</param>
//        /// <param name="grantStore">The <see cref="IOAuthGrantStore"/> is check if specified access token is revoked.</param>
//        /// <exception cref="ArgumentNullException">Specified <paramref name="systemClock"/> is null.</exception>
//        /// <exception cref="ArgumentNullException">Specified <paramref name="grantStore"/> is null.</exception>
//        public AccessTokenValidator(ISystemClock systemClock, IOAuthGrantStore grantStore)
//        {
//            _systemClock = Guard.ArgumentNotNull(systemClock, nameof(systemClock));
//            _grantStore = Guard.ArgumentNotNull(grantStore, nameof(grantStore));
//        }

//        /// <summary>
//        ///  Validates the access token.
//        /// </summary>
//        /// <param name="context">The <see cref="AccessTokenValidationContext"/> representing the execution context in which the access token validation is performed.</param>
//        /// <returns>The task to validate the access token.</returns>
//        /// <exception cref="ArgumentNullException">Specified <paramref name="context"/> is null.</exception>
//        public async Task ValidateAsync(AccessTokenValidationContext context)
//        {
//            Guard.ArgumentNotNull(context, nameof(context));

//            if (context.AccessToken.HasExpired(_systemClock))
//            {
//                context.Failed(OAuthErrors.UnauthorizedClient.AccessTokenHasExpired);
//                return;
//            }

//            if (!await _grantStore.VaidateAccessTokenAsync(context.AccessToken.Fingerprint))
//            {
//                context.Failed(OAuthErrors.UnauthorizedClient.AccessTokenIsRevoked);
//                return;
//            }


//            if (context.TargetScopes.Except(context.AccessToken.Scopes).Any())
//            {
//                context.Failed(OAuthErrors.UnauthorizedClient.UnauthorizedScopes);
//            }
//        }
//    }

//}
