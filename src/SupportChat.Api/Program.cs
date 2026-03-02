using SupportChat.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITimeProvider, SupportChat.Api.Services.TimeProvider>();
builder.Services.AddSingleton<ITeamRegistry, TeamRegistry>();
builder.Services.AddSingleton<IChatQueueService, ChatQueueService>();
builder.Services.AddSingleton<IAssignmentService, AssignmentService>();
builder.Services.AddSingleton<IInactivityMonitor, InactivityMonitor>();

builder.Services.AddControllers();
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

var rootResponse = new
{
    service = "Support Chat API",
    endpoints = new[]
    {
        "POST /api/chat/session - create and queue a session",
        "POST /api/chat/session/{id}/poll - poll (every 1s)",
        "GET  /api/chat/session/{id} - get session"
    }
};

app.MapGet("/", () => TypedResults.Ok(rootResponse));

app.MapControllers();

app.Run();
