using System.Security.Claims;
using CyberZone.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace CyberZone.Infrastructure.Realtime;

[Authorize]
public class NotificationHub : Hub
{
    private readonly UserManager<User> _userManager;

    public NotificationHub(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

            var user = await _userManager.FindByIdAsync(userId);
            if (user?.ManagedClubId is { } clubId && clubId != Guid.Empty)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"club-{clubId}");
            }
        }

        await base.OnConnectedAsync();
    }
}
