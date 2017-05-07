using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ElevenNote.Web.Controllers
{

    public class AdminController : Controller
    {

        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;
        // Controllers
        // GET: /Admin/
        [Authorize(Roles = "Administrator")]
        public ActionResult Index()
        {
            return View();
        }
    }
}