using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BankAccounts.Models;

namespace BankAccounts.Controllers
{
    public class HomeController : Controller
    {
        private BankContext dbContext;
        public HomeController(BankContext context)
        {
            dbContext = context;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            int? loggedID = HttpContext.Session.GetInt32("LoggedId");
            if (loggedID == null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Success");
            }
        }

        [HttpPost("register")]
        public IActionResult Register(User newUser)
        {
            if(ModelState.IsValid)
            {
                if(dbContext.Users.Any(u => u.Email == newUser.Email))
                {
                    ModelState.AddModelError("Email", "Email already registered!");
                    return View("Index");
                }
                else
                {
                    PasswordHasher<User> hasher = new PasswordHasher<User>();
                    newUser.Password = hasher.HashPassword(newUser, newUser.Password);
                    dbContext.Users.Add(newUser);
                    dbContext.SaveChanges();
                    User justMade = dbContext.Users.FirstOrDefault(u => u.Email == newUser.Email);
                    HttpContext.Session.SetInt32("LoggedId", justMade.UserId);
                    HttpContext.Session.SetString("LoggedName", justMade.FirstName);
                    return RedirectToAction("Account", justMade.UserId);
                }
            }
            else
            {
                return View ("Index");
            }
        }

        [HttpGet("login")]
        public IActionResult LoginForm()
        {
            int? loggedID = HttpContext.Session.GetInt32("LoggedId");
            if (loggedID == null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Account", new {userId = (int)loggedID});
            }
        }

        [HttpPost("login")]
        public IActionResult Login(UserLogin userLogin)
        {
            if (ModelState.IsValid)
            {
                User userInDb = dbContext.Users.FirstOrDefault(u => u.Email == userLogin.Email);
                if (userInDb == null)
                {
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("LoginForm");
                }
                else
                {
                    PasswordHasher<UserLogin> hasher = new PasswordHasher<UserLogin>();
                    PasswordVerificationResult check = hasher.VerifyHashedPassword(userLogin, userInDb.Password, userLogin.Password);
                    if (check == 0)
                    {
                        ModelState.AddModelError("Email", "Invalid Email/Password");
                        return View("LoginForm");
                    }
                    else
                    {
                        HttpContext.Session.SetInt32("LoggedId", userInDb.UserId);
                        HttpContext.Session.SetString("LoggedName", userInDb.FirstName);
                        return RedirectToAction("Account", new {userId = userInDb.UserId});
                    }
                }
            }
            else
            {
                return View("LoginForm");
            }
        }

        [HttpGet("account/{userId}")]
        public IActionResult Account(int userId)
        {
            int? loggedID = HttpContext.Session.GetInt32("LoggedId");
            if (loggedID != userId)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.LoggedName = HttpContext.Session.GetString("LoggedName");
                ViewBag.LoggedID = (int)loggedID;
                List<Transaction> transactions = dbContext.Transactions
                    .Include(t => t.Owner)
                    .Where(t => t.Owner.UserId == (int)loggedID)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToList();
                return View(transactions);
            }
        }

        [HttpPost("account/{userId}")]
        public IActionResult Transact(int userId, Transaction newTrans)
        {
            int? loggedID = HttpContext.Session.GetInt32("LoggedId");
            List<Transaction> transactions = dbContext.Transactions
                .Include(t => t.Owner)
                .Where(t => t.Owner.UserId == (int)loggedID)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();
            ViewBag.LoggedId = (int)loggedID;
            ViewBag.LoggedName = HttpContext.Session.GetString("LoggedName");
            if (loggedID != userId)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Index");
            }
            else
            {
                if(ModelState.IsValid)
                {
                    if(-newTrans.Amount > dbContext.Transactions
                        .Include(t => t.Owner)
                        .Where(t => t.Owner.UserId == (int)loggedID)
                        .Sum(t => t.Amount))
                    {
                        
                        ModelState.AddModelError("Amount", "Insufficient balance!");
                        return View("Account", transactions);
                    }
                    else
                    {
                        newTrans.UserId = userId;
                        dbContext.Transactions.Add(newTrans);
                        dbContext.SaveChanges();
                        return RedirectToAction("Account", new {userId = userId});
                    }
                }
                else
                {
                return View ("Account", transactions);
                }
            }
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
