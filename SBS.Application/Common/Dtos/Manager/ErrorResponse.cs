using System.Collections.Generic;

namespace SBS.Application.Common.Dtos.Manager;

public class ErrorResponse
{
    public string Message { get; set; } = null!;
    public List<string>? Errors { get; set; }
}
