using System.ComponentModel.DataAnnotations;

namespace Demo.Configurations
{
    public class FirstStartConfiguration
    {
        public string InstallerAppName { get; set; }

        [Required]
        public string AdminUserName { get; set; }

        [Required]
        public string AdminDisplayName { get; set; }

        [Required, EmailAddress]
        public string AdminEmail { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6)]
        public string AdminPassword { get; set; }

        [Required]
        public string Database { get; set; }

        public string ConnectionString { get; set; }
    }
}
