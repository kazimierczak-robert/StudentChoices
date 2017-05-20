using StudentChoices.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace StudentChoices.Controllers
{
    public class AdminController : Controller
    {
        public ActionResult ChangePass()
        {
            if (Session["User"].ToString() == "Admin" || Session["User"].ToString() == "SuperAdmin")
            {
                return View();
            }
            else
            {
                return RedirectToAction("", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePass(ChangePass pass)
        {
            if (Session["User"].ToString() == "Admin" || Session["User"].ToString() == "SuperAdmin")
            {
                using (PPDBEntities db = new PPDBEntities())
                {
                    byte[] oldPasswordSHA256;
                    string oldPasswordStr;
                    using (var sha256 = SHA256.Create())
                    {
                        oldPasswordSHA256 = sha256.ComputeHash(Encoding.ASCII.GetBytes(pass.oldPassword));
                        oldPasswordStr = BitConverter.ToString(oldPasswordSHA256).Replace("-", string.Empty);

                        string userLogin = Session["UserName"].ToString();
                        if (oldPasswordStr == db.Admins.Where(x => x.Login == userLogin).Select(x => x.Password).FirstOrDefault().ToString())
                        {
                            if (pass.newPassword == pass.newPassword2)
                            {
                                byte[] newPasswordSHA256 = sha256.ComputeHash(Encoding.ASCII.GetBytes(pass.newPassword));
                                string newPasswordStr = BitConverter.ToString(newPasswordSHA256).Replace("-", string.Empty);

                                var findUser = db.Admins.Where(u => u.Login == userLogin).FirstOrDefault();
                                findUser.Password = newPasswordStr;
                                db.Entry(findUser).State = EntityState.Modified;
                                db.SaveChanges();
                                ModelState.AddModelError("", "Hasło zmieniono pomyślnie!");
                            }
                            else
                            {
                                ModelState.AddModelError("", "Podane hasła nie zgadzają się!");
                            }

                        }
                        else
                        {
                            ModelState.AddModelError("", "Podane stare hasło jest nieprawidłowe!");
                        }
                    }
                    return View();
                }
            }
            else
            {
                return RedirectToAction("", "Home");
            }
        }
    }
}