using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ElevenNote.Web.Models
{

    public class UserRoles
    {

        [Display(Name = "Role Name")]
        public string RoleName { get; set; }
    }


    public class UserRole
    {

        [Display(Name = "User Name")]
        public string UserName { get; set; }
        [Display(Name = "Role Name")]
        public string RoleName { get; set; }
    }


    public class ExpandedUser
    {
       
        [Display(Name = "User Name")]
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        [Display(Name = "Lockout End Date Utc")]
        public DateTime? LockoutEndDateUtc { get; set; }
        public int AccessFailedCount { get; set; }
        public string PhoneNumber { get; set; }
        public IEnumerable<UserRoles> Roles { get; set; }
    }

    public class Role
    {

        public string Id { get; set; }
        [Display(Name = "Role Name")]
        public string RoleName { get; set; }
    }

    public class UserAndRoles
    {

        [Display(Name = "User Name")]
        public string UserName { get; set; }
        public List<UserRole> UserRole { get; set; }
    }
}
