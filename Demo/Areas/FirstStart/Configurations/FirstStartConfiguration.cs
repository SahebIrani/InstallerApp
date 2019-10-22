using System.ComponentModel.DataAnnotations;

namespace Demo.Areas.FirstStart.Configurations
{
    public class FirstStartConfiguration
    {
        [Required] public string AdminUserName { get; set; }

        [Required, EmailAddress] public string AdminEmail { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6)]
        public string AdminPassword { get; set; }

        [Required] public string Database { get; set; }

        public string ConnectionString { get; set; }

        public bool InitializeFakeData { get; set; }
    }
}
