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
        void setSessionClassGroups(string selectedValue)
        {
            using (PPDBEntities db = new PPDBEntities())
            {
                List<string> ClassGroups = new List<string>();
                string oneClassGroup = string.Empty;
                foreach (var elem in db.ClassGroups)
                {
                    oneClassGroup = elem.DegreeCourse.ToString() + "/" + elem.Graduate.ToString() + "/";
                    if (elem.FullTimeStudies == true) { oneClassGroup += "STACJ"; }
                    else { oneClassGroup += "NIESTACJ"; }
                    oneClassGroup += "/" + elem.Semester.ToString() + "/" + elem.Speciality.ToString();
                    ClassGroups.Add(oneClassGroup);
                }
                if (ClassGroups.IndexOf(selectedValue)>=0)
                {
                    ClassGroups.Remove(selectedValue);
                    ClassGroups.Insert(0, selectedValue);
                }
                Session["ClassGroups"] = new SelectList(ClassGroups);
                setSessionCategoriesAndStats("", ClassGroups.ElementAt(0));
            }
        }

        void setSessionCategoriesAndStats(string selectedValue, string selectedClassGroup)
        {
            using (PPDBEntities db = new PPDBEntities())
            {
                var selectedClassGroupDetails= selectedClassGroup.Split('/');
                
                string DegreeCourse = selectedClassGroupDetails[0];
                Byte Graduate = Byte.Parse(selectedClassGroupDetails[1]);
                bool FullTimeStudies = selectedClassGroupDetails[2] == "STACJ" ? true : false;
                Byte Semester = Byte.Parse(selectedClassGroupDetails[3]);
                string Speciality = selectedClassGroupDetails[4];

                var selectedClassGroupID = db.ClassGroups.Where(x=>x.DegreeCourse== DegreeCourse &&
                    x.Graduate == Graduate && x.FullTimeStudies == FullTimeStudies &&
                    x.Semester == Semester && x.Speciality == Speciality).FirstOrDefault().ClassGroupID;

                Session["NoOfStudents"] = db.StudentsAndClassGroups.Where(x => x.ClassGroupID == selectedClassGroupID).Count();

                List<string> Categories = new List<string>();
                foreach (var elem in db.Categories.Where(x => x.ClassGroupID == selectedClassGroupID).Select(x => x.Name))
                {
                    Categories.Add(elem);
                }
                if (Categories.IndexOf(selectedValue) >= 0)
                {
                    Categories.Remove(selectedValue);
                    Categories.Insert(0, selectedValue);
                }
                Session["Categories"] = new SelectList(Categories);

                int NoOfSavedStudents = 0;
                int NoOfSavedStudentsOnOneSubject = 0;
                Dictionary<string, int> stats = new Dictionary<string, int>();

                var firstCategoryName = Categories.ElementAt(0);
                var firstCategoryID = db.Categories.Where(x => x.Name == firstCategoryName).FirstOrDefault().CategoryID;
                foreach (var item in db.ElectiveSubjectsAndSpecialities.Where(x => x.CategoryID == firstCategoryID))
                {
                    NoOfSavedStudentsOnOneSubject = db.StudentChoices.Where(x => x.ChoiceID == item.ElectiveSubjectAndSpecialityID && x.PreferenceNo == 1).Count();
                    stats.Add(item.Name, NoOfSavedStudentsOnOneSubject);
                    NoOfSavedStudents += NoOfSavedStudentsOnOneSubject;
                }
                Session["NoOfSavedStudents"] = NoOfSavedStudents;
                Session["Stats"] = stats;
            }
        }

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

                        if (Session["ClassGroups"] == null)
                        {
                            setSessionClassGroups("");
                        }

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
                                    //TODO
                                    var ClassGroupIDs = db.StudentsAndClassGroups.Where(x => x.StudentNo == usrStudent.StudentNo).Select(x => x.ClassGroupID).FirstOrDefault();
                                    var CategoryIDs = db.Categories.Where(x => x.ClassGroupID == ClassGroupIDs).Select(x => x.CategoryID).FirstOrDefault();

                                    Subjects.Add("Nie wybrano");
                                    foreach (var elem in db.ElectiveSubjectsAndSpecialities.Where(x => x.CategoryID == CategoryIDs).ToList())
                                    {
                                        Subjects.Add(elem.Name);
                                    }
                                    Session["Subjects"] = new SelectList(Subjects);
                                }
                                else if((bool)HttpContext.Application["ShareResults"] == true)
                                {
                                    Dictionary<string, Dictionary<string, string>> resultsAll = new Dictionary<string, Dictionary<string, string>>();

                                    Dictionary<string, string> resultsOnGroup;
                                    string oneClassGroupStr = string.Empty;

                                    var ClassGroups = db.StudentsAndClassGroups.Where(x => x.StudentNo == usrStudent.StudentNo);
                                    foreach (var oneClassGroup in ClassGroups)
                                    {
                                        resultsOnGroup = new Dictionary<string, string>();
                                        var Categories = db.Categories.Where(x => x.ClassGroupID == oneClassGroup.ClassGroupID);
                                        foreach (var Cat in Categories)
                                        {

                                            var FinalChoiceID = db.FinalChoices.Where(x => x.StudentNo == usrStudent.StudentNo && x.CategoryID == Cat.CategoryID).Select(x => x.ChoiceID).FirstOrDefault();
                                            var FinalChoiceName = db.ElectiveSubjectsAndSpecialities.Where(x=>x.ElectiveSubjectAndSpecialityID==FinalChoiceID).Select(x => x.Name).FirstOrDefault();
                                            resultsOnGroup[Cat.Name]= FinalChoiceName;
                                        }
                                        var ClassGroup=db.ClassGroups.Where(x => x.ClassGroupID == oneClassGroup.ClassGroupID).FirstOrDefault();

                                        oneClassGroupStr = ClassGroup.DegreeCourse.ToString() + ", " + ClassGroup.Graduate.ToString() + ". stopień, ";
                                        if (ClassGroup.FullTimeStudies == true) { oneClassGroupStr += "st. stacjonarne"; }
                                        else { oneClassGroupStr += "st. niestacjonarne"; }
                                        oneClassGroupStr += ", sem. " + ClassGroup.Semester.ToString() + "., " + ClassGroup.Speciality.ToString()
                                            + ", średnia ocen: "+ oneClassGroup.AverageGrade.ToString();
                                        
                                        resultsAll[oneClassGroupStr] = resultsOnGroup;
                                    }


                                    Session["Results"] = resultsAll;
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
        public ActionResult ChangeClassGroup(string ClassGroups)
        {
            if (Session["User"].ToString() == "Admin" || Session["User"].ToString() == "SuperAdmin")
            {
                setSessionClassGroups(ClassGroups);
            }
            return RedirectToAction("", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeCategory(string Categories, string ClassGroups)
        {
            if (Session["User"].ToString() == "Admin" || Session["User"].ToString() == "SuperAdmin")
            {
                setSessionCategoriesAndStats(Categories, ClassGroups);
            }
            return RedirectToAction("", "Home");
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
                HttpContext.Application["RecStop"] = date;
                HttpContext.Application["RecStopString"] = endDate;
                HttpContext.Application["ShareResults"] = false;

                if (date.AddDays(1)>DateTime.Now)
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
                Session["ClassGroups"] = null;
                Session["NoOfStudents"] = null;
                Session["Categories"] = null;
                Session["NoOfSavedStudents"] = null;
                Session["Stats"] = null;
                Session["Results"] = null;
            }
            return RedirectToAction("", "Home");
        }

        public ActionResult Run()
        {
            if (Session["User"].ToString() == "Admin" || Session["User"].ToString() == "SuperAdmin")
            {
                if ((bool)HttpContext.Application["RecActive"] == false && (bool)HttpContext.Application["AfterRec"] == true)
                {

                }
            }
            return RedirectToAction("", "Home");
        }
        public ActionResult ShareResults()
        {
            if (Session["User"].ToString() == "Admin" || Session["User"].ToString() == "SuperAdmin")
            {
                if((bool)HttpContext.Application["RecActive"] == false && (bool)HttpContext.Application["AfterRec"] == true)
                {
                    HttpContext.Application["AfterRec"] = false;
                    HttpContext.Application["ShareResults"] = true;
                }
            }
            return RedirectToAction("", "Home");
        }
        public void ExportResults()
        {
            if (Session["User"].ToString() == "Admin" || Session["User"].ToString() == "SuperAdmin")
            {
                if ((bool)HttpContext.Application["RecActive"] == false && (bool)HttpContext.Application["AfterRec"] == true)
                {
                    using (PPDBEntities db = new PPDBEntities())
                    {
                        List<Models.Export.Category> exportData = new List<Models.Export.Category>();

                        var categories = db.ClassGroups.Join(db.Categories, c=>c.ClassGroupID,
                        cm=>cm.ClassGroupID, (c, cm)=>new
                        {
                            CategoryID = cm.CategoryID,
                            Name =cm.Name,
                            DegreeCourse = c.DegreeCourse,
                            Graduate = c.Graduate,
                            FullTimeStudies = c.FullTimeStudies,
                            Semester = c.Semester,
                            Speciality = c.Speciality,
                        }).ToList();

                        List<Models.Export.ElectiveSubjectAndSpeciality> subjectslist;
                        List<string> studentsnolist;

                        foreach (var categoryitem in categories)
                        {
                            subjectslist = new List<Models.Export.ElectiveSubjectAndSpeciality>();
                            foreach (var subjectitem in db.ElectiveSubjectsAndSpecialities.Where(x=>x.CategoryID== categoryitem.CategoryID))
                            {
                                studentsnolist = new List<string>();
                                foreach (var choiceitem in db.FinalChoices.Where(x => x.ChoiceID == subjectitem.ElectiveSubjectAndSpecialityID))
                                {
                                    studentsnolist.Add(choiceitem.StudentNo.ToString());
                                }
                                subjectslist.Add(new Models.Export.ElectiveSubjectAndSpeciality(subjectitem.Name, studentsnolist));
                            }
                            exportData.Add(new Models.Export.Category(categoryitem.Name, categoryitem.DegreeCourse, categoryitem.Graduate, categoryitem.FullTimeStudies, categoryitem.Semester, categoryitem.Speciality, subjectslist));
                        }

                        Response.ClearContent();
                        Response.Buffer = true;
                        Response.AddHeader("content-disposition", "attachment; filename = Results.xml");
                        Response.ContentType = "text/xml";
                        var serializer = new System.Xml.Serialization.XmlSerializer(exportData.GetType());
                        serializer.Serialize(Response.OutputStream, exportData);
                    }
                }
            }
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