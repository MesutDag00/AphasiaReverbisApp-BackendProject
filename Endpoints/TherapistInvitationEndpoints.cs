using Microsoft.EntityFrameworkCore;
using AphaisaReverbes.Contracts;
using AphaisaReverbes.Data;
using AphaisaReverbes.Models;
using AphaisaReverbes.Services;

namespace AphaisaReverbes.Endpoints;

// Minimal admin endpoint to create therapist invitations (for testing/ops)
internal static class TherapistInvitationEndpoints
{
    public static RouteGroupBuilder MapTherapistInvitationEndpoints(this IEndpointRouteBuilder app)
    {
        // This is mapped under /api in ApiEndpoints.
        var group = app.MapGroup("/therapist-invitations").WithTags("TherapistInvitations");
        group.MapPost("/", CreateTherapistInvitation);
        return group;
    }

    private static async Task<IResult> CreateTherapistInvitation(AppDbContext db)
    {
        // 6 haneli güvenli kod
        for (var attempt = 0; attempt < 40; attempt++)
        {
            var code = InvitationCodeGenerator.Generate();
            var now = DateTimeOffset.UtcNow;

            var entity = new TherapistInvitation
            {
                Id = Guid.NewGuid(),
                Code = code,
                CreatedAtUtc = now
            };

            db.TherapistInvitations.Add(entity);

            try
            {
                await db.SaveChangesAsync();
                return EndpointSupport.Created(
                    $"/api/therapist-invitations/{entity.Id}",
                    new CreateTherapistInvitationResponse(entity.Code, entity.CreatedAtUtc)
                );
            }
            catch (DbUpdateException)
            {
                db.ChangeTracker.Clear();
            }
        }

        return EndpointSupport.Fail(StatusCodes.Status500InternalServerError, "Davet kodu üretilemedi. Lütfen tekrar deneyin.");
    }
}

