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
    public class AccountController : Controller
    {
        //Login
        public ActionResult Login()
        {
            if (Session["User"] == null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("", "Home");
            }
        }

        [HttpPost]
        public ActionResult Login(Users user)
        {
            using (PPDBEntities db = new PPDBEntities())
            {
                byte[] clientPassword = Encoding.Default.GetBytes(user.Password);

                //utworzenie skrotu od pobranego hasla (SHA256)
                using (var sha256 = SHA256.Create())
                {
                    byte[] clientPasswordSHA256 = sha256.ComputeHash(clientPassword);
                    string stringClientPasswordSHA256 = Encoding.Default.GetString(clientPasswordSHA256);
                    user.Password = stringClientPasswordSHA256;
                }

                //Sprawdzenie czy to admin
                var usrAdmin = db.Admins.Where(u => u.Login == user.Login &&
                    u.Password == user.Password).FirstOrDefault();

                if (usrAdmin != null)
                {
                    //Sprawdzenie czy konto jest aktywne
                    if (usrAdmin.Active==true)
                    {
                        Session["UserID"] = usrAdmin.AdminID.ToString();
                        if (usrAdmin.SuperAdmin == true)
                        {
                            Session["User"] = "SuperAdmin";
                        }
                        else
                        {
                            Session["User"] = "Admin";
                        }
                        usrAdmin.LastLogin = DateTime.Now;
                        db.Entry(usrAdmin).State = EntityState.Modified;
                        db.SaveChanges();
                        return RedirectToAction("", "Home");
                    }
                }
                else
                {
                    //Sprawdzenie czy to student
                    var usrStudent = db.Students.Where(u => u.Login == user.Login).FirstOrDefault();

                    if (usrStudent != null)
                    {
                        //Sprawdzenie czy nie przekroczono limitu logowań
                        if (usrStudent.TriesNo<3)
                        {
                            //Sprawdzenie poprawności hasła
                            if (usrStudent.Password == user.Password)
                            {
                                Session["UserID"] = usrAdmin.AdminID.ToString();
                                Session["User"] = "Student";
                                usrStudent.TriesNo = 0;
                                db.Entry(usrStudent).State = EntityState.Modified;
                                db.SaveChanges();
                                return RedirectToAction("", "Home");
                            }
                            else
                            {
                                usrStudent.TriesNo += 1;
                                db.Entry(usrStudent).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                    }
                }
            }
            ModelState.AddModelError("", "Dane logowania są niepoprawne!");
            return View();
        }

        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            if (Session["User"] != null)
            {
                Session["UserID"] = null;
                Session["User"] = null;
            }
            return RedirectToAction("", "Home");
        }
    }
}