using System.Collections.Concurrent;
using SupportChat.Api.Domain;

namespace SupportChat.Api.Services;

/// <summary>
/// FIFO queue for chat sessions. Main queue (all teams on shift) + overflow queue when main is full during office hours.
/// Refuses when queue full (and no overflow capacity during office hours).
/// </summary>
public interface IChatQueueService
{
    (bool Accepted, Guid? SessionId) TryEnqueue();
    ChatSession? GetSession(Guid sessionId);
    IReadOnlyList<ChatSession> GetQueuedSessions();
    IReadOnlyList<ChatSession> GetAllSessions();
    bool TryPeek(out ChatSession? session);
    bool TryDequeue(out ChatSession? session);
}

public class ChatQueueService : IChatQueueService
{
    private readonly ITimeProvider _time;
    private readonly ITeamRegistry _registry;
    private readonly ConcurrentQueue<ChatSession> _mainQueue = new();
    private readonly ConcurrentQueue<ChatSession> _overflowQueue = new();
    private readonly ConcurrentDictionary<Guid, ChatSession> _sessions = new();

    public ChatQueueService(ITimeProvider time, ITeamRegistry registry)
    {
        _time = time;
        _registry = registry;
    }

    public (bool Accepted, Guid? SessionId) TryEnqueue()
    {
        var now = _time.UtcNow;
        var mainMax = _registry.GetMainPoolMaxQueueLength(now);
        var overflowMax = _registry.GetOverflowPoolMaxQueueLength(now);
        var mainQueueCount = _mainQueue.Count;
        var overflowQueueCount = _overflowQueue.Count;

        if (mainMax > 0 && mainQueueCount < mainMax)
        {
            var session = new ChatSession(Guid.NewGuid());
            _sessions[session.Id] = session;
            _mainQueue.Enqueue(session);
            return (true, session.Id);
        }

        if (overflowMax > 0 && overflowQueueCount < overflowMax)
        {
            var session = new ChatSession(Guid.NewGuid());
            _sessions[session.Id] = session;
            _overflowQueue.Enqueue(session);
            return (true, session.Id);
        }

        return (false, null);
    }

    public ChatSession? GetSession(Guid sessionId) =>
        _sessions.TryGetValue(sessionId, out var s) ? s : null;

    public IReadOnlyList<ChatSession> GetQueuedSessions()
    {
        var list = new List<ChatSession>();
        list.AddRange(_mainQueue);
        list.AddRange(_overflowQueue);
        return list;
    }

    public IReadOnlyList<ChatSession> GetAllSessions() => _sessions.Values.ToList();

    public bool TryPeek(out ChatSession? session)
    {
        if (_mainQueue.TryPeek(out session))
            return true;
        if (_overflowQueue.TryPeek(out session))
            return true;
        session = null;
        return false;
    }

    public bool TryDequeue(out ChatSession? session)
    {
        if (_mainQueue.TryDequeue(out session))
            return true;
        if (_overflowQueue.TryDequeue(out session))
            return true;
        session = null;
        return false;
    }

}
