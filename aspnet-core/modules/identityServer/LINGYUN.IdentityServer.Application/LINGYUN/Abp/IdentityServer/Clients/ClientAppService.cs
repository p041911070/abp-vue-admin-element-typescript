﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.IdentityServer.Clients;
using Volo.Abp.Security.Encryption;

namespace LINGYUN.Abp.IdentityServer.Clients
{
    public class ClientAppService : AbpIdentityServerAppServiceBase, IClientAppService
    {
        private IStringEncryptionService _encryptionService;
        protected IStringEncryptionService EncryptionService => LazyGetRequiredService(ref _encryptionService);

        protected IClientRepository ClientRepository { get; }

        public ClientAppService(IClientRepository clientRepository)
        {
            ClientRepository = clientRepository;
        }

        public virtual async Task<ClientClaimDto> AddClaimAsync(ClientClaimCreateDto clientClaimCreate)
        {
            var client = await ClientRepository.GetAsync(clientClaimCreate.ClientId);

            client.AddClaim(clientClaimCreate.Value, clientClaimCreate.Type);
            var clientClaim = client.FindClaim(clientClaimCreate.Value, clientClaimCreate.Type);

            return ObjectMapper.Map<ClientClaim, ClientClaimDto>(clientClaim);
        }

        public virtual async Task<ClientPropertyDto> AddPropertyAsync(ClientPropertyCreateDto clientPropertyCreate)
        {
            var client = await ClientRepository.GetAsync(clientPropertyCreate.ClientId);

            client.AddProperty(clientPropertyCreate.Key, clientPropertyCreate.Value);
            var clientProperty = client.FindProperty(clientPropertyCreate.Key, clientPropertyCreate.Value);

            return ObjectMapper.Map<ClientProperty, ClientPropertyDto>(clientProperty);
        }

        public virtual async Task<ClientSecretDto> AddSecretAsync(ClientSecretCreateDto clientSecretCreate)
        {
            var client = await ClientRepository.GetAsync(clientSecretCreate.ClientId);

            var clientSecretValue = EncryptionService.Encrypt(clientSecretCreate.Value);

            client.AddSecret(clientSecretValue, clientSecretCreate.Expiration, 
                clientSecretCreate.Type, clientSecretCreate.Description);
            
            var clientSecret = client.FindSecret(clientSecretValue, clientSecretCreate.Type);

            return ObjectMapper.Map<ClientSecret, ClientSecretDto>(clientSecret);
        }

        public virtual async Task<ClientDto> CreateAsync(ClientCreateDto clientCreate)
        {
            var clientIdExists = await ClientRepository.CheckClientIdExistAsync(clientCreate.ClientId);
            if(clientIdExists)
            {
                throw new UserFriendlyException(L[AbpIdentityServerErrorConsts.ClientIdExisted, clientCreate.ClientId]);
            }
            var client = new Client(GuidGenerator.Create(), clientCreate.ClientId);
            client.ClientName = clientCreate.ClientName;
            client.Description = clientCreate.Description;
            foreach(var grantType in clientCreate.AllowedGrantTypes)
            {
                client.AddGrantType(grantType.GrantType);
            }

            client = await ClientRepository.InsertAsync(client, true);

            return ObjectMapper.Map<Client, ClientDto>(client);
        }

        public virtual async Task DeleteAsync(ClientGetByIdInputDto clientGetByIdInput)
        {
            await ClientRepository.DeleteAsync(clientGetByIdInput.Id);
        }

        public virtual async Task DeleteClaimAsync(ClientClaimGetByKeyInputDto clientClaimGetByKey)
        {
            var client = await ClientRepository.GetAsync(clientClaimGetByKey.ClientId);
            client.RemoveClaim(clientClaimGetByKey.Value, clientClaimGetByKey.Type);
            await ClientRepository.UpdateAsync(client);
        }

        public virtual async Task DeletePropertyAsync(ClientPropertyGetByKeyDto clientPropertyGetByKey)
        {
            var client = await ClientRepository.GetAsync(clientPropertyGetByKey.ClientId);
            client.RemoveProperty(clientPropertyGetByKey.Key, clientPropertyGetByKey.Value);
            await ClientRepository.UpdateAsync(client);
        }

        public virtual async Task DeleteSecretAsync(ClientSecretGetByTypeDto clientSecretGetByType)
        {
            var client = await ClientRepository.GetAsync(clientSecretGetByType.ClientId);
            client.RemoveSecret(clientSecretGetByType.Value, clientSecretGetByType.Type);
            await ClientRepository.UpdateAsync(client);
        }

        public virtual async Task<ClientDto> GetAsync(ClientGetByIdInputDto clientGetById)
        {
            var client = await ClientRepository.GetAsync(clientGetById.Id);

            return ObjectMapper.Map<Client, ClientDto>(client);
        }

        public virtual async Task<PagedResultDto<ClientDto>> GetAsync(ClientGetByPagedInputDto clientGetByPaged)
        {
            // Abp官方IdentityServer项目不支持Filter过滤...
            var clients = await ClientRepository.GetListAsync(clientGetByPaged.Sorting,
                clientGetByPaged.SkipCount, clientGetByPaged.MaxResultCount, true);

            var clientCount = await ClientRepository.GetCountAsync();

            return new PagedResultDto<ClientDto>(clientCount, 
                ObjectMapper.Map<List<Client>, List<ClientDto>>(clients));
        }

        public virtual async Task<ClientDto> UpdateAsync(ClientUpdateInputDto clientUpdateInput)
        {
            var client = await ClientRepository.GetAsync(clientUpdateInput.Id);

            #region Basic Property
            client.ConcurrencyStamp = clientUpdateInput.Client.ConcurrencyStamp;
            client.ClientId = clientUpdateInput.Client.ClientId ?? client.ClientId;
            client.ClientUri = clientUpdateInput.Client.ClientUri ?? client.ClientUri;
            client.ClientName = clientUpdateInput.Client.ClientName ?? client.ClientName;
            client.AbsoluteRefreshTokenLifetime = clientUpdateInput.Client.AbsoluteRefreshTokenLifetime
                ?? client.AbsoluteRefreshTokenLifetime;
            client.AccessTokenLifetime = clientUpdateInput.Client.AccessTokenLifetime
                ?? client.AccessTokenLifetime;
            client.AccessTokenType = clientUpdateInput.Client.AccessTokenType ?? client.AccessTokenType;
            client.AllowAccessTokensViaBrowser = clientUpdateInput.Client.AllowAccessTokensViaBrowser
                ?? client.AllowAccessTokensViaBrowser;
            client.AllowOfflineAccess = clientUpdateInput.Client.AllowOfflineAccess
                ?? client.AllowOfflineAccess;
            client.AllowPlainTextPkce = clientUpdateInput.Client.AllowPlainTextPkce ?? client.AllowPlainTextPkce;
            client.AllowRememberConsent = clientUpdateInput.Client.AllowRememberConsent ?? client.AllowRememberConsent;
            client.AlwaysIncludeUserClaimsInIdToken = clientUpdateInput.Client.AlwaysIncludeUserClaimsInIdToken
                ?? client.AlwaysIncludeUserClaimsInIdToken;
            client.AlwaysSendClientClaims = clientUpdateInput.Client.AlwaysSendClientClaims ?? client.AlwaysSendClientClaims;
            client.AuthorizationCodeLifetime = clientUpdateInput.Client.AuthorizationCodeLifetime
                ?? client.AuthorizationCodeLifetime;
            client.BackChannelLogoutSessionRequired = clientUpdateInput.Client.BackChannelLogoutSessionRequired
                ?? client.BackChannelLogoutSessionRequired;

            client.BackChannelLogoutUri = clientUpdateInput.Client.BackChannelLogoutUri
                ?? client.BackChannelLogoutUri;
            client.ClientClaimsPrefix = clientUpdateInput.Client.ClientClaimsPrefix ?? client.ClientClaimsPrefix;
            client.ConsentLifetime = clientUpdateInput.Client.ConsentLifetime ?? client.ConsentLifetime;
            client.Description = clientUpdateInput.Client.Description ?? client.Description;
            client.DeviceCodeLifetime = clientUpdateInput.Client.DeviceCodeLifetime ?? client.DeviceCodeLifetime;
            client.Enabled = clientUpdateInput.Client.Enabled ?? client.Enabled;
            client.EnableLocalLogin = clientUpdateInput.Client.EnableLocalLogin ?? client.EnableLocalLogin;
            client.FrontChannelLogoutSessionRequired = clientUpdateInput.Client.FrontChannelLogoutSessionRequired
                ?? client.FrontChannelLogoutSessionRequired;
            client.FrontChannelLogoutUri = clientUpdateInput.Client.FrontChannelLogoutUri ?? client.FrontChannelLogoutUri;

            client.IdentityTokenLifetime = clientUpdateInput.Client.IdentityTokenLifetime ?? client.IdentityTokenLifetime;
            client.IncludeJwtId = clientUpdateInput.Client.IncludeJwtId ?? client.IncludeJwtId;
            client.LogoUri = clientUpdateInput.Client.LogoUri ?? client.LogoUri;
            client.PairWiseSubjectSalt = clientUpdateInput.Client.PairWiseSubjectSalt ?? client.PairWiseSubjectSalt;
            client.ProtocolType = clientUpdateInput.Client.ProtocolType ?? client.ProtocolType;
            client.RefreshTokenExpiration = clientUpdateInput.Client.RefreshTokenExpiration ?? client.RefreshTokenExpiration;
            client.RefreshTokenUsage = clientUpdateInput.Client.RefreshTokenUsage ?? client.RefreshTokenUsage;
            client.RequireClientSecret = clientUpdateInput.Client.RequireClientSecret ?? client.RequireClientSecret;
            client.RequireConsent = clientUpdateInput.Client.RequireConsent ?? client.RequireConsent;

            client.RequirePkce = clientUpdateInput.Client.RequirePkce ?? client.RequirePkce;
            client.SlidingRefreshTokenLifetime = clientUpdateInput.Client.SlidingRefreshTokenLifetime
                ?? client.SlidingRefreshTokenLifetime;
            client.UpdateAccessTokenClaimsOnRefresh = clientUpdateInput.Client.UpdateAccessTokenClaimsOnRefresh
                ?? client.UpdateAccessTokenClaimsOnRefresh;

            client.UserCodeType = clientUpdateInput.Client.UserCodeType ?? client.UserCodeType;
            client.UserSsoLifetime = clientUpdateInput.Client.UserSsoLifetime ?? client.UserSsoLifetime;
            #endregion

            #region AllowScope

            foreach(var scope in clientUpdateInput.Client.AllowedScopes)
            {
                var clientScope = client.FindScope(scope.Scope);
                if (clientScope == null)
                {
                    client.AddScope(scope.Scope);
                }
            }

            #endregion

            #region RedirectUris

            foreach(var redirect in clientUpdateInput.Client.RedirectUris)
            {
                var clientRedirect = client.FindRedirectUri(redirect.RedirectUri);
                if(clientRedirect == null)
                {
                    client.AddRedirectUri(redirect.RedirectUri);
                }
            }

            #endregion

            #region AllowedGrantTypes

            foreach (var grantType in clientUpdateInput.Client.AllowedGrantTypes)
            {
                var clientGrantType = client.FindGrantType(grantType.GrantType);
                if (clientGrantType == null)
                {
                    client.AddGrantType(grantType.GrantType);
                }
            }

            #endregion

            #region AllowedCorsOrigins

            foreach (var corgOrigin in clientUpdateInput.Client.AllowedCorsOrigins)
            {
                var clientCorsOrigin = client.FindCorsOrigin(corgOrigin.Origin);
                if (clientCorsOrigin == null)
                {
                    client.AddCorsOrigin(corgOrigin.Origin);
                }
            }

            #endregion

            #region PostLogoutRedirectUris

            foreach (var logoutRedirect in clientUpdateInput.Client.PostLogoutRedirectUris)
            {
                var clientLogoutRedirect = client.FindPostLogoutRedirectUri(logoutRedirect.PostLogoutRedirectUri);
                if (clientLogoutRedirect == null)
                {
                    client.AddPostLogoutRedirectUri(logoutRedirect.PostLogoutRedirectUri);
                }
            }

            #endregion

            #region IdentityProviderRestrictions

            foreach (var provider in clientUpdateInput.Client.IdentityProviderRestrictions)
            {
                var clientIdentityProvider = client.FindIdentityProviderRestriction(provider.Provider);
                if (clientIdentityProvider == null)
                {
                    client.AddIdentityProviderRestriction(provider.Provider);
                }
            }

            #endregion

            client = await ClientRepository.UpdateAsync(client, true);

            return ObjectMapper.Map<Client, ClientDto>(client);
        }

        public virtual async Task<ClientClaimDto> UpdateClaimAsync(ClientClaimUpdateDto clientClaimUpdate)
        {
            var client = await ClientRepository.GetAsync(clientClaimUpdate.ClientId);

            var clientClaim = client.Claims.FirstOrDefault(claim => claim.Type.Equals(clientClaimUpdate.Type));
            if(clientClaim == null)
            {
                throw new UserFriendlyException(L[AbpIdentityServerErrorConsts.ClientClaimNotFound, clientClaimUpdate.Type]);
            }
            clientClaim.Value = clientClaimUpdate.Value;

            return ObjectMapper.Map<ClientClaim, ClientClaimDto>(clientClaim);
        }

        public virtual async Task<ClientPropertyDto> UpdatePropertyAsync(ClientPropertyUpdateDto clientPropertyUpdate)
        {
            var client = await ClientRepository.GetAsync(clientPropertyUpdate.ClientId);

            var clientProperty = client.Properties
                .FirstOrDefault(property => property.Key.Equals(clientPropertyUpdate.Key));
            if (clientProperty == null)
            {
                throw new UserFriendlyException(
                    L[AbpIdentityServerErrorConsts.ClientPropertyNotFound, clientPropertyUpdate.Key]);
            }
            clientProperty.Value = clientPropertyUpdate.Value;

            return ObjectMapper.Map<ClientProperty, ClientPropertyDto>(clientProperty);
        }

        public virtual async Task<ClientSecretDto> UpdateSecretAsync(ClientSecretUpdateDto clientSecretUpdate)
        {
            var client = await ClientRepository.GetAsync(clientSecretUpdate.ClientId);

            var clientSecret = client.ClientSecrets
                .FirstOrDefault(secret => secret.Type.Equals(clientSecretUpdate.Type));
            if (clientSecret == null)
            {
                throw new UserFriendlyException(
                    L[AbpIdentityServerErrorConsts.ClientSecretNotFound, clientSecretUpdate.Type]);
            }
            clientSecret.Value = EncryptionService.Encrypt(clientSecretUpdate.Value);

            return ObjectMapper.Map<ClientSecret, ClientSecretDto>(clientSecret);
        }
    }
}
