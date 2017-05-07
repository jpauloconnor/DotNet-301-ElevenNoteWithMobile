using ElevenNote.Web.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList.Mvc;
using PagedList;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using ElevenNote.Data;
using System.Net;

namespace ElevenNote.Web.Controllers
{

    public class AdminController : Controller
    {

        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;
       
        // Controllers
        // GET: /Admin/
        [Authorize(Roles = "Administrator")]
        #region public ActionResult Index(string searchStringUserNameOrEmail)
        public ActionResult Index(string searchStringUserNameOrEmail, string currentFilter, int? page)
        {
            try
            {
                int intPage = 1;
                int intPageSize = 5;
                int intTotalPageCount = 0;

                if (searchStringUserNameOrEmail != null)
                {
                    intPage = 1;
                }
                else
                {
                    if (currentFilter != null)
                    {
                        searchStringUserNameOrEmail = currentFilter;
                        intPage = page ?? 1;
                    }
                    else
                    {
                        searchStringUserNameOrEmail = "";
                        intPage = page ?? 1;
                    }
                }

                ViewBag.CurrentFilter = searchStringUserNameOrEmail;

                List<ExpandedUser> col_UserDTO = new List<ExpandedUser>();
                int intSkip = (intPage - 1) * intPageSize;

                intTotalPageCount = UserManager.Users
                    .Where(x => x.UserName.Contains(searchStringUserNameOrEmail))
                    .Count();

                var result = UserManager.Users
                    .Where(x => x.UserName.Contains(searchStringUserNameOrEmail))
                    .OrderBy(x => x.UserName)
                    .Skip(intSkip)
                    .Take(intPageSize)
                    .ToList();

                foreach (var item in result)
                {
                    ExpandedUser objUserDTO = new ExpandedUser();

                    objUserDTO.UserName = item.UserName;
                    objUserDTO.Email = item.Email;
                    objUserDTO.LockoutEndDateUtc = item.LockoutEndDateUtc;

                    col_UserDTO.Add(objUserDTO);
                }

                // Set the number of pages
                var _UserDTOAsIPagedList =
                    new StaticPagedList<ExpandedUser>
                    (
                        col_UserDTO, intPage, intPageSize, intTotalPageCount
                        );

                return View(_UserDTOAsIPagedList);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error: " + ex);
                List<ExpandedUser> col_UserDTO = new List<ExpandedUser>();

                return View(col_UserDTO.ToPagedList(1, 25));
            }
        }
        #endregion

        // GET: /Admin/Edit/Create 
        [Authorize(Roles = "Administrator")]
        #region public ActionResult Create()
        public ActionResult Create()
        {
            var expandedUser = new ExpandedUser();

            ViewBag.Roles = GetAllRolesAsSelectList();

            return View(expandedUser);
        }
        #endregion

        // POST: /Admin/Create
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        #region public ActionResult Create(ExpandedUser expUser)
        public ActionResult Create(ExpandedUser expUser)
        {
            try
            {
                if (expUser == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var Email = expUser.Email.Trim();
                var UserName = expUser.Email.Trim();
                var Password = expUser.Password.Trim();

                if (Email == "")
                {
                    throw new Exception("No Email");
                }

                if (Password == "")
                {
                    throw new Exception("No Password");
                }

                // UserName is LowerCase of the Email
                UserName = Email.ToLower();

                // Create user
                var objNewAdminUser = new ApplicationUser { UserName = UserName, Email = Email };
                var AdminUserCreateResult = UserManager.Create(objNewAdminUser, Password);

                if (AdminUserCreateResult.Succeeded == true)
                {
                    string strNewRole = Convert.ToString(Request.Form["Roles"]);

                    if (strNewRole != "0")
                    {
                        // Put user in role
                        UserManager.AddToRole(objNewAdminUser.Id, strNewRole);
                    }

                    return Redirect("~/Admin");
                }
                else
                {
                    ViewBag.Roles = GetAllRolesAsSelectList();
                    ModelState.AddModelError(string.Empty,
                        "Error: Failed to create the user. Check password requirements.");
                    return View(expUser);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Roles = GetAllRolesAsSelectList();
                ModelState.AddModelError(string.Empty, "Error: " + ex);
                return View("Create");
            }
        }
        #endregion


        // GET: /Admin/AddRole
        [Authorize(Roles = "Administrator")]
        #region public ActionResult AddRole()
        public ActionResult AddRole()
        {
            var role = new Role();

            return View(role);
        }
        #endregion

        // POST: /Admin/AddRole
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        #region public ActionResult AddRole(Role role)
        public ActionResult AddRole(Role role)
        {
            try
            {
                if (role == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var roleName = role.RoleName.Trim();

                if (roleName == "")
                {
                    throw new Exception("No RoleName");
                }

                // Create Role
                var roleManager =
                    new RoleManager<IdentityRole>(
                        new RoleStore<IdentityRole>(new ApplicationDbContext())
                        );

                if (!roleManager.RoleExists(roleName))
                {
                    roleManager.Create(new IdentityRole(roleName));
                }

                return Redirect("~/Admin/ViewAllRoles");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error: " + ex);
                return View("AddRole");
            }
        }
        #endregion

        // GET: /Admin/EditRoles
        [Authorize(Roles = "Administrator")]
        #region ActionResult EditRoles(string username)
        public ActionResult EditRoles(string username)
        {
            if (username == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            username = username.ToLower();
            
            ExpandedUser expUser = GetUser(username);

            if (expUser == null)
            {
                return HttpNotFound();
            }

            var userAndRoles = GetUserAndRoles(username);
            return View(userAndRoles);
        }
        #endregion

        // POST: /Admin/EditRoles
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        #region public ActionResult EditRoles(UserAndRoles userAndRoles)
        public ActionResult EditRoles(UserAndRoles userAndRoles)
        {
            try
            {
                if (userAndRoles == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                string UserName = userAndRoles.UserName;
                string strNewRole = Convert.ToString(Request.Form["AddRole"]);

                if (strNewRole != "No Roles Found")
                {
                    // Go get the User
                    ApplicationUser user = UserManager.FindByName(UserName);

                    // Put user in role
                    UserManager.AddToRole(user.Id, strNewRole);
                }

                ViewBag.AddRole = new SelectList(RolesUserIsNotIn(UserName));

                var userAndRolesObject = GetUserAndRoles(UserName);

                return View(userAndRolesObject);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error: " + ex);
                return View("EditRoles");
            }
        }
        #endregion

        // GET: /Admin/Edit/User 
        [Authorize(Roles = "Administrator")]
        #region public ActionResult EditUser(string username)
        public ActionResult EditUser(string username)
        {
            if (username == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExpandedUser expUser = GetUser(username);
            if (expUser == null)
            {
                return HttpNotFound();
            }
            return View(expUser);
        }
        #endregion

        // POST: /Admin/User
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        #region public ActionResult EditUser(ExpandedUser paramExpandedUserDTO)
        public ActionResult EditUser(ExpandedUser paramExpandedUserDTO)
        {
            try
            {
                if (paramExpandedUserDTO == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                ExpandedUser objExpandedUserDTO = UpdateUser(paramExpandedUserDTO);

                if (objExpandedUserDTO == null)
                {
                    return HttpNotFound();
                }

                return Redirect("~/Admin");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error: " + ex);
                return View("EditUser", GetUser(paramExpandedUserDTO.UserName));
            }
        }
        #endregion




        //Utility
        #region public ApplicationUserManager UserManager
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ??
                    HttpContext.GetOwinContext()
                    .GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        #endregion
        #region public ApplicationRoleManager RoleManager
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ??
                    HttpContext.GetOwinContext()
                    .GetUserManager<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }
        #endregion

        // GET: /Admin/ViewAllRoles
        [Authorize(Roles = "Administrator")]
        #region public ActionResult ViewAllRoles()
        public ActionResult ViewAllRoles()
        {
            var roleManager =
                new RoleManager<IdentityRole>
                (
                    new RoleStore<IdentityRole>(new ApplicationDbContext())
                    );

            List<Role> colRole = (from objRole in roleManager.Roles
                                  select new Role
                                  {
                                      Id = objRole.Id,
                                      RoleName = objRole.Name
                                  }).ToList();

            return View(colRole);
        }

        #endregion

        #region private List<SelectListItem> GetAllRolesAsSelectList()
        private List<SelectListItem> GetAllRolesAsSelectList()
        {
            List<SelectListItem> RoleList =
                new List<SelectListItem>();

            var roleManager =
                new RoleManager<IdentityRole>(
                    new RoleStore<IdentityRole>(new ApplicationDbContext()));

            var roleList = roleManager.Roles.OrderBy(x => x.Name).ToList();

            RoleList.Add(
                new SelectListItem
                {
                    Text = "Select",
                    Value = "0"
                });

            foreach (var item in roleList)
            {
                RoleList.Add(
                    new SelectListItem
                    {
                        Text = item.Name.ToString(),
                        Value = item.Name.ToString()
                    });
            }
            return RoleList;
        }
        #endregion

        #region private ExpandedUser GetUser(string username)
        private ExpandedUser GetUser(string username)
        {
            var expUser = new ExpandedUser();

            var result = UserManager.FindByName(username);

            // If we could not find the user, throw an exception
            if (result == null) throw new Exception("Could not find the User");

            expUser.UserName = result.UserName;
            expUser.Email = result.Email;
            expUser.LockoutEndDateUtc = result.LockoutEndDateUtc;
            expUser.AccessFailedCount = result.AccessFailedCount;
            expUser.PhoneNumber = result.PhoneNumber;

            return expUser;
        }
        #endregion
    
        #region private UserAndRoles GetUserAndRoles(string username)
        private UserAndRoles GetUserAndRoles(string username)
        {
            // Go get the User
            ApplicationUser user = UserManager.FindByName(username);
            List<UserRole> userRole =
                (from role in UserManager.GetRoles(user.Id)
                 select new UserRole
                 {
                     RoleName = role,
                     UserName = username
                 }).ToList();

            if (userRole.Count() == 0)
            {
                userRole.Add(new UserRole { RoleName = "No Roles Found" });
            }

            ViewBag.AddRole = new SelectList(RolesUserIsNotIn(username));

            // Create UserRolesAndPermissions
            var userAndRoles = new UserAndRoles();
            userAndRoles.UserName = username;
            userAndRoles.UserRole = userRole;
            return userAndRoles;
        }
        #endregion
      
        #region private List<string> RolesUserIsNotIn(string username)
        private List<string> RolesUserIsNotIn(string username)
        {        
            var allRoles = RoleManager.Roles.Select(x => x.Name).ToList();

            // Go get the roles for an individual
            var user = UserManager.FindByName(username);

            if (user == null)
            {
                throw new Exception("Could not find the User");
            }

            var rolesForUser = UserManager.GetRoles(user.Id).ToList();
            var rolesUserNotIn = (from role in allRoles
                                       where !rolesForUser.Contains(role)
                                       select role).ToList();

            if (rolesUserNotIn.Count() == 0)
            {
                rolesUserNotIn.Add("No Roles Found");
            }
            return rolesUserNotIn;
        }
        #endregion

        #region private ExpandedUser UpdateUser(ExpandedUserDTO exdUser)
        private ExpandedUser UpdateDTOUser(ExpandedUser expUser)
        {
            ApplicationUser result =
                UserManager.FindByName(expUser.UserName);

            // If we could not find the user, throw an exception
            if (result == null)
            {
                throw new Exception("Could not find the User");
            }

            result.Email = expUser.Email;

            // Lets check if the account needs to be unlocked
            if (UserManager.IsLockedOut(result.Id))
            {
                // Unlock user
                UserManager.ResetAccessFailedCountAsync(result.Id);
            }

            UserManager.Update(result);

            // Was a password sent across?
            if (!string.IsNullOrEmpty(expUser.Password))
            {
                // Remove current password
                var removePassword = UserManager.RemovePassword(result.Id);
                if (removePassword.Succeeded)
                {
                    // Add new password
                    var AddPassword =
                        UserManager.AddPassword(
                            result.Id,
                            expUser.Password
                            );

                    if (AddPassword.Errors.Count() > 0)
                    {
                        throw new Exception(AddPassword.Errors.FirstOrDefault());
                    }
                }
            }

            return expUser;
        }
        #endregion

    }
}