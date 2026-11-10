
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using DemirbasZimmet.API.Data;
using DemirbasZimmet.API.DTOs;
using DemirbasZimmet.API.Services.Interfaces;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace DemirbasZimmet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IParolaService _parolaService;
    private readonly IConfiguration _configuration;

    public AuthController(
        AppDbContext context,
        IParolaService parolaService,
        IConfiguration configuration)
    {
        _context = context;
        _parolaService = parolaService;
        _configuration = configuration;
    }

    [HttpPost("giris")]
    public async Task<IActionResult> Giris([FromBody] GirisDto dto)
    {
        string eposta = dto.Eposta.Trim();

        var kullanici = await _context.Kullanicilar
            .FirstOrDefaultAsync(x => x.Eposta == eposta);

        // Kullanıcı yoksa, pasifse veya parola yanlışsa giriş reddedilir.
        if (kullanici == null ||
            !kullanici.AktifMi ||
            !_parolaService.Dogrula(kullanici, dto.Parola))
        {
            return Unauthorized(new
            {
                mesaj = "E-posta veya parola hatalı."
            });
        }

        // JWT için kullanıcı bilgileri
        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                kullanici.KullaniciId.ToString()),

            new Claim(
                ClaimTypes.Name,
                kullanici.AdSoyad),

            new Claim(
                ClaimTypes.Email,
                kullanici.Eposta),

            new Claim(
                ClaimTypes.Role,
                kullanici.Rol)
        };

        // Gizli anahtar
        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "JWT anahtarı yapılandırılmamış.");

        var key = new SymmetricSecurityKey(
            Convert.FromBase64String(jwtKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        // Token süresi
        int expirationMinutes =
            _configuration.GetValue<int>("Jwt:ExpirationMinutes");

        var expirationDate = DateTime.UtcNow.AddMinutes(
            expirationMinutes > 0 ? expirationMinutes : 60);

        // Token oluşturma
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expirationDate,
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler()
            .WriteToken(token);

        return Ok(new
        {
            mesaj = "Giriş başarılı.",
            token = tokenString,
            sonKullanmaTarihi = expirationDate,
            kullanici = new
            {
                kullanici.KullaniciId,
                kullanici.AdSoyad,
                kullanici.Eposta,
                kullanici.Rol
            }
        });
    }
}
