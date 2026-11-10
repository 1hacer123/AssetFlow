
using DemirbasZimmet.API.Models;
using DemirbasZimmet.API.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace DemirbasZimmet.API.Services.Implementations;

public class ParolaService : IParolaService
{
    private readonly IPasswordHasher<Kullanici> _passwordHasher;

    public ParolaService(IPasswordHasher<Kullanici> passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    public string Hashle(Kullanici kullanici, string parola)
    {
        return _passwordHasher.HashPassword(kullanici, parola);
    }

    public bool Dogrula(Kullanici kullanici, string parola)
    {
        var sonuc = _passwordHasher.VerifyHashedPassword(
            kullanici,
            kullanici.ParolaHash,
            parola
        );

        return sonuc == PasswordVerificationResult.Success ||
               sonuc == PasswordVerificationResult.SuccessRehashNeeded;
    }
}
