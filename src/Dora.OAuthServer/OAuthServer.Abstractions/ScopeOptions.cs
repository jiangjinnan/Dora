using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.OAuthServer
{
    /// <summary>
    /// The OAuth scopes based options.
    /// </summary>
    public class ScopeOptions
    {
        #region Properties
        /// <summary>
        /// Identifier of the scope.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Title of the scope displayed in the delegate conset page.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Icon url of the scope displayed in the delegate conset page.
        /// </summary>
        public string IconUrl { get; set; }

        /// <summary>
        /// Description the scope displayed in the delegate conset page.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Indicates whether this scope is a default one, which is used when the scope is not explicitly provided.
        /// </summary>
        public bool Optional { get; set; }
        #endregion

        /// <summary>
        /// Convert to a <see cref="DelegateScope"/>.
        /// </summary>
        /// <returns>The <see cref="DelegateScope"/> to which the current <see cref="ScopeOptions"/> is converted.</returns>
        public DelegateScope ToOAuthScope()
        {
            return new DelegateScope(Id, Title, Description, Optional, IconUrl);
        }
    }
}
