using System.Collections.Generic;

namespace SBS.Application.Common.Dtos.Profile;

public class ResultDto
{
    public bool Succeeded { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ResultDto Success() => new() { Succeeded = true };

    public static ResultDto Failure(IEnumerable<string> errors) => new() 
    { 
        Succeeded = false, 
        Errors = new List<string>(errors) 
    };
}
