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

        public ActionResult IndexStudent()
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
                student.StudentNo = Math.Abs(student.StudentNo);
                student.Login = student.Name.ToLower() + "." + student.Surname.ToLower();
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

                if (db.Students.Where(st => st.StudentNo == student.StudentNo).Count() == 0 && db.StudentsAndClassGroups.Where(st => st.StudentNo == student.StudentNo).Count() == 0)
                {
                    if (db.Students.Where(st => st.Login == student.Login).Count() == 0)
                    {
                        if (student.AverageGrade >= 2.0 && student.AverageGrade <= 5.0)
                        {
                            db.Students.Add(studentToAdd);
                            db.StudentsAndClassGroups.Add(stdToAdd);
                            db.SaveChanges();
                            TempData["Success"] = "Dodano studenta pomyślnie!";
                            return RedirectToAction("", "Home");
                        }
                        else
                        {
                            ModelState.AddModelError("AverageGrade", "Średnia musi być z przedziału 2.0 do 5.0!");
                        }
                    }
                    else
                    {
                        //ModelState.AddModelError("Surname", "Użytkownik o takim loginie już istnieje!");
                        ViewBag.Alert = "Użytkownik o takim loginie już istnieje!";
                    }
                }
                else
                {
                    ModelState.AddModelError("StudentNo", "Użytkownik o takim indeksie już istnieje!");
                }
            }
            PopulateClassGroupsList();
            return View();
        }

        [HttpGet]
        public ActionResult EditStudents(int id)
        {
            TempData["Info"] = "Puste pole hasła powoduje, że hasło pozostaje niezmienione!";
            var student = (from st in db.Students
                           join std in db.StudentsAndClassGroups on st.StudentNo equals std.StudentNo
                            where st.StudentNo == id
                            select new EditStudents()    
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
        public ActionResult EditStudents([Bind(Include = "StudentNo,Login,Password,Name,Surname,AverageGrade,ClassGroupID")] EditStudents student)//AddStudents
        {
            if (Session["User"].ToString().Contains("Admin"))
            {
                student.Login = student.Name.ToLower() + "." + student.Surname.ToLower();
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
                
                if (student.AverageGrade >= 2.0 && student.AverageGrade <= 5.0)
                {
                    db.SaveChanges();
                    TempData["Success"] = "Edytowano studenta pomyślnie!";
                    return RedirectToAction("", "Home");
                }
                else
                {
                    TempData["Info"] = "Puste pole hasła powoduje, że hasło pozostaje niezmienione!";
                    ModelState.AddModelError("AverageGrade", "Średnia musi być z przedziału 2.0 do 5.0!");
                }
            }
            PopulateClassGroupsList();
            return View();
        }


        ///////////////    DODAWANIE I EDYCJA DANYCH   ////////////////////

        public ActionResult AddData()
        {
            return View();
        }

        public ActionResult EditData()
        {
            return View();
        }

        ////////////    DODAWANIE I EDYCJA KATEGORII   ////////////////////

        [HttpGet]
        public ActionResult AddCategories()
        {
            PopulateClassGroupsList();
            return View();
        }

        [HttpPost]
        public ActionResult AddCategories([Bind(Include = "Name,ClassGroupID,Information,MaxNoChoices")] AddCategories category)
        {
            if (Session["User"].ToString().Contains("Admin"))
            {
                category.MaxNoChoices = Math.Abs(category.MaxNoChoices);
                var categoryToAdd = new Categories();
                categoryToAdd.Name = category.Name;
                categoryToAdd.ClassGroupID = category.ClassGroupID;
                categoryToAdd.Information = category.Information;
                categoryToAdd.MaxNoChoices = category.MaxNoChoices;
                categoryToAdd.CreatedBy = (int)Session["AdminID"];
                categoryToAdd.CreationDate = DateTime.Now;
                if (db.Categories.Where(ct => ct.Name == category.Name).Count() == 0)
                {
                    db.Categories.Add(categoryToAdd);
                    db.SaveChanges();
                    TempData["Success"] = "Dodano kategorię pomyślnie! Aby dodać przedmiot obieralny lub specjalność do kategorii należy wejść w opcję Edycja kategorii.";
                    return RedirectToAction("", "Home");
                }
                else
                {
                    ModelState.AddModelError("Name", "Kategoria o takiej nazwie już istnieje!");
                }
            }
            PopulateClassGroupsList();
            return View();
        }

        public ActionResult IndexCat()
        {
            var data = (from c in db.Categories
                        select new AddCategories
                        {
                            CategoryID = c.CategoryID,
                            ClassGroupID = c.ClassGroupID,
                            Name = c.Name,
                            Information = c.Information,
                            MaxNoChoices = c.MaxNoChoices
                        }).ToList();
            return View(data);
        }

        [HttpGet]
        public ActionResult EditCategories(int id)
        {
            var category = (from ct in db.Categories
                            where ct.CategoryID == id
                            select new AddCategories()
                            {
                                CategoryID = ct.CategoryID,
                                Name = ct.Name,
                                ClassGroupID = ct.ClassGroupID,
                                Information = ct.Information,
                                MaxNoChoices = ct.MaxNoChoices
                            }).FirstOrDefault();
            PopulateClassGroupsList(category.ClassGroupID.ToString());
            return View(category);
        }

        [HttpPost]
        public ActionResult EditCategories([Bind(Include = "CategoryID,Name,ClassGroupID,Information,MaxNoChoices")] AddCategories category)
        {
            if (Session["User"].ToString().Contains("Admin"))
            {
                category.MaxNoChoices = Math.Abs(category.MaxNoChoices);//
                var categoryToEdit = (from c in db.Categories
                                      where c.CategoryID == category.CategoryID
                                      select c).FirstOrDefault();
                categoryToEdit.Name = category.Name;
                categoryToEdit.ClassGroupID = category.ClassGroupID;
                categoryToEdit.Information = category.Information;
                categoryToEdit.MaxNoChoices = category.MaxNoChoices;

                db.Entry(categoryToEdit).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Edytowano kategorię pomyślnie!";
                return RedirectToAction("", "Home");
            }
            PopulateClassGroupsList();
            return View();
        }

        ////////////    DODAWANIE I EDYCJA PRZEDMIOTÓW / SPECJALNOŚCI   ////////////////////

        [HttpGet]
        public ActionResult AddSubSpec(int id)
        {
            if (Session["User"].ToString().Contains("Admin"))
            {  
                var sub = db.Categories.Find(id); 
                if (db.Categories.Where(ct => ct.CategoryID == sub.CategoryID).Count() == 1)
                {
                    ViewBag.CatID = id;
                    return View();
                }
                if (sub == null)
                {
                    ModelState.AddModelError("CategoryID", "Nie ma takiej kategorii!");
                }
            }
            return RedirectToAction("", "EditData");
        }

        [HttpPost]
        public ActionResult AddSubSpec([Bind(Include = "CategoryID,Name,Information,UpperLimit,LowerLimit")] ElectiveSubjectsAndSpecialities subject)
        {
            if (Session["User"].ToString().Contains("Admin"))
            {
                subject.LowerLimit = Math.Abs((sbyte)subject.LowerLimit);//
                subject.UpperLimit = Math.Abs((sbyte)subject.UpperLimit);//
                var subjectToAdd = new ElectiveSubjectsAndSpecialities();
                subjectToAdd.CategoryID = subject.CategoryID;
                subjectToAdd.Name = subject.Name;
                subjectToAdd.Information = subject.Information;
                subjectToAdd.UpperLimit = subject.UpperLimit;
                subjectToAdd.LowerLimit = subject.LowerLimit;
                subjectToAdd.CreatedBy = (int)Session["AdminID"];
                subjectToAdd.CreationDate = DateTime.Now;
                
                if (db.ElectiveSubjectsAndSpecialities.Where(ct => ct.Name == subject.Name).Count() != 0)
                {
                    TempData["Alert"] = "Przedmiot/specjalność o takiej nazwie już isnieje!";
                    return RedirectToAction("AddSubSpec", "Admin", new { @id = subject.CategoryID });
                }
                if (subject.UpperLimit < subject.LowerLimit)
                {
                    TempData["Alert"] = "Limit górny nie może być mniejszy od dolnego!";
                    return RedirectToAction("AddSubSpec", "Admin", new { @id = subject.CategoryID });
                }
                else if(db.ElectiveSubjectsAndSpecialities.Where(ct => ct.Name == subject.Name).Count() == 0 && subject.UpperLimit >= subject.LowerLimit)
                {
                    db.ElectiveSubjectsAndSpecialities.Add(subjectToAdd);
                    db.SaveChanges();
                    TempData["Success"] = "Dodano przedmiot/specjalność pomyślnie!";
                    return RedirectToAction("", "Home");
                }
            }
            return View();
        }

        public ActionResult IndexSubSpec(int id)
        {
            var data = (from s in db.ElectiveSubjectsAndSpecialities
                        join sub in db.Categories on s.CategoryID equals sub.CategoryID
                        where s.CategoryID == id
                        select new AddSubSpec()
                        {
                            CategoryID = s.CategoryID,
                            ElectiveSubjectAndSpecialityID = s.ElectiveSubjectAndSpecialityID,
                            Name = s.Name,
                            Information = s.Information,
                            UpperLimit = s.UpperLimit,
                            LowerLimit = s.LowerLimit
                        }).ToList();
            return View(data);
        }
        
        [HttpGet]
        public ActionResult EditSubSpec(int id)
        {
            if (Session["User"].ToString().Contains("Admin"))
            {
                var sub = db.Categories.Find(id);
                if (db.Categories.Where(ct => ct.CategoryID == sub.CategoryID).Count() == 1)
                {
                    ViewBag.CatID = id;
                    
                    var subject = (from s in db.ElectiveSubjectsAndSpecialities
                                   join ctg in db.Categories on s.CategoryID equals ctg.CategoryID
                                   where s.CategoryID == id 
                                   select new AddSubSpec()
                                   {
                                       CategoryID = s.CategoryID,
                                       ElectiveSubjectAndSpecialityID = s.ElectiveSubjectAndSpecialityID,
                                       Name = s.Name,
                                       Information = s.Information,
                                       UpperLimit = s.UpperLimit,
                                       LowerLimit = s.LowerLimit
                                   }).FirstOrDefault();

                    ViewBag.SubID = subject.ElectiveSubjectAndSpecialityID;
                    return View(subject);
                }
                if (sub == null)
                {
                    ModelState.AddModelError("CategoryID", "Nie ma takiej kategorii!");
                }
            }
            return RedirectToAction("", "EditData");
        }

        [HttpPost]
        public ActionResult EditSubSpec([Bind(Include = "ElectiveSubjectAndSpecialityID,CategoryID,Name,Information,UpperLimit,LowerLimit")] ElectiveSubjectsAndSpecialities subject)
        {
            if (Session["User"].ToString().Contains("Admin"))
            {
                var subjectToEdit = (from s in db.ElectiveSubjectsAndSpecialities
                                      where s.ElectiveSubjectAndSpecialityID == subject.ElectiveSubjectAndSpecialityID
                                      && s.CategoryID == subject.CategoryID                                                                                
                                     select s).FirstOrDefault();
                subjectToEdit.Name = subject.Name;
                subjectToEdit.Information = subject.Information;
                subjectToEdit.UpperLimit = subject.UpperLimit;
                subjectToEdit.LowerLimit = subject.LowerLimit;

                if (subject.UpperLimit >= subject.LowerLimit)
                {
                    db.Entry(subjectToEdit).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Edytowano przedmiot/specjalność pomyślnie!";
                    return RedirectToAction("", "Home");
                }
                else
                {
                    TempData["Alert"] = "Limit górny nie może być mniejszy od dolnego!";
                    return RedirectToAction("EditSubSpec", "Admin", new { @id = subject.CategoryID });
                }
            }
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

   
        //////////////////////////////////////////////////////////

    [HttpGet]
    public ActionResult AddGroups()
    {
        return View();
    }

    [HttpGet]
    public ActionResult AddGroups([Bind(Include ="DegreeCourse,Graduate,FullTimeStudies,Semester,Speciality")]AddGroups group)
    {
        if (Session["User"].ToString().Contains("Admin"))
        {
                var groupsToAdd = new ClassGroups();
                groupsToAdd.DegreeCourse = group.DegreeCourse;
                groupsToAdd.Graduate = group.Graduate;
                groupsToAdd.FullTimeStudies = group.FullTimeStudies;
                groupsToAdd.Semester = group.Semester;
                groupsToAdd.Speciality = group.Speciality;

                db.ClassGroups.Add(groupsToAdd);
                db.SaveChanges();

        }

            return View();
    }

    public ActionResult IndexGroup()
        {
            var data = (from g in db.ClassGroups
                        select new AddGroups
                        {
                            DegreeCourse = g.DegreeCourse,
                            Graduate = g.Graduate,
                            FullTimeStudies = g.FullTimeStudies,
                            Semester = g.Semester,
                            Speciality = g.Speciality
                        }).ToList();
            return View(data);
        }

    [HttpGet]
    public ActionResult EditGroups (int id)
    {
            var data = (from g in db.ClassGroups
                        where g.ClassGroupID == id
                        select new AddGroups
                        {
                            DegreeCourse = g.DegreeCourse,
                            Graduate = g.Graduate,
                            FullTimeStudies = g.FullTimeStudies,
                            Semester = g.Semester,
                            Speciality = g.Speciality
                        }).FirstOrDefault();
            return View(data);
        }

        [HttpPost]
        public ActionResult EditGroups([Bind(Include = "DegreeCourse,Graduate,FullTimeStudies,Semester,Speciality")] AddGroups groups)
        {
            if (Session["User"].ToString().Contains("Admin"))
            {
                var groupsToEdit = (from g in db.ClassGroups
                                    where g.ClassGroupID == groups.ClassGroupID
                                    select g
                    ).FirstOrDefault();
                groupsToEdit.DegreeCourse = groups.DegreeCourse;
                groupsToEdit.Graduate = groups.Graduate;
                groupsToEdit.FullTimeStudies = groups.FullTimeStudies;
                groupsToEdit.Semester = groups.Semester;
                groupsToEdit.Speciality = groups.Speciality;

                db.Entry(groupsToEdit).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("","Home");
            }
            return View();
        }


        //////////////////////////////////////////////// ADMIN

        [HttpGet]
        public ActionResult AddAdmin()
        {
            return View();
        }

        [HttpGet]
        public ActionResult AddAdmin([Bind(Include = "Login,Password,Active,SuperAdmin")]AddAdmin admin)
        {
            if (Session["User"].ToString().Contains("SuperAdmin"))
            {
                var AdminToAdd = new Admins();
                AdminToAdd.Login = admin.Login;
                AdminToAdd.Password = admin.Password;
                AdminToAdd.Active = admin.Active;
                AdminToAdd.SuperAdmin = admin.SuperAdmin;


                db.Admins.Add(AdminToAdd);
                db.SaveChanges();

            }

            return View();
        }

        public ActionResult IndexAdmin()
        {
            var data = (from a in db.Admins
                        select new AddAdmin
                        {
                            Login = a.Login,
                            Password = a.Password,
                            Active = a.Active,
                            SuperAdmin = a.SuperAdmin
                        }).ToList();
            return View(data);
        }

        [HttpGet]
        public ActionResult EditAdmin(int id)
        {
            var data = (from a in db.Admins
                        where a.AdminID == id
                        select new AddAdmin
                        {
                            Login = a.Login,
                            Password = a.Password,
                            Active = a.Active,
                            SuperAdmin = a.SuperAdmin
                        }).FirstOrDefault();
            return View(data);
        }

        [HttpPost]
        public ActionResult EditAdmin([Bind(Include = "Login,Password,Active,SuperAdmin")] AddAdmin admin)
        {
            if (Session["User"].ToString().Contains("SuperAdmin"))
            {
                var adminToEdit = (from g in db.Admins
                                    where g.AdminID == admin.AdminID
                                    select g
                    ).FirstOrDefault();
                adminToEdit.Login = admin.Login;
                adminToEdit.Password = admin.Password;
                adminToEdit.Active = admin.Active;
                adminToEdit.SuperAdmin = admin.SuperAdmin;


                db.Entry(adminToEdit).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("", "Home");
            }
            return View();
        }

        public ActionResult Import()
        {
            return View();
        }

    }
}