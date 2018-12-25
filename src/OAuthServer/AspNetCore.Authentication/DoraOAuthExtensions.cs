// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Dora.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DoraOAuthExtensions
    {
        public static AuthenticationBuilder AddDora<TUser>(this AuthenticationBuilder builder, Action<DoraOAuthOptions<TUser>> configureOptions)
            => builder.AddDora("Dora", "Dora", configureOptions);
        public static AuthenticationBuilder AddDora<TUser>(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<DoraOAuthOptions<TUser>> configureOptions)
            => builder.AddOAuth<DoraOAuthOptions<TUser>, DoraOAuthtHandler<TUser>>(authenticationScheme, displayName, configureOptions);
    }
}