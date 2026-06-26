using System;

namespace SBS.Application.Common.ManagerExceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"Không tìm thấy '{name}' với id = {key}.")
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }
}
