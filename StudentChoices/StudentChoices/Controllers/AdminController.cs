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
                                ViewBag.Success = "Hasło zmieniono pomyślnie!";
                            }
                            else
                            {
                                ViewBag.Alert = "Podane hasła nie zgadzają się!";
                            }

                        }
                        else
                        {
                            ViewBag.Alert = "Podane stare hasło jest nieprawidłowe!";
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

        ////////////    DODAWANIE I EDYCJA STUDENTÓW    ////////////////////

        private PPDBEntities db = new PPDBEntities();

        public ActionResult Index()
        {
            var data = (from s in db.Students
                        join g in db.StudentsAndClassGroups on s.StudentNo equals g.StudentNo
                       select new AddStudents
                       {
                           StudentNo = s.StudentNo,
                           Name = s.Name,
                           Surname = s.Surname,
                           ClassGroupID = g.ClassGroupID,
                           AverageGrade = g.AverageGrade,
                           Login = s.Login,
                           Password = s.Password
                       }).ToList();
            return View(data);
        }

        [HttpGet]
        public ActionResult AddStudents()
        {
            PopulateClassGroupsList();
            return View();
        }

        [HttpPost]
        public ActionResult AddStudents([Bind(Include = "StudentNo,Login,Password,Name,Surname,AverageGrade,ClassGroupID")] AddStudents student)
        {
            if (Session["User"].ToString().Contains("Admin"))
            {
                student.Password = HashPassword(student.Password);    
                var studentToAdd = new Students();
                studentToAdd.StudentNo = student.StudentNo;
                studentToAdd.Login = student.Name.ToLower() + "." + student.Surname.ToLower();
                studentToAdd.Password = student.Password;
                studentToAdd.Name = student.Name;
                studentToAdd.Surname = student.Surname;
                studentToAdd.CreatedBy = (int)Session["AdminID"];
                studentToAdd.CreationDate = DateTime.Now;
                var stdToAdd = new StudentsAndClassGroups();
                stdToAdd.StudentNo = student.StudentNo;
                stdToAdd.AverageGrade = student.AverageGrade;
                stdToAdd.ClassGroupID = student.ClassGroupID;
                stdToAdd.CreatedBy = (int)Session["AdminID"];
                stdToAdd.CreationDate = DateTime.Now;
                if (db.Students.Where(st => st.Login == student.Login).Count() == 0)
                {
                    db.Students.Add(studentToAdd);
                    db.StudentsAndClassGroups.Add(stdToAdd);
                    db.SaveChanges();
                    return RedirectToAction("", "Home");
                }
                else
                {
                    ModelState.AddModelError("Login", "Użytkownik o takim loginie już istnieje.");
                }
                return View();
            }
            PopulateClassGroupsList();
            return View();
        }

        [HttpGet]
        public ActionResult EditStudents(int id)
        {
            var student = (from st in db.Students
                           join std in db.StudentsAndClassGroups on st.StudentNo equals std.StudentNo
                            where st.StudentNo == id
                            select new AddStudents()
                            {
                                StudentNo = st.StudentNo,
                                Login = st.Login,
                                Name = st.Name,
                                Surname = st.Surname,
                                AverageGrade = std.AverageGrade,
                                ClassGroupID = std.ClassGroupID
                            }).FirstOrDefault();
            PopulateClassGroupsList(student.ClassGroupID.ToString());
            return View(student);
        }

        [HttpPost]
        public ActionResult EditStudents([Bind(Include = "StudentNo,Login,Password,Name,Surname,AverageGrade,ClassGroupID")] AddStudents student)
        {
            if (Session["User"].ToString().Contains("Admin"))
            {
                var studentToEdit = (from s in db.Students
                                  where s.StudentNo == student.StudentNo
                                  select s).FirstOrDefault();
                studentToEdit.Login = student.Name.ToLower() + "." + student.Surname.ToLower();
                if (student.Password != null)
                {
                    studentToEdit.Password = HashPassword(student.Password);
                }
                studentToEdit.Name = student.Name;
                studentToEdit.Surname = student.Surname;
                var stdToEdit = (from ss in db.StudentsAndClassGroups
                                 where ss.StudentNo == student.StudentNo
                                 select ss).FirstOrDefault();
                stdToEdit.AverageGrade = student.AverageGrade;
                stdToEdit.ClassGroupID = student.ClassGroupID;
                stdToEdit.LastEdit = DateTime.Now;

                if (db.Students.Where(st => st.Login == student.Login).Count() == 0)
                {
                    db.SaveChanges();
                    return RedirectToAction("", "Admin");
                }
                else
                {
                    ModelState.AddModelError("Login", "Użytkownik o takim loginie już istnieje.");
                }
                return View();
            }
            PopulateClassGroupsList();
            return View();
        }

       


        public ActionResult AddData()
        {
            return View();
        }

        public ActionResult EditData()
        {
            return View();
        }

        [HttpGet]
        public ActionResult AddCategories()
        {
            PopulateClassGroupsList();
            return View();
        }

        
        private string HashPassword(string plainPassword)
        {
            byte[] pass = Encoding.Default.GetBytes(plainPassword);
            using (var sha256 = SHA256.Create())
            {
                byte[] hashPass = sha256.ComputeHash(pass);
                return BitConverter.ToString(hashPass).Replace("-", string.Empty);
            }
        }

        private void PopulateClassGroupsList(object selectedClassGroup = null)
        {
            var ctx = new PPDBEntities();
            var classgroups = from j in ctx.ClassGroups
                              select new
                              {
                                  ClassGroupID = j.ClassGroupID,
                                  ClassGroup = j.DegreeCourse + "/" + j.Graduate + "/" + j.FullTimeStudies + "/" + j.Semester + "/" + j.Speciality
                              };
            ViewBag.ClassGroupID = new SelectList(classgroups, "ClassGroupID", "ClassGroup", selectedClassGroup);
        }

    }
}