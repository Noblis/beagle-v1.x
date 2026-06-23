using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Claims;

namespace Supermodel.Presentation.WebMonk.Auth;

public static class AuthClaimsHelper
{
    public static List<Claim> CreateNewClaimsListWithIdAndLabel(long id, string label)
    {
        return
        [
            new(ClaimTypes.NameIdentifier, id.ToString(), ClaimValueTypes.Integer64),
            new(ClaimTypes.Name, label, ClaimValueTypes.String)
        ];
    }

    public static bool IsInRole(this ImmutableList<Claim> me, string role)
    {
        //return me.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).Any(x => x == role);
        return me.Any(x => x.Type == ClaimTypes.Role && x.Value == role);
    }
}