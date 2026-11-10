
using DemirbasZimmet.API.Models;

namespace DemirbasZimmet.API.Services.Interfaces;

public interface IParolaService
{
    string Hashle(Kullanici kullanici, string parola);

    bool Dogrula(Kullanici kullanici, string parola);
}
