using System.Collections.Generic;

namespace SBS.Application.Common.Dtos.Auth;

public class AuthResultDto
{
    public bool Succeeded { get; set; }
    public List<string> Errors { get; set; } = new();
    public AuthResponseDto? Data { get; set; }

    public static AuthResultDto Success(AuthResponseDto data) => new() { Succeeded = true, Data = data };

    public static AuthResultDto Failure(IEnumerable<string> errors) => new() 
    { 
        Succeeded = false, 
        Errors = new List<string>(errors) 
    };
}
