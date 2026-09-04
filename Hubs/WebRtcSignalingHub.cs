using Microsoft.AspNetCore.SignalR;

namespace MiniCSKH.Hubs;

/// <summary>Kênh trao đổi tín hiệu WebRTC (SDP offer/answer + ICE candidate) để 2 trình duyệt kết nối
/// trực tiếp peer-to-peer, không qua trung gian điện thoại — dùng cho cuộc gọi nội bộ agent↔agent
/// hoặc agent↔khách (khách mở link web), KHÔNG gọi được tới số điện thoại thật (khác VoipToken/StringeeX).</summary>
public class WebRtcSignalingHub : Hub
{
    public async Task JoinRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        await Clients.OthersInGroup(roomId).SendAsync("PeerJoined", Context.ConnectionId);
    }

    public async Task LeaveRoom(string roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        await Clients.OthersInGroup(roomId).SendAsync("PeerLeft", Context.ConnectionId);
    }

    public async Task SendOffer(string roomId, string sdp) =>
        await Clients.OthersInGroup(roomId).SendAsync("ReceiveOffer", Context.ConnectionId, sdp);

    public async Task SendAnswer(string roomId, string sdp) =>
        await Clients.OthersInGroup(roomId).SendAsync("ReceiveAnswer", Context.ConnectionId, sdp);

    public async Task SendIceCandidate(string roomId, string candidate) =>
        await Clients.OthersInGroup(roomId).SendAsync("ReceiveIceCandidate", Context.ConnectionId, candidate);
}
