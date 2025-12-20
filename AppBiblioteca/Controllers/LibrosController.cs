using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using AppBiblioteca.Models;

namespace AppBiblioteca.Controllers
{
    [Authorize]
    public class LibrosController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
