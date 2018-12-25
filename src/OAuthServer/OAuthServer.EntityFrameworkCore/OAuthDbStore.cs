using Dora.OAuthServer.Properties;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dora.OAuthServer
{
    public class OAuthDbStore<TDbContext>: 
        IDelegateConsentStore, 
        IOAuthGrantStore,
        IApplicationStore
        where TDbContext : DbContext
    {
        public TDbContext DbContext { get;  }

        /// <summary>
        /// The <see cref="ILookupNormalizer"/> to normalize identifiers which are case insensitive.
        /// </summary>
        public ILookupNormalizer Normalizer { get; }

        public OAuthDbStore(TDbContext dbContext, ILookupNormalizer normalizer)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            Normalizer = normalizer ?? throw new ArgumentNullException(nameof(normalizer));
        }

        async Task IDelegateConsentStore.AddAsync(string clientId, string userName, IEnumerable<string> scopes)
        {
            clientId = Normalizer.Normalize(Guard.ArgumentNotNullOrWhiteSpace(clientId, nameof(clientId)));
            userName = Normalizer.Normalize(Guard.ArgumentNotNullOrWhiteSpace(userName, nameof(userName)));
            Guard.ElementNotNullOrWhiteSpace(scopes, nameof(scopes), false);
            scopes = scopes.Select(it => Normalizer.Normalize(it)).ToArray();

            var entity = await DbContext.FindAsync<DelegateConsentEntity>(clientId, userName);
            if (null == entity)
            {
                entity = new DelegateConsentEntity
                {
                    ClientId = clientId,
                    UserName = userName,
                    Scopes = string.Join(Constants.SeperatorString, scopes)
                };
                await DbContext.AddAsync(entity);
                await DbContext.SaveChangesAsync();
            }
            else
            {
                var existingScopes = entity.Scopes.Split(Constants.SeperatorCharacter);
                entity.Scopes = string.Join(Constants.SeperatorString.ToString(), existingScopes.Union(scopes));
                DbContext.Attach(entity);
                DbContext.Update(entity);
            }

            await DbContext.SaveChangesAsync();
        }

        async Task IDelegateConsentStore.RemoveAllScopesAsync(string clientId, string userName)
        {
            clientId = this.Normalizer.Normalize(Guard.ArgumentNotNullOrWhiteSpace(clientId, nameof(clientId)));
            userName = this.Normalizer.Normalize(Guard.ArgumentNotNullOrWhiteSpace(userName, nameof(userName)));
            DbContext.Remove(new DelegateConsentEntity { ClientId = clientId, UserName = userName });
            await DbContext.SaveChangesAsync();
        }

        async Task IDelegateConsentStore.RemoveScopesAsync(string clientId, string userName, IEnumerable<string> scopes)
        {
            clientId = Normalizer.Normalize(Guard.ArgumentNotNullOrWhiteSpace(clientId, nameof(clientId)));
            userName = Normalizer.Normalize(Guard.ArgumentNotNullOrWhiteSpace(userName, nameof(userName)));
            Guard.ElementNotNullOrWhiteSpace(scopes, nameof(scopes), false);

            var entity = await DbContext.FindAsync<DelegateConsentEntity>(clientId, userName);
            if (null == entity)
            {
                throw new OAuthException(string.Format(Resources.ExceptionConsentNotExists, userName, clientId));
            }
            var allScopes = entity.Scopes.Split(Constants.SeperatorCharacter);
            scopes = scopes.Select(it => Normalizer.Normalize(it)).ToArray();
            var except = scopes.Except(allScopes);
            if (except.Any())
            {
                throw new OAuthException(string.Format(Resources.ExceptionConsentScopeNotExists, userName, clientId, except.First()));
            }
            var newScopes = allScopes.Except(scopes);
            if (newScopes.Any())
            {
                entity.Scopes = string.Join(Constants.SeperatorString, allScopes.Except(scopes));
                DbContext.Attach(entity);
                DbContext.Update(entity);
            }
            else
            {
                DbContext.Remove(entity);
            }
            await DbContext.SaveChangesAsync();
        }

        async Task<DelegateConsent> IDelegateConsentStore.GetAsync(string clientId, string userName)
        {
            clientId = Normalizer.Normalize(Guard.ArgumentNotNullOrWhiteSpace(clientId, nameof(clientId)));
            userName = Normalizer.Normalize(Guard.ArgumentNotNullOrWhiteSpace(userName, nameof(userName)));
            var entity = await DbContext.FindAsync<DelegateConsentEntity>(clientId, userName);
            return entity?.ToOAuthDelegateConsent();
        }

        async Task<IEnumerable<DelegateConsent>> IDelegateConsentStore.GetAsync(string userName)
        {
            userName = Normalizer.Normalize(Guard.ArgumentNotNullOrWhiteSpace(userName, nameof(userName)));
            var result = await (from it in DbContext.Set<DelegateConsentEntity>()
                          where it.UserName == userName
                          select it).ToListAsync();
            return result.Select(it => it.ToOAuthDelegateConsent());
        }

        async Task IOAuthGrantStore.UpdateAuthorizationCodeAsync(string clientId, string userName, string authorizationCode)
        {
            clientId = Normalizer.Normalize(Guard.ArgumentNotNullOrWhiteSpace(clientId, nameof(clientId)));
            userName = Normalizer.Normalize(Guard.ArgumentNotNullOrWhiteSpace(userName, nameof(userName)));
            Guard.ArgumentNotNullOrWhiteSpace(authorizationCode, nameof(authorizationCode));

            var entity = await this.DbContext.FindAsync<OAuthGrantEntity>(clientId, userName);
            if (entity != null)
            {
                entity.AuthorizationCode = authorizationCode;
                entity.RefreshToken = null;
                entity.AccessToken = null;
                DbContext.Update(entity);
            }
            else
            {
                entity = OAuthGrantEntity.Create(clientId, userName, authorizationCode, null, null, Normalizer);
                await DbContext.AddAsync(entity);
            }
            await this.DbContext.SaveChangesAsync();
        }

        async Task IOAuthGrantStore.UpdateTokensAsync(string clientId, string userName, string refreshToken, string accessToken)
        {
            clientId = this.Normalizer.Normalize(Guard.ArgumentNotNullOrWhiteSpace(clientId, nameof(clientId)));
            userName = this.Normalizer.Normalize(Guard.ArgumentNotNullOrWhiteSpace(userName, nameof(userName)));
            Guard.ArgumentNotNullOrWhiteSpace(accessToken, nameof(accessToken));

            var entity = await DbContext.FindAsync<OAuthGrantEntity>(clientId, userName);
            if (entity != null)
            {
                entity.AuthorizationCode = null;
                entity.RefreshToken = refreshToken;
                entity.AccessToken = accessToken;
                DbContext.Attach(entity);
                DbContext.Update(entity);
            }
            else
            {
                entity = OAuthGrantEntity.Create(clientId, userName, null, accessToken, refreshToken, Normalizer);
                await DbContext.AddAsync(entity);
            }
            await DbContext.SaveChangesAsync();
        }

        async Task<bool> IOAuthGrantStore.VaidateAuthorizationCodeAsync(string authorizationCode)
        {
            authorizationCode = Normalizer.Normalize(Guard.ArgumentNotNullOrWhiteSpace(authorizationCode, nameof(authorizationCode)));
            return await DbContext.Set<OAuthGrantEntity>().AnyAsync(it => it.AuthorizationCode == authorizationCode);
        }

        async Task<bool> IOAuthGrantStore.VaidateRefreshTokenAsync(string refreshToken)
        {
            refreshToken = Normalizer.Normalize(Guard.ArgumentNotNullOrWhiteSpace(refreshToken, nameof(refreshToken)));
            return await DbContext.Set<OAuthGrantEntity>().AnyAsync(it => it.RefreshToken == refreshToken);
        }

        async Task<bool> IOAuthGrantStore.VaidateAccessTokenAsync(string accessToken)
        {
            accessToken = Normalizer.Normalize(Guard.ArgumentNotNullOrWhiteSpace(accessToken, nameof(accessToken)));
            return await DbContext.Set<OAuthGrantEntity>().AnyAsync(it => it.AccessToken == accessToken);
        }

        async Task<string> IOAuthGrantStore.GetUserNameAsync(string authorizationCode)
        {
            Guard.ArgumentNotNullOrWhiteSpace(authorizationCode, nameof(authorizationCode));
            return (await DbContext.Set<OAuthGrantEntity>().SingleOrDefaultAsync(it => it.AuthorizationCode == authorizationCode))?.UserName;
        }

        async Task IOAuthGrantStore.RevokeAccessTokenAsync(string accessToken)
        {
            Guard.ArgumentNotNullOrWhiteSpace(accessToken, nameof(accessToken));
            var grant = await DbContext.Set<OAuthGrantEntity>().SingleOrDefaultAsync(it => it.AccessToken == accessToken);
            if (null == grant)
            {
                throw new OAuthException(Resources.ExceptionRevokedAccessTokenNotExists);
            }
            grant.AccessToken = null;
            DbContext.Attach(grant);
            DbContext.Update(grant);
            await DbContext.SaveChangesAsync();
        }

        async Task IApplicationStore.CreateAsync(Application application)
        {
            var entity = ApplicationEntity.Create(Guard.ArgumentNotNull(application, nameof(application)), Normalizer);
            await DbContext.AddAsync(entity);
            await DbContext.SaveChangesAsync();
        }

        async Task IApplicationStore.UpdateAsync(Application application)
        {
            var entity = ApplicationEntity.Create(Guard.ArgumentNotNull(application, nameof(application)), Normalizer);
            DbContext.Attach(entity);
            DbContext.Update(entity);
            await DbContext.SaveChangesAsync();
        }

        async Task<Application> IApplicationStore.GetByClientIdAsync(string clientId)
        {
            var entity = await this.DbContext.FindAsync<ApplicationEntity>(Normalizer.Normalize(Guard.ArgumentNotNullOrWhiteSpace(clientId, nameof(clientId))));
            return entity?.ToApplication();
        }

        async Task<IEnumerable<Application>> IApplicationStore.GetOneByUserNameAsync(string owner)
        {
            Guard.ArgumentNotNullOrWhiteSpace(owner, nameof(owner));
            var result = await (from app in this.DbContext.Set<ApplicationEntity>()
                         where app.Owner == this.Normalizer.Normalize(owner)
                         select app).ToListAsync();
            return result.Select(it => it.ToApplication()).ToArray();
        }

        async Task IApplicationStore.DeleteAsync(string clientId)
        {
            Guard.ArgumentNotNullOrWhiteSpace(clientId, nameof(clientId));
            ApplicationEntity entity = new ApplicationEntity { ClientId = clientId };
            DbContext.Attach(entity);
            DbContext.Entry(entity).State = EntityState.Deleted;
            await DbContext.SaveChangesAsync();
        }
    }
}
