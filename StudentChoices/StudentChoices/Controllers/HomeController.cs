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
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(Users user)
        {
            using (PPDBEntities db = new PPDBEntities())
            {
                byte[] clientPassword = Encoding.ASCII.GetBytes(user.Password);

                //utworzenie skrotu od pobranego hasla (SHA256)
                using (var sha256 = SHA256.Create())
                {
                    byte[] clientPasswordSHA256 = sha256.ComputeHash(clientPassword);
                    user.Password = BitConverter.ToString(clientPasswordSHA256).Replace("-", string.Empty); ;
                }

                //Sprawdzenie czy to admin
                var usrAdmin = db.Admins.Where(u => u.Login == user.Login &&
                    u.Password == user.Password).FirstOrDefault();

                if (usrAdmin != null)
                {
                    //Sprawdzenie czy konto jest aktywne
                    if (usrAdmin.Active == true)
                    {
                        Session["UserName"] = usrAdmin.Login;
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

                        List<string> ClassGroups = new List<string>();
                        string oneClassGroup = string.Empty;
                        foreach (var elem in db.ClassGroups)
                        {
                            oneClassGroup = elem.DegreeCourse.ToString() + "/" + elem.Graduate.ToString() + "/";
                            if(elem.FullTimeStudies==true) { oneClassGroup += "STACJ"; }
                            else { oneClassGroup += "NIESTACJ"; }
                            oneClassGroup += "/" + elem.Semester.ToString() + "/" +elem.Speciality.ToString();
                            ClassGroups.Add(oneClassGroup);
                        }
                        Session["ClassGroups"] = new SelectList(ClassGroups);

                        List<string> Categories = new List<string>();
                        foreach (var elem in db.Categories.Where(x => x.ClassGroupID == db.ClassGroups.FirstOrDefault().ClassGroupID).Select(x => x.Name))
                        {
                            Categories.Add(elem);
                        }
                        Session["Categories"] = new SelectList(Categories);
                        Session["NoOfStudents"] = db.StudentsAndClassGroups.Where(x => x.ClassGroupID == db.ClassGroups.FirstOrDefault().ClassGroupID).Count();

                        int NoOfSavedStudents = 0;
                        int NoOfSavedStudentsOnOneSubject = 0;
                        Dictionary<string, int> stats = new Dictionary<string, int>();
                        foreach (var elem in db.Categories.Where(x => x.ClassGroupID == db.ClassGroups.FirstOrDefault().ClassGroupID).Select(x => x.CategoryID))
                        {
                            foreach (var item in db.ElectiveSubjectsAndSpecialities.Where(x => x.CategoryID==elem))
                            {
                                NoOfSavedStudentsOnOneSubject = db.StudentChoices.Where(x => x.ChoiceID == item.ElectiveSubjectAndSpecialityID && x.PreferenceNo == 1).Count();
                                stats.Add(item.Name, NoOfSavedStudentsOnOneSubject);
                                NoOfSavedStudents += NoOfSavedStudentsOnOneSubject;
                            }                          
                        }
                        Session["NoOfSavedStudents"] = NoOfSavedStudents;
                        Session["Stats"] = stats;

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
                        if (usrStudent.TriesNo < 3)
                        {
                            //Sprawdzenie poprawności hasła
                            if (usrStudent.Password == user.Password)
                            {
                                Session["UserName"] = usrStudent.Login + " (" + usrStudent.Name + " " + usrStudent.Surname + ")";
                                Session["User"] = "Student";
                                usrStudent.TriesNo = 0;
                                db.Entry(usrStudent).State = EntityState.Modified;
                                db.SaveChanges();

                                if ((bool)HttpContext.Application["RecActive"] == true)
                                {
                                    List<string> Subjects = new List<string>();
                                    var ClassGroupIDs = db.StudentsAndClassGroups.Where(x => x.StudentNo == usrStudent.StudentNo).Select(x => x.ClassGroupID).FirstOrDefault();
                                    var CategoryIDs = db.Categories.Where(x => x.ClassGroupID == ClassGroupIDs).Select(x => x.CategoryID).FirstOrDefault();

                                    Subjects.Add("Nie wybrano");
                                    foreach (var elem in db.ElectiveSubjectsAndSpecialities.Where(x => x.CategoryID == CategoryIDs).ToList())
                                    {
                                        Subjects.Add(elem.Name);
                                    }
                                    Session["Subjects"] = new SelectList(Subjects);
                                }
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveConfig(bool isRecruitmentActive, string endDate)
        {
            if (Session["User"].ToString() == "Admin" || Session["User"].ToString() == "SuperAdmin")
            {
                HttpContext.Application["RecActive"] = isRecruitmentActive;
                var dateStr = endDate.Split('.');
                var date= new DateTime(Int32.Parse(dateStr[2]), Int32.Parse(dateStr[1]), Int32.Parse(dateStr[0]));
                HttpContext.Application["RecStop"] = 
                HttpContext.Application["RecStopString"] = endDate;

                if(date.AddDays(1)>DateTime.Now)
                {
                    HttpContext.Application["RecActive"] = true;
                    HttpContext.Application["AfterRec"] = false;
                }
                else
                {
                    HttpContext.Application["RecActive"] = false;
                    HttpContext.Application["AfterRec"] = true;
                }

            }
            return RedirectToAction("", "Home");
        }

        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            if (Session["User"] != null)
            {
                Session["UserName"] = null;
                Session["User"] = null;
                Session["Subjects"] = null;
            }
            return RedirectToAction("", "Home");
        }

        public ActionResult About()
        {
            //ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            //ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}