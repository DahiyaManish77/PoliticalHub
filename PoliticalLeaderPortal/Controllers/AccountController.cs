using PoliticalLeaderPortal.Models;
using PoliticalLeaderPortal.ViewModels;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;

namespace PoliticalLeaderPortal.Controllers
{
    public class AccountController : Controller
    {
        private const int PasswordIterations = 120000;
        private readonly PoliticalLeaderPortalDbEntities1 _db;

        public AccountController()
        {
            _db = new PoliticalLeaderPortalDbEntities1();
        }

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToLocal(returnUrl);
            }

            return View(new AccountLoginVM
            {
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(AccountLoginVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string email = NormalizeEmail(model.EmailAddress);

            ApplicationUser user =
                _db.ApplicationUsers
                .FirstOrDefault(x =>
                    x.EmailAddress.ToLower() == email &&
                    x.IsActive);

            if (user == null || !VerifyPassword(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid email address or password.");
                return View(model);
            }

            user.LastLoginDate = DateTime.Now;
            _db.SaveChanges();

            FormsAuthentication.SetAuthCookie(user.FullName, model.RememberMe);

            Session["UserId"] = user.UserId;
            Session["UserName"] = user.FullName;
            Session["UserEmail"] = user.EmailAddress;
            Session["RoleId"] = user.RoleId;
            Session["RoleName"] = user.ApplicationRole != null
                ? user.ApplicationRole.RoleName
                : "";

            return RedirectAfterLogin(user, model.ReturnUrl);
        }

        [HttpGet]
        public ActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new AccountRegisterVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(AccountRegisterVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string email = NormalizeEmail(model.EmailAddress);

            bool emailExists =
                _db.ApplicationUsers
                .Any(x => x.EmailAddress.ToLower() == email);

            if (emailExists)
            {
                ModelState.AddModelError("EmailAddress", "This email address is already registered.");
                return View(model);
            }

            ApplicationRole role = GetOrCreateCitizenRole();

            ApplicationUser user = new ApplicationUser
            {
                RoleId = role.RoleId,
                FullName = model.FullName.Trim(),
                EmailAddress = email,
                MobileNumber = String.IsNullOrWhiteSpace(model.MobileNumber)
                    ? null
                    : model.MobileNumber.Trim(),
                PasswordHash = HashPassword(model.Password),
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            _db.ApplicationUsers.Add(user);
            _db.SaveChanges();

            FormsAuthentication.SetAuthCookie(user.FullName, false);

            Session["UserId"] = user.UserId;
            Session["UserName"] = user.FullName;
            Session["UserEmail"] = user.EmailAddress;
            Session["RoleId"] = user.RoleId;

            TempData["Success"] = "Your account has been created successfully.";

            return RedirectToAction("Index", "Home");
        }

        private ActionResult RedirectAfterLogin(ApplicationUser user, string returnUrl)
        {
            if (!String.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            string roleName = user.ApplicationRole != null
                ? user.ApplicationRole.RoleName
                : "";

            if (IsAdminRole(roleName))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            return RedirectToAction("Index", "Home");
        }

        private static bool IsAdminRole(string roleName)
        {
            return String.Equals(roleName, "SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
                   String.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase) ||
                   String.Equals(roleName, "Editor", StringComparison.OrdinalIgnoreCase);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();

            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }

            base.Dispose(disposing);
        }

        private ApplicationRole GetOrCreateCitizenRole()
        {
            ApplicationRole role =
                _db.ApplicationRoles
                .FirstOrDefault(x => x.RoleName == "Citizen");

            if (role != null)
            {
                return role;
            }

            role = new ApplicationRole
            {
                RoleName = "Citizen",
                Description = "Public website registered user",
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            _db.ApplicationRoles.Add(role);
            _db.SaveChanges();

            return role;
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (!String.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        private static string NormalizeEmail(string email)
        {
            return (email ?? String.Empty).Trim().ToLowerInvariant();
        }

        private static string HashPassword(string password)
        {
            byte[] salt = new byte[16];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, PasswordIterations))
            {
                byte[] hash = pbkdf2.GetBytes(32);

                return "PBKDF2$" +
                    PasswordIterations +
                    "$" +
                    Convert.ToBase64String(salt) +
                    "$" +
                    Convert.ToBase64String(hash);
            }
        }

        private static bool VerifyPassword(string password, string storedHash)
        {
            if (String.IsNullOrWhiteSpace(password) || String.IsNullOrWhiteSpace(storedHash))
            {
                return false;
            }

            if (!storedHash.StartsWith("PBKDF2$", StringComparison.OrdinalIgnoreCase))
            {
                return String.Equals(password, storedHash);
            }

            string[] parts = storedHash.Split('$');

            if (parts.Length != 4)
            {
                return false;
            }

            int iterations;

            if (!Int32.TryParse(parts[1], out iterations))
            {
                return false;
            }

            byte[] salt = Convert.FromBase64String(parts[2]);
            byte[] expectedHash = Convert.FromBase64String(parts[3]);

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                byte[] actualHash = pbkdf2.GetBytes(expectedHash.Length);

                return FixedTimeEquals(actualHash, expectedHash);
            }
        }

        private static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            int diff = 0;

            for (int i = 0; i < left.Length; i++)
            {
                diff |= left[i] ^ right[i];
            }

            return diff == 0;
        }
    }
}
