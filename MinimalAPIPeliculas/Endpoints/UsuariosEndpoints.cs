using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MinimalAPIPeliculas.DTOs;
using MinimalAPIPeliculas.Filtros;
using MinimalAPIPeliculas.Servicios;
using MinimalAPIPeliculas.Utilidades;

namespace MinimalAPIPeliculas.Endpoints
{
    public static class UsuariosEndpoints
    {
        public static RouteGroupBuilder MapUsuarios(this RouteGroupBuilder group)
        {
            group.MapPost("/registrar", Registrar)
                .AddEndpointFilter<FiltroValidaciones<CredencialesUsuarioDto>>();

            group.MapPost("/login", Login)
                .AddEndpointFilter<FiltroValidaciones<CredencialesUsuarioDto>>();

            group.MapPost("/haceradmin", HacerAdmin)
                .AddEndpointFilter<FiltroValidaciones<EditarClaimDto>>()
                .RequireAuthorization("esadmin");

            group.MapPost("/removeradmin", RemoverAdmin)
                .AddEndpointFilter<FiltroValidaciones<EditarClaimDto>>()
                .RequireAuthorization("esadmin");

            group.MapGet("/renovarToken", RenovarToken)
                .RequireAuthorization();

            return group;
        }

        static async Task<Results<Ok<RespuestaAutenticacionDto>, BadRequest<IEnumerable<IdentityError>>>> Registrar(CredencialesUsuarioDto credencialesUsuarioDto,
                [FromServices] UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            var usuario = new IdentityUser
            {
                UserName = credencialesUsuarioDto.Email,
                Email = credencialesUsuarioDto.Email
            };

            var resultado = await userManager.CreateAsync(usuario, credencialesUsuarioDto.Password);

            if (resultado.Succeeded)
            {
                var credencialesRespuesta =
                    await ConstruirToken(credencialesUsuarioDto, configuration, userManager);
                return TypedResults.Ok(credencialesRespuesta);
            }
            else
            {
                return TypedResults.BadRequest(resultado.Errors);
            }
        }

        static async Task<Results<Ok<RespuestaAutenticacionDto>, BadRequest<string>>> Login(CredencialesUsuarioDto credencialesUsuarioDto, [FromServices] SignInManager<IdentityUser> signInManager,
            [FromServices] UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            var usuario = await userManager.FindByEmailAsync(credencialesUsuarioDto.Email);

            if (usuario is null)
            {
                return TypedResults.BadRequest("Login incorrecto");
            }

            var resultado = await signInManager.CheckPasswordSignInAsync(usuario,
                credencialesUsuarioDto.Password, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                var respuestaAutenticacion =
                    await ConstruirToken(credencialesUsuarioDto, configuration, userManager);
                return TypedResults.Ok(respuestaAutenticacion);
            }
            else
            {
                return TypedResults.BadRequest("Login incorrecto");
            }
        }

        static async Task<Results<NoContent, NotFound>> HacerAdmin(EditarClaimDto editarClaimDto,
            [FromServices] UserManager<IdentityUser> userManager)
        {
            var usuario = await userManager.FindByEmailAsync(editarClaimDto.Email);
            if (usuario is null)
            {
                return TypedResults.NotFound();
            }

            await userManager.AddClaimAsync(usuario, new Claim("esadmin", "true"));
            return TypedResults.NoContent();
        }

        static async Task<Results<NoContent, NotFound>> RemoverAdmin(EditarClaimDto editarClaimDto,
            [FromServices] UserManager<IdentityUser> userManager)
        {
            var usuario = await userManager.FindByEmailAsync(editarClaimDto.Email);
            if (usuario is null)
            {
                return TypedResults.NotFound();
            }

            await userManager.RemoveClaimAsync(usuario, new Claim("esadmin", "true"));
            return TypedResults.NoContent();
        }

        public static async Task<Results<Ok<RespuestaAutenticacionDto>, NotFound>> RenovarToken(
            IServicioUsuarios servicioUsuarios, IConfiguration configuration,
            [FromServices] UserManager<IdentityUser> userManager)
        {
            var usuario = await servicioUsuarios.ObtenerUsuario();

            if (usuario is null)
            {
                return TypedResults.NotFound();
            }

            var credencialesUsuarioDto = new CredencialesUsuarioDto { Email = usuario.Email! };

            var respuestaAutenticacionDto = await ConstruirToken(credencialesUsuarioDto, configuration,
                userManager);

            return TypedResults.Ok(respuestaAutenticacionDto);
        }

        private static async Task<RespuestaAutenticacionDto> ConstruirToken(CredencialesUsuarioDto credencialesUsuarioDto,
                IConfiguration configuration, UserManager<IdentityUser> userManager)
        {
            var claims = new List<Claim>
            {
                new("email", credencialesUsuarioDto.Email),
                new("lo que yo quiera", "cualquier otro valor")
            };

            var usuario = await userManager.FindByNameAsync(credencialesUsuarioDto.Email);
            var claimsDb = await userManager.GetClaimsAsync(usuario!);

            claims.AddRange(claimsDb);

            var llave = Llaves.ObtenerLlave(configuration);
            var creds = new SigningCredentials(llave.First(), SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddYears(1);

            var tokenDeSeguridad = new JwtSecurityToken(issuer: null, audience: null, claims: claims,
                expires: expiracion, signingCredentials: creds);

            var token = new JwtSecurityTokenHandler().WriteToken(tokenDeSeguridad);

            return new RespuestaAutenticacionDto
            {
                Token = token,
                Expiracion = expiracion
            };
        }
    }
}
