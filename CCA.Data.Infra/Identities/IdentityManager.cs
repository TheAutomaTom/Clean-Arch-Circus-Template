﻿using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;
using CCA.Core.Application.Interfaces.Auth;
using CCA.Core.Application.Interfaces.Infrastructure;
using CCA.Core.Infra.Models.Identities;
using CCA.Core.Infra.Models.Responses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CCA.Data.Infra.Identities
{
	public class IdentityManager : IManageIdentities
  {
    public IdentityManagerSettings Settings { get; }

    readonly ILogger<IdentityManager> _logger;
    readonly ICache _cache;

    readonly HttpClient _adminClient;
		readonly JsonSerializerOptions _jsonOptions;


    public IdentityManager(IOptions<IdentityManagerSettings> settings, ILogger<IdentityManager> logger, ICache cache)
    {
      Settings = settings.Value;

      _logger = logger;
      _cache = cache;

      _adminClient = new HttpClient()
      {
        BaseAddress = new Uri(settings.Value.BaseAddress)
      };

			_jsonOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
		}

    public async Task<Result> CreateUser(IdentityCreateDto user, string role)
    {
      try
      {
        // Url
        var url = $"{Settings.PathToGetUser}";
        url = url.Replace("{{Realm}}", Settings.KeycloakRealm);

        // Content
        var json = JsonSerializer.Serialize(user);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Auth token
        var token = await getAdminToken();
        _adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Send request
        var response = await _adminClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

        // Manually get the new user's Id to use for further operations
        var search = new IdentitySearchDto()
        {
          Email = user.Email,
          SearchParams = [IdentitySearchParam.Email]

        };

        var userDetail = await getUser(search);

				var assignedRole = await assignRole(userDetail, Enum.Parse<UserRole>(role));
				

        return Result.Ok();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failed to create user");
				return Result.Fail(ex);
      }
    }

    async Task<IdentityGetDto> getUser(IdentitySearchDto search)
    {
      // Url base
      var url = $"{Settings.PathToGetUser}";
      url = url.Replace("{{Realm}}", Settings.KeycloakRealm);

      // Url query parameters // TODO: getUserBy(KeycloakUserQuery query)
      var queryString = HttpUtility.ParseQueryString(string.Empty);
      queryString.Add(IdentitySearchParam.Email.ToString(), search.Email);      
      url += "?" + queryString.ToString();

      // Auth token
      var token = await getAdminToken();
      _adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

      // Send
      var response = await _adminClient.GetAsync(url);
      response.EnsureSuccessStatusCode();

      // Receive
      var responseJson = await response.Content.ReadAsStringAsync();
      var result = JsonSerializer.Deserialize<List<IdentityGetDto>>(responseJson, _jsonOptions);

      if (result!.Count == 0)
      {
        return null;
      }
      
      // TODO: refactor user query and add `exact` param to avoid multiple hits.
      if (result.Count > 1)
      {
        _logger.LogWarning("More than one user found for search criteria: {search}", search);
      }

      // Assign roles to user.

      return result.FirstOrDefault()!;
    }


    async Task<Result> assignRole(IdentityGetDto userDetail, UserRole roleRequested)
    {
			// Content
      var role = await getRoleDetailByName(roleRequested);
			var roleRequest = new UserRoleDetail[] {role };
			var json = JsonSerializer.Serialize(roleRequest);
			var content = new StringContent(json, Encoding.UTF8, "application/json");

			// Url
			var url = $"{Settings.PathToAssignUseRole}";
			url = url.Replace("{{Realm}}", Settings.KeycloakRealm);
			url = url.Replace("{{User-Uuid}}", userDetail.Id);
			url = url.Replace("{{Ui-Client-Uuid}}", Settings.UiClientUuid);

			// Auth token
			var token = await getAdminToken();
			_adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

			// Send request
			var response = await _adminClient.PostAsync(url, content);
			response.EnsureSuccessStatusCode();

			return Result.Ok();

		}



    async Task<IEnumerable<UserRoleDetail>> updateRoleDetailsCache(UserRole role = UserRole.Unregistered)
    {
      var url = $"{Settings.PathToGetAllRoles}";
      url = url.Replace("{{Realm}}", Settings.KeycloakRealm);
      url = url.Replace("{{Ui-Client-Uuid}}", Settings.UiClientUuid);

      var token = await getAdminToken();
      _adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

			try
			{
				var response = await _adminClient.GetAsync(url);
				response.EnsureSuccessStatusCode();

				var json = await response.Content.ReadAsStringAsync();

				var roles = JsonSerializer.Deserialize<List<UserRoleDetail>>(json, _jsonOptions);

				foreach (var r in roles)
				{
					await _cache.Create(r.Name, r, TimeSpan.FromDays(1));
				}

				return roles;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to get roles details");
				throw;
			}
		}






		async Task<UserRoleDetail> getRoleDetailByName(UserRole roleType)
		{
			// Check the cache
			var exists = await _cache.Exists(roleType.ToString());

			if (exists.IsOk && exists.Data == true)
			{
				var cached = await _cache.Read<UserRoleDetail>(roleType.ToString());
				if (cached.IsOk && !String.IsNullOrEmpty(cached?.Data!.Id))
				{
					return cached.Data!;
				}
			}

			// Else, call Keycloak
			var allRoles = await updateRoleDetailsCache(roleType);
			var role = allRoles.FirstOrDefault(r => r.Name == roleType.ToString());
			// TODO: Null check

			return role;


		}


		async Task<string> getAdminToken()
    {
      // Check the cache for an unexpired token.
      var exists = await _cache.Exists("adminToken");

      if (exists.IsOk && exists.Data == true)
      {
        var cached = await _cache.Read<IdentityServerToken>("adminToken");
        if (cached.IsOk && !String.IsNullOrEmpty(cached.Data!.AccessToken))
        {
          return cached.Data.AccessToken;
        }
      }

      // Else, request a new token.

      var formData = new FormUrlEncodedContent(
        new Dictionary<string, string>
        {
          { "grant_type", "client_credentials" },
          { "client_id", Settings.ClientId },
          { "client_secret", Settings.ApiClientSecret }
        }
      );

      try
      {

        var response = await _adminClient.PostAsync(Settings.PathToAdminToken, formData);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<IdentityServerToken>(json);

        await _cache.Create("adminToken", result, TimeSpan.FromMinutes(result!.ExpiresInSeconds / 60));

        return result.AccessToken;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failed to get admin token");
        throw;
      }
    }


  }
}