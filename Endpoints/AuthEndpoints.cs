using Microsoft.EntityFrameworkCore;
using AphaisaReverbes.Contracts;
using AphaisaReverbes.Data;
using AphaisaReverbes.Services;

namespace AphaisaReverbes.Endpoints;

internal static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");
        group.MapPost("/login", Login);
        return group;
    }

    private static async Task<IResult> Login(LoginRequest request, AppDbContext db, JwtTokenService tokens, CancellationToken ct)
    {
        if (!EndpointSupport.TryTrimRequired(request.Email, 320, "email", out var email, out var err))
            return err!;
        if (!EndpointSupport.TryTrimRequired(request.Password, 200, "password", out var password, out err))
            return err!;

        email = email.ToLowerInvariant();

        var therapist = await db.Therapists
            .AsNoTracking()
            .Where(x => x.Email == email)
            .Select(x => new { x.Id, x.Email, x.PasswordHash, x.FirstName, x.LastName })
            .SingleOrDefaultAsync(ct);

        if (therapist is not null)
        {
            if (!PasswordService.Verify(password, therapist.PasswordHash))
                return EndpointSupport.BadRequest("Email veya şifre hatalı.");

            var token = tokens.CreateToken(therapist.Id, therapist.Email, "Therapist");
            return EndpointSupport.Ok(new LoginResponse(
                token,
                "Therapist",
                therapist.Id,
                therapist.Email,
                therapist.FirstName,
                therapist.LastName
            ));
        }

        var patient = await db.Patients
            .AsNoTracking()
            .Where(x => x.Email == email)
            .Select(x => new { x.Id, x.Email, x.PasswordHash, x.FirstName, x.LastName })
            .SingleOrDefaultAsync(ct);

        if (patient is null)
            return EndpointSupport.BadRequest("Email veya şifre hatalı.");

        if (!PasswordService.Verify(password, patient.PasswordHash))
            return EndpointSupport.BadRequest("Email veya şifre hatalı.");

        var patientToken = tokens.CreateToken(patient.Id, patient.Email, "Patient");
        return EndpointSupport.Ok(new LoginResponse(
            patientToken,
            "Patient",
            patient.Id,
            patient.Email,
            patient.FirstName,
            patient.LastName
        ));
    }
}

