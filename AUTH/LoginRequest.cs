namespace twbERRANDSapi.AUTH
{
    public sealed class LoginRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public sealed class UserPermissionDto
    {
        public string PermissionKey { get; set; } = string.Empty;
        public string ModuleName { get; set; } = string.Empty;
        public string? RoutePath { get; set; }
        public string? ParentPermissionKey { get; set; }
        public bool CanView { get; set; }
        public bool CanOpen { get; set; }
        public int SortOrder { get; set; }
        public bool IsMenu { get; set; }
    }

    //UPDATE 04-05-2026
    public sealed class AuthUserDto
    {
        public string UserId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string Dept { get; set; } = string.Empty;
        public string UserRights { get; set; } = string.Empty;
        public string Administrator { get; set; } = string.Empty;
        public string Stats { get; set; } = string.Empty;
        public string Entity { get; set; } = string.Empty;
        public List<UserPermissionDto> Permissions { get; set; } = new();
    }
    // End of the Update

    public sealed class LoginUserDto
    {
        public string UserId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Designation { get; set; }
        public string Branch { get; set; } = string.Empty;
        public string? Dept { get; set; }
        public string? Userights { get; set; }
        public string Administrator { get; set; } = "N";
        public string? Stats { get; set; }
    }
    public sealed class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AuthUserDto? User { get; set; }
    }
}
