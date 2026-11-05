namespace HRIStwinbeeAPI.Models.Security
{
    public sealed class SecurityUserDto
    {
        public string UserId { get; set; } = string.Empty;

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? FullName { get; set; }

        public string? Designation { get; set; }

        public string? Administrator { get; set; }

        public string? Branch { get; set; }

        public string? Stats { get; set; }

        public string? Dept { get; set; }

        public string? Entity { get; set; }

        public string? UserRights { get; set; }
    }


    public sealed class SecurityModuleDto
    {
        public string ModuleCode { get; set; } = string.Empty;

        public string ModuleName { get; set; } = string.Empty;

        public string? RoutePath { get; set; }

        public string? ParentModuleCode { get; set; }

        public bool CanView { get; set; }

        public bool CanOpen { get; set; }

        public int SortOrder { get; set; }

        public bool IsMenu { get; set; }
    }


    public sealed class SaveSecurityModuleRequest
    {
        public string ModuleCode { get; set; } = string.Empty;

        public bool CanView { get; set; }

        public bool CanOpen { get; set; }
    }


    public sealed class SaveSecurityUserRequest
    {
        public string UserId { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string? LastName { get; set; }

        public string? Password { get; set; }

        public string? Designation { get; set; }

        public string Administrator { get; set; } = "N";

        public string? Branch { get; set; }

        public string Stats { get; set; } = "A";

        public string? Dept { get; set; }

        public string? Entity { get; set; }

        public string? UserRights { get; set; }

        public List<SaveSecurityModuleRequest> Modules { get; set; } = new();
    }


    public sealed class SecurityUserDetailDto
    {
        public SecurityUserDto User { get; set; } = new();

        public List<SecurityModuleDto> Modules { get; set; } = new();
    }
}