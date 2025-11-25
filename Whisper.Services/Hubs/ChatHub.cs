using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Whisper.Services.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {

    }
}
