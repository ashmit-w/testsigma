using Puppet.Core;

namespace Puppet.Host;

/// <summary>The four /session/* routes. One AppSession per host instance, held as a singleton.</summary>
public static class SessionEndpoints
{
    public static void MapSessionEndpoints(this WebApplication app)
    {
        app.MapPost("/session/start", async (StartRequest request, AppSession session) =>
        {
            try
            {
                var model = await session.StartAsync(request.ExePath);
                return Results.Ok(await BuildState(model, session));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        app.MapPost("/session/reset", async (AppSession session) =>
        {
            try
            {
                var model = await session.ResetAsync();
                return Results.Ok(await BuildState(model, session));
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        app.MapGet("/session/state", async (AppSession session) =>
        {
            try
            {
                var model = await session.CurrentAsync();
                return Results.Ok(await BuildState(model, session));
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        });

        app.MapPost("/session/run", async (RunRequest request, AppSession session) =>
        {
            Flow flow;
            try
            {
                flow = new Flow { Steps = [.. request.Steps.Select(FlowStepMapper.Map)] };
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }

            try
            {
                var replay = await session.ReplayAsync(flow);
                var palette = BlockGenerator.Generate(replay.Model);
                return Results.Ok(new RunResponse
                {
                    StepResults = [.. replay.StepResults.Select(StepResultResponse.From)],
                    Palette = palette,
                    Coverage = palette.Coverage,
                    ProcessId = await session.CurrentProcessIdAsync(),
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        });
    }

    private static async Task<SessionStateResponse> BuildState(ModelDocument model, AppSession session)
    {
        var palette = BlockGenerator.Generate(model);
        return new SessionStateResponse
        {
            Palette = palette,
            Coverage = palette.Coverage,
            ProcessId = await session.CurrentProcessIdAsync(),
        };
    }
}
