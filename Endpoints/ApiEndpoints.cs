namespace AphaisaReverbes.Endpoints;

internal static class ApiEndpoints
{
    public static void MapApi(this WebApplication app)
    {
        // Root API group: keep common prefix in one place.
        var api = app.MapGroup("/api");

        api.MapTherapistEndpoints();
        api.MapTherapistInvitationEndpoints();
        api.MapPatientEndpoints();
    }
}

