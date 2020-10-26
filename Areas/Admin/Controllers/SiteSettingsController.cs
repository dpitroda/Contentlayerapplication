using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ContentApp.Areas.Admin.Controllers
{   
    [Area("Admin")]
    public class SiteSettingsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
