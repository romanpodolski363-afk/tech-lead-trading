# Support Chat Back End

Backend for the support chat system: FIFO queue, team capacity, overflow, shifts, round-robin assignment (junior first), and inactivity monitor.

## Requirements

- .NET 8 SDK

## Build and test

```bash
dotnet restore
dotnet build
dotnet test
```

## Run API

```bash
cd src/SupportChat.Api
dotnet run
```

- **POST** `/api/chat/session` — Create and queue a chat. Returns `{ "status": "OK", "sessionId": "..." }` or `{ "status": "Refused", "sessionId": null }`.
- **POST** `/api/chat/session/{id}/poll` — Poll (call every 1s after OK). Returns status `Queued`, `Assigned`, or `Inactive`.
- **GET** `/api/chat/session/{id}` — Get session details.

## Design

- **Domain**: `Seniority`, `Agent`, `Team`, `ChatSession`; capacity = 10 × efficiency per agent; max queue = capacity × 1.5.
- **Teams**: A (08–16 UTC), B (16–24 UTC), C (00–08 UTC night), Overflow (office hours 08–24 UTC, 6 juniors).
- **Queue**: FIFO; main queue first, overflow when main full during office hours; refuse when both full.
- **Assignment**: Round robin preferring junior first (junior → mid → senior → team lead); only agents on shift get new chats.
- **Inactivity**: Session marked inactive if no poll for 3 seconds; assigned agent’s count is decremented.

## Testing (API running on http://localhost:5000 or http://127.0.0.1:5000)

### PowerShell

```powershell
# 1. Create a session (save the sessionId from the response)
Invoke-RestMethod -Method Post -Uri "http://127.0.0.1:5000/api/chat/session"

# 2. Poll a session (replace YOUR-SESSION-ID with the real GUID from step 1)
Invoke-RestMethod -Method Post -Uri "http://127.0.0.1:5000/api/chat/session/YOUR-SESSION-ID/poll"

# 3. Get session details
Invoke-RestMethod -Uri "http://127.0.0.1:5000/api/chat/session/YOUR-SESSION-ID"
```

### cURL (Windows PowerShell / Git Bash)

```bash
# 1. Create a session
curl -X POST http://127.0.0.1:5000/api/chat/session

# 2. Poll (replace YOUR-SESSION-ID with the sessionId from step 1)
curl -X POST http://127.0.0.1:5000/api/chat/session/YOUR-SESSION-ID/poll

# 3. Get session
curl http://127.0.0.1:5000/api/chat/session/YOUR-SESSION-ID
```

### Chrome DevTools Console (same origin: open http://127.0.0.1:5000/ first)

```javascript
// 1. Create session
fetch('/api/chat/session', { method: 'POST' }).then(r => r.json()).then(console.log);

// 2. Poll (use the sessionId from step 1, e.g. 77545c9f-fc91-4ca2-bd00-60a3ca7a7453)
fetch('/api/chat/session/77545c9f-fc91-4ca2-bd00-60a3ca7a7453/poll', { method: 'POST' }).then(r => r.json()).then(console.log);

// 3. Get session
fetch('/api/chat/session/77545c9f-fc91-4ca2-bd00-60a3ca7a7453').then(r => r.json()).then(console.log);
```

### What the Chrome screenshot showed

1. **Create session** — `POST /api/chat/session` returned `{ status: 'OK', sessionId: '77545c9f-fc91-4ca2-bd00-60a3ca7a7453' }`. So a new chat was accepted and queued.
2. **Poll with placeholder** — The URL had `PASTE-SESSION-ID-HERE` instead of the real ID. The server had no such session, so it returned **404**. The code then called `.json()` on that error response; the 404 body wasn’t valid JSON, so you saw **"Unexpected end of JSON input"**.
3. **Poll with real ID** — Using the actual `sessionId` from step 1, the poll returned `{ status: 'Assigned', assignedAgentId: 'B-J1' }`. So the session was assigned to agent **B-J1** (Team B, first junior). Always use the `sessionId` from the create response in the poll URL.