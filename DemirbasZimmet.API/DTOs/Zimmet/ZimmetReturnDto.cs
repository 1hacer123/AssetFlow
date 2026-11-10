using System.ComponentModel.DataAnnotations;
using DemirbasZimmet.API.Models;

namespace DemirbasZimmet.API.DTOs.Zimmet;

public class ZimmetReturnDto
{
    [EnumDataType(typeof(DemirbasDurum))]
    public DemirbasDurum IadeSonrasiDurum { get; set; }
}