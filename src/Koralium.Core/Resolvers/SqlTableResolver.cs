﻿using Koralium.Core.Interfaces;
using Koralium.Core.Metadata;
using Koralium.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koralium.Core.Resolvers
{
    internal class SqlTableResolver : SqlToExpression.ITableResolver
    {
        private readonly MetadataStore _metadataStore;
        public SqlTableResolver(MetadataStore metadataStore)
        {
            _metadataStore = metadataStore;
        }

        public async ValueTask<IQueryable> ResolveTableName(string name, object additionalData)
        {
            if(!(additionalData is TableResolverData tableResolverData))
            {
                throw new Exception();
            }

            if(_metadataStore.TryGetTable(name, out var table))
            {
                await CheckAuthorization(tableResolverData.ServiceProvider, table.SecurityPolicy, tableResolverData.HttpContext);

                var resolver = (ITableResolver)tableResolverData.ServiceProvider.GetRequiredService(table.Resolver);

                return await resolver.GetQueryable(tableResolverData.HttpContext);
            }
            else
            {
                //TODO: Fix exceptions
                throw new Exception();
            }
        }

        private async Task CheckAuthorization(IServiceProvider serviceProvider, string securityPolicy, HttpContext context)
        {
            //Check authentication and authorization
            if (securityPolicy != null)
            {
                var authorizationPolicyProvider = serviceProvider.GetRequiredService<IAuthorizationPolicyProvider>();
                var authorizationHandlerProvider = serviceProvider.GetRequiredService<IAuthorizationHandlerProvider>();
                var user = context.User;
                AuthorizationPolicy policy = null;
                if (securityPolicy.Equals("default"))
                {
                    policy = await authorizationPolicyProvider.GetDefaultPolicyAsync();
                }
                else
                {
                    policy = await authorizationPolicyProvider.GetPolicyAsync(securityPolicy);
                }
                var authContext = new AuthorizationHandlerContext(policy.Requirements, user, null);
                var authHandlers = await authorizationHandlerProvider.GetHandlersAsync(authContext);

                foreach (var authHandler in authHandlers)
                {
                    await authHandler.HandleAsync(authContext);
                }
                if (!authContext.HasSucceeded)
                {
                    //TODO: make good exception
                    throw new Exception("");
                }
            }
        }
    }
}
