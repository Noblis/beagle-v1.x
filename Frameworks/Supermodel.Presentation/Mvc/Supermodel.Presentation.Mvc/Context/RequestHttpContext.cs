using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Supermodel.Presentation.Mvc.Context;

public static class RequestHttpContext
{
    #region Methods & Method-like Properties
    public static void Configure(Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    public static Microsoft.AspNetCore.Http.HttpContext Current => _httpContextAccessor.HttpContext!;

    public static long? CurrentUserId
    {
        get 
        { 
            var userNameIdentifierClaim = Current.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userNameIdentifierClaim == null) return null;
            return long.Parse(userNameIdentifierClaim.Value); 
        }
    }
    public static string? CurrentUserLabel
    {
        get 
        { 
            var userNameClaim = Current.User.FindFirst(ClaimTypes.Name);
            if (userNameClaim == null) return null;
            return userNameClaim.Value; 
        }
    }
    public static IEnumerable<Claim> CurrentUserClaims => ((ClaimsIdentity)Current.User.Identity!).Claims;
    public static bool CurrentUserHasClaim(Predicate<Claim> match) => ((ClaimsIdentity)Current.User.Identity!).HasClaim(match);
    public static bool IsCurrentUserInRole(string role) => CurrentUserHasClaim(x => x.Type == ClaimTypes.Role && x.Value == role);
    public static ClaimsPrincipal CurrentUser => Current.User;

    public static IUrlHelper GetUrlHelperWithEmptyViewContext()
    {
        var urlHelperFactory = Current.RequestServices.GetRequiredService<IUrlHelperFactory>();
        var urlHelper = urlHelperFactory.GetUrlHelper(new ActionContext(Current, new RouteData(), new ActionDescriptor())); 
        return urlHelper;
    }
    #endregion

    #region Fields
    private static Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor = null!;
    #endregion
}