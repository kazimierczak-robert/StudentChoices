using StudentChoices.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;

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
                stdToAdd.AverageGrade = Math.Round(student.AverageGrade, 2);
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
                stdToEdit.AverageGrade = Math.Round(student.AverageGrade, 2);
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
                else if (db.ElectiveSubjectsAndSpecialities.Where(ct => ct.Name == subject.Name).Count() == 0 && subject.UpperLimit >= subject.LowerLimit)
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
        public ActionResult AddGroups([Bind(Include = "DegreeCourse,Graduate,FullTimeStudies,Semester,Speciality")]AddGroups group)
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
        public ActionResult EditGroups(int id)
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
                return RedirectToAction("", "Home");
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

        public ActionResult MakeChanges()
        {
            if (Session["ClassGroupsEditFinalChoices"] == null)
            {
                setSessionClassGroups("");
            }
            return View();
        }


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
                if (ClassGroups.IndexOf(selectedValue) >= 0)
                {
                    ClassGroups.Remove(selectedValue);
                    ClassGroups.Insert(0, selectedValue);
                }
                Session["ClassGroupsEditFinalChoices"] = new SelectList(ClassGroups);
                setSessionCategoriesAndStats("", ClassGroups.ElementAt(0));
            }
        }

        void setSessionCategoriesAndStats(string selectedValue, string selectedClassGroup)
        {
            using (PPDBEntities db = new PPDBEntities())
            {
                var selectedClassGroupDetails = selectedClassGroup.Split('/');

                string DegreeCourse = selectedClassGroupDetails[0];
                Byte Graduate = Byte.Parse(selectedClassGroupDetails[1]);
                bool FullTimeStudies = selectedClassGroupDetails[2] == "STACJ" ? true : false;
                Byte Semester = Byte.Parse(selectedClassGroupDetails[3]);
                string Speciality = selectedClassGroupDetails[4];

                var selectedClassGroupID = db.ClassGroups.Where(x => x.DegreeCourse == DegreeCourse &&
x.Graduate == Graduate && x.FullTimeStudies == FullTimeStudies &&
x.Semester == Semester && x.Speciality == Speciality).FirstOrDefault().ClassGroupID;

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
                Session["CategoriesEditFinalChoices"] = new SelectList(Categories);

                Dictionary<int, string[]> subjectsStats = new Dictionary<int, string[]>();

                
                var firstCategoryName = Categories.ElementAt(0);
                var firstCategoryID = db.Categories.Where(x => x.Name == firstCategoryName).FirstOrDefault().CategoryID;

                Session["SubjectCombobox"] = new SelectList(subjectsStats, "Key", "Value[0]");


                string upperLimit = "";
                string lowerLimit = "";
                string noOfStudents = "";

                foreach (var subject in db.ElectiveSubjectsAndSpecialities.Where(x => x.CategoryID == firstCategoryID))
                {
                    if(subject.UpperLimit.HasValue)
                    {
                        upperLimit = subject.UpperLimit.ToString();
                    }
                    else
                    {
                        upperLimit = "brak";
                    }
                    if (subject.LowerLimit.HasValue)
                    {
                        lowerLimit = subject.LowerLimit.ToString();
                    }
                    else
                    {
                        lowerLimit = "brak";
                    }
                    noOfStudents = db.FinalChoices.Where(x => x.ChoiceID == subject.ElectiveSubjectAndSpecialityID).Count().ToString();
                    subjectsStats.Add(subject.ElectiveSubjectAndSpecialityID, new string[] { subject.Name, lowerLimit, upperLimit, noOfStudents });
                }

                Session["SubjectsStats"] = subjectsStats;

                Dictionary<int, Dictionary<int, string[]>> students = new Dictionary<int,  Dictionary<int, string[]>>();
                string studentName = "";
                string studentGrade = "";
                foreach (var subject in db.ElectiveSubjectsAndSpecialities.Where(x => x.CategoryID == firstCategoryID))
                {
                    Dictionary<int, string[]> studentsOneSubject = new Dictionary<int, string[]>();

                    foreach (var finalChoice in db.FinalChoices.Where(x => x.ChoiceID == subject.ElectiveSubjectAndSpecialityID))
                    {
                        string[] studentDetails = new string[2];

                        studentName = db.Students.Where(x=>x.StudentNo == finalChoice.StudentNo).Select(x=>x.Name).FirstOrDefault() + " " + db.Students.Where(x => x.StudentNo == finalChoice.StudentNo).Select(x => x.Surname).FirstOrDefault();
                        studentGrade = db.StudentsAndClassGroups.Where(x => x.StudentNo == finalChoice.StudentNo && x.ClassGroupID == selectedClassGroupID).Select(x => x.AverageGrade).FirstOrDefault().ToString();

                        studentDetails[0] = studentName;
                        studentDetails[1] = studentGrade;

                        studentsOneSubject.Add(finalChoice.StudentNo, studentDetails);
                    }
                    students.Add(subject.ElectiveSubjectAndSpecialityID, studentsOneSubject);
                }
                Session["SubjectsStudents"] = students;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeClassGroup(string ClassGroups)
        {
            if (Session["User"].ToString() == "Admin" || Session["User"].ToString() == "SuperAdmin")
            {
                setSessionClassGroups(ClassGroups);
            }
            return RedirectToAction("MakeChanges", "Admin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeCategory(string Categories, string ClassGroups)
        {
            if (Session["User"].ToString() == "Admin" || Session["User"].ToString() == "SuperAdmin")
            {
                setSessionCategoriesAndStats(Categories, ClassGroups);
            }
            return RedirectToAction("MakeChanges", "Admin");
        }

        [HttpPost]
        public ActionResult MoveStudent(string StudentId, int OldSubID, int NewSubID)
        {
            if (Session["User"].ToString() == "Admin" || Session["User"].ToString() == "SuperAdmin")
            {
                var subjectsStats = (Dictionary<int, string[]>)Session["SubjectsStats"];
                if(subjectsStats[NewSubID].ElementAt(2) != "brak")
                {
                    if (Int32.Parse(subjectsStats[NewSubID][2]) == Int32.Parse(subjectsStats[NewSubID][3]))
                    {
                        TempData["Alert"] = "Nie można przenieść studenta - liczba studentów w docelowej grupie będzie większa niż jej limit górny!";
                        return RedirectToAction("MakeChanges", "Admin");
                    }
                }
                if (subjectsStats[OldSubID].ElementAt(1) != "brak")
                {
                    if (Int32.Parse(subjectsStats[OldSubID][1]) == Int32.Parse(subjectsStats[OldSubID][3]))
                    {
                        TempData["Alert"] = "Nie można przenieść studenta - liczba studentów w grupie będzie mniejsza niż jej limit dolny!";
                        return RedirectToAction("MakeChanges", "Admin");
                    }
                }

                subjectsStats[OldSubID][3] = (Int32.Parse(subjectsStats[OldSubID][3]) - 1).ToString();
                subjectsStats[NewSubID][3] = (Int32.Parse(subjectsStats[NewSubID][3]) + 1).ToString();

                var students = (Dictionary<int, Dictionary<int, string[]>>)Session["SubjectsStudents"];

                string[] student = new string[2];
                int stdNo = Int32.Parse(StudentId);

                student = students[OldSubID][stdNo];
                students[OldSubID].Remove(stdNo);
                students[NewSubID].Add(stdNo, student);

                var finalChoiceToEdit = db.FinalChoices.Where(x => x.StudentNo == stdNo && x.ChoiceID==OldSubID).FirstOrDefault();
                finalChoiceToEdit.ChoiceID = NewSubID;
                finalChoiceToEdit.LastEditedBy = (int)Session["AdminID"];
                finalChoiceToEdit.LastEdit = DateTime.Now;
                db.Entry(finalChoiceToEdit).State = EntityState.Modified;
                db.SaveChanges();

                Session["SubjectsStats"] = subjectsStats;
                Session["SubjectsStudents"] = students;

                TempData["Success"] = "Przeniesiono studenta pomyślnie!";
            }
            return RedirectToAction("MakeChanges", "Admin");
        }

        public string GetStudentSelectList(int subID)
        {
            var students = ((Dictionary<int, Dictionary<int, string[]>>)Session["SubjectsStudents"])[subID];
            string result = "";
            foreach(KeyValuePair <int, string[]> pair in students)
            {
                result += pair.Key + ",";
                result += pair.Value[0] +",";
            }
            result.Remove(result.Length - 2, 1);
            return result;
        }

        [HttpPost]
        public ActionResult SwapStudent(int StudentIdSwap, int OldSubIDSwap, int SwapStudentID, int SwapSubjectID)
        {
            if (Session["User"].ToString() == "Admin" || Session["User"].ToString() == "SuperAdmin")
            {
                var students = (Dictionary<int, Dictionary<int, string[]>>)Session["SubjectsStudents"];

                string[] student = new string[2];

                student = students[OldSubIDSwap][StudentIdSwap];
                students[OldSubIDSwap].Remove(StudentIdSwap);
                students[SwapSubjectID].Add(StudentIdSwap, student);

                student = students[SwapSubjectID][SwapStudentID];
                students[SwapSubjectID].Remove(SwapStudentID);
                students[OldSubIDSwap].Add(SwapStudentID, student);

                var finalChoiceToEdit = db.FinalChoices.Where(x => x.StudentNo == StudentIdSwap && x.ChoiceID == OldSubIDSwap).FirstOrDefault();
                finalChoiceToEdit.ChoiceID = SwapSubjectID;
                finalChoiceToEdit.LastEditedBy = (int)Session["AdminID"];
                finalChoiceToEdit.LastEdit = DateTime.Now;
                db.Entry(finalChoiceToEdit).State = EntityState.Modified;

                finalChoiceToEdit = db.FinalChoices.Where(x => x.StudentNo == SwapStudentID && x.ChoiceID == SwapSubjectID).FirstOrDefault();
                finalChoiceToEdit.ChoiceID = OldSubIDSwap;
                finalChoiceToEdit.LastEditedBy = (int)Session["AdminID"];
                finalChoiceToEdit.LastEdit = DateTime.Now;
                db.Entry(finalChoiceToEdit).State = EntityState.Modified;

                db.SaveChanges();

                Session["SubjectsStudents"] = students;

                TempData["Success"] = "Zamieniono studentów pomyślnie!";
            }
            return RedirectToAction("MakeChanges", "Admin");
        }

        public ActionResult Import()
        {
            Session["Data"] = null;
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
        public ActionResult Import(HttpPostedFileBase file)
        {
            if (file != null && (Session["User"].ToString() == "Admin" || Session["User"].ToString() == "SuperAdmin"))
            {
                byte[] bytes = new byte[17];
                file.InputStream.Seek(24, SeekOrigin.Begin);
                file.InputStream.Read(bytes, 0, 17);
                file.InputStream.Seek(0, SeekOrigin.Begin);

                string result = System.Text.Encoding.UTF8.GetString(bytes).Split(' ')[0];

                if (result == "ArrayOfAdmin" && Session["User"].ToString() == "SuperAdmin")
                {
                    /*var filename = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/App_Data"), filename);
                    file.SaveAs(path);*/
                    List<StudentChoices.Models.Import.Admin> admins = null;

                    XmlSerializer serializer = new XmlSerializer(typeof(List<StudentChoices.Models.Import.Admin>));
                    try
                    {
                        admins = (List<StudentChoices.Models.Import.Admin>)serializer.Deserialize(file.InputStream);
                        foreach (var admin in admins)
                        {
                            if (admin.Login == "admin")
                            {
                                admins.Remove(admin);
                                break;
                            }
                        }
                        TempData["Success"] = "Wczytano dane pomyślnie!";
                        ViewBag.nameOfImportedData = "Administratorzy";
                        ViewBag.importedData = admins;
                    }
                    catch (Exception)
                    {
                        TempData["Alert"] = "Wystąpił błąd podczas odczytu pliku! Sprawdź zawartość pliku i spróbuj ponownie.";
                    }

                }
                else if (result == "ArrayOfStudent")
                {
                    List<StudentChoices.Models.Import.Student.Student> students = null;
                    XmlSerializer serializer = new XmlSerializer(typeof(List<StudentChoices.Models.Import.Student.Student>));
                    try
                    {
                        students = (List<StudentChoices.Models.Import.Student.Student>)serializer.Deserialize(file.InputStream);
                        TempData["Success"] = "Wczytano dane pomyślnie!";
                        ViewBag.nameOfImportedData = "Studenci";
                        ViewBag.importedData = students;
                    }
                    catch (Exception)
                    {
                        TempData["Alert"] = "Wystąpił błąd podczas odczytu pliku! Sprawdź zawartość pliku i spróbuj ponownie.";
                    }
                }
                else if (result == "ArrayOfClassGroup")
                {
                    List<StudentChoices.Models.Import.Subject.ClassGroup> subjects = null;
                    XmlSerializer serializer = new XmlSerializer(typeof(List<StudentChoices.Models.Import.Subject.ClassGroup>));
                    try
                    {
                        subjects = (List<StudentChoices.Models.Import.Subject.ClassGroup>)serializer.Deserialize(file.InputStream);
                        TempData["Success"] = "Wczytano dane pomyślnie!";
                        ViewBag.nameOfImportedData = "Przedmioty obieralne i specjelności";
                        ViewBag.importedData = subjects;
                    }
                    catch (Exception)
                    {
                        TempData["Alert"] = "Wystąpił błąd podczas odczytu pliku! Sprawdź zawartość pliku i spróbuj ponownie.";
                    }
                }
                else
                {
                    TempData["Alert"] = "Nie rozpoznano rodzaju danych! Sprawdź zawartość pliku i spróbuj ponownie.";
                }
                Session["Data"] = ViewBag.importedData;
            }
            return View();
        }

        [HttpPost]
        public void changeImportedData(string nameOfImportedData, int row, string columnName, string newValue)
        {
            if (Session["User"].ToString() == "Admin" || Session["User"].ToString() == "SuperAdmin")
            {
                if (nameOfImportedData == "Administratorzy" && Session["User"].ToString() == "SuperAdmin")
                {
                    List<StudentChoices.Models.Import.Admin> admins = (List<StudentChoices.Models.Import.Admin>)Session["Data"];
                    PropertyInfo propertyInfo = admins[row].GetType().GetProperty(columnName);
                    propertyInfo.SetValue(admins[row], Convert.ChangeType(newValue, propertyInfo.PropertyType), null);
                    Session["Data"] = admins;
                }
                else if (nameOfImportedData == "Studenci")
                {
                    List<StudentChoices.Models.Import.Student.Student> students = (List<StudentChoices.Models.Import.Student.Student>)Session["Data"];
                    int tempCounter = 0;
                    foreach (var student in students)
                    {
                        if (tempCounter == row)
                        {
                            PropertyInfo propertyInfo = student.GetType().GetProperty(columnName);
                            propertyInfo.SetValue(student, Convert.ChangeType(newValue, propertyInfo.PropertyType), null);
                            Session["Data"] = students;
                            return;
                        }
                        else
                        {
                            tempCounter += 1;
                        }
                        foreach (var classgroup in student.ClassGroup)
                        {
                            if (tempCounter == row)
                            {
                                PropertyInfo propertyInfo = classgroup.GetType().GetProperty(columnName);
                                propertyInfo.SetValue(classgroup, Convert.ChangeType(newValue, propertyInfo.PropertyType), null);
                                Session["Data"] = students;
                                return;
                            }
                            else
                            {
                                tempCounter += 1;
                            }
                        }
                    }
                }
                else if (nameOfImportedData == "Przedmioty obieralne i specjelności")
                {
                    List<StudentChoices.Models.Import.Subject.ClassGroup> subjects = (List<StudentChoices.Models.Import.Subject.ClassGroup>)Session["Data"];

                    int tempCounter = 0;
                    foreach (var classgroup in subjects)
                    {
                        if (tempCounter == row)
                        {
                            PropertyInfo propertyInfo = classgroup.GetType().GetProperty(columnName);
                            propertyInfo.SetValue(classgroup, Convert.ChangeType(newValue, propertyInfo.PropertyType), null);
                            Session["Data"] = subjects;
                            return;
                        }
                        else
                        {
                            tempCounter += 1;
                        }
                        foreach (var category in classgroup.Category)
                        {
                            if (tempCounter == row)
                            {
                                PropertyInfo propertyInfo = category.GetType().GetProperty(columnName);
                                propertyInfo.SetValue(category, Convert.ChangeType(newValue, propertyInfo.PropertyType), null);
                                Session["Data"] = subjects;
                                return;
                            }
                            else
                            {
                                tempCounter += 1;
                            }
                            foreach (var subject in category.ElectiveSubjectAndSpeciality)
                            {
                                if (tempCounter == row)
                                {
                                    PropertyInfo propertyInfo = subject.GetType().GetProperty(columnName);
                                    propertyInfo.SetValue(subject, Convert.ChangeType(newValue, propertyInfo.PropertyType), null);
                                    Session["Data"] = subjects;
                                    return;
                                }
                                else
                                {
                                    tempCounter += 1;
                                }
                                foreach (var file in subject.Files)
                                {
                                    if (tempCounter == row)
                                    {
                                        PropertyInfo propertyInfo = file.GetType().GetProperty(columnName);
                                        propertyInfo.SetValue(file, Convert.ChangeType(newValue, propertyInfo.PropertyType), null);
                                        Session["Data"] = subjects;
                                        return;
                                    }
                                    else
                                    {
                                        tempCounter += 1;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [HttpPost]
        public void deleteFromImportedData(string nameOfImportedData, int row)
        {
            if (Session["User"].ToString() == "Admin" || Session["User"].ToString() == "SuperAdmin")
            {
                if (nameOfImportedData == "Administratorzy" && Session["User"].ToString() == "SuperAdmin")
                {
                    List<StudentChoices.Models.Import.Admin> admins = (List<StudentChoices.Models.Import.Admin>)Session["Data"];
                    admins[row].ToRemove = true;
                    Session["Data"] = admins;
                }
                else if (nameOfImportedData == "Studenci")
                {
                    List<StudentChoices.Models.Import.Student.Student> students = (List<StudentChoices.Models.Import.Student.Student>)Session["Data"];
                    int tempCounter = 0;
                    foreach (var student in students)
                    {
                        if (tempCounter == row)
                        {
                            student.ToRemove = true;
                            Session["Data"] = students;
                            return;
                        }
                        else
                        {
                            tempCounter += 1;
                        }
                        foreach (var classgroup in student.ClassGroup)
                        {
                            if (tempCounter == row)
                            {
                                classgroup.ToRemove = true;
                                Session["Data"] = students;
                                return;
                            }
                            else
                            {
                                tempCounter += 1;
                            }
                        }
                    }
                }
                else if (nameOfImportedData == "Przedmioty obieralne i specjelności")
                {
                    List<StudentChoices.Models.Import.Subject.ClassGroup> subjects = (List<StudentChoices.Models.Import.Subject.ClassGroup>)Session["Data"];

                    int tempCounter = 0;
                    foreach (var classgroup in subjects)
                    {
                        if (tempCounter == row)
                        {
                            classgroup.ToRemove = true;
                            Session["Data"] = subjects;
                            return;
                        }
                        else
                        {
                            tempCounter += 1;
                        }
                        foreach (var category in classgroup.Category)
                        {
                            if (tempCounter == row)
                            {
                                category.ToRemove = true;
                                Session["Data"] = subjects;
                                return;
                            }
                            else
                            {
                                tempCounter += 1;
                            }
                            foreach (var subject in category.ElectiveSubjectAndSpeciality)
                            {
                                if (tempCounter == row)
                                {
                                    subject.ToRemove = true;
                                    Session["Data"] = subjects;
                                    return;
                                }
                                else
                                {
                                    tempCounter += 1;
                                }
                                foreach (var file in subject.Files)
                                {
                                    if (tempCounter == row)
                                    {
                                        file.ToRemove = true;
                                        Session["Data"] = subjects;
                                        return;
                                    }
                                    else
                                    {
                                        tempCounter += 1;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [HttpPost]
        public ActionResult SaveToDB(string nameOfImportedData)
        {
            if (Session["User"].ToString() == "Admin" || Session["User"].ToString() == "SuperAdmin")
            {
                if (nameOfImportedData == "Administratorzy" && Session["User"].ToString() == "SuperAdmin")
                {
                    List<StudentChoices.Models.Import.Admin> admins = (List<StudentChoices.Models.Import.Admin>)Session["Data"];
                    foreach (var admin in admins.Where(x => x.ToRemove == false))
                    {
                        if (db.Admins.Where(x => x.Login == admin.Login).Count() > 0)
                        {
                            var adminToEdit = db.Admins.Where(x => x.Login == admin.Login).FirstOrDefault();
                            adminToEdit.Password = admin.Password;
                            adminToEdit.Active = false;
                            db.Entry(adminToEdit).State = EntityState.Modified;
                        }
                        else
                        {
                            var newAdmin = new Admins();
                            newAdmin.Login = admin.Login;
                            newAdmin.Password = admin.Password;
                            newAdmin.Active = false;
                            newAdmin.SuperAdmin = false;
                            db.Admins.Add(newAdmin);
                        }
                        db.SaveChanges();
                    }
                }
                else if (nameOfImportedData == "Studenci")
                {
                    List<StudentChoices.Models.Import.Student.Student> students = (List<StudentChoices.Models.Import.Student.Student>)Session["Data"];
                    foreach (var student in students.Where(x => x.ToRemove == false))
                    {
                        if (db.Students.Where(x => x.StudentNo == student.StudentNo).Count() > 0)
                        {
                            var studentToEdit = db.Students.Where(x => x.StudentNo == student.StudentNo).FirstOrDefault();
                            studentToEdit.Login = student.Login;
                            studentToEdit.Password = student.Password;
                            studentToEdit.Name = student.Name;
                            studentToEdit.Surname = student.Surname;
                            studentToEdit.LastEditedBy = (int)Session["AdminID"];
                            studentToEdit.LastEdit = DateTime.Now;
                            db.Entry(studentToEdit).State = EntityState.Modified;
                        }
                        else
                        {
                            var newStudent = new Students();
                            newStudent.StudentNo = student.StudentNo;
                            newStudent.Login = student.Login;
                            newStudent.Password = student.Password;
                            newStudent.Name = student.Name;
                            newStudent.Surname = student.Surname;
                            newStudent.CreatedBy = (int)Session["AdminID"];
                            newStudent.CreationDate = DateTime.Now;
                            db.Students.Add(newStudent);
                        }
                        db.SaveChanges();
                        foreach (var classgroup in student.ClassGroup.Where(x => x.ToRemove == false))
                        {
                            if (classgroup.Speciality == "")
                            {
                                classgroup.Speciality = "-";
                            }
                            if (db.ClassGroups.Where(x => x.DegreeCourse == classgroup.DegreeCourse && x.Graduate == classgroup.Graduate && x.FullTimeStudies == classgroup.FullTimeStudies && x.Semester == classgroup.Semester && x.Speciality == classgroup.Speciality).Select(x => x.ClassGroupID).Count() == 0)
                            {
                                var newClassGroup = new ClassGroups();
                                newClassGroup.DegreeCourse = classgroup.DegreeCourse;
                                newClassGroup.Graduate = classgroup.Graduate;
                                newClassGroup.FullTimeStudies = classgroup.FullTimeStudies;
                                newClassGroup.Semester = classgroup.Semester;
                                newClassGroup.Speciality = classgroup.Speciality;
                                newClassGroup.CreatedBy = (int)Session["AdminID"];
                                newClassGroup.CreationDate = DateTime.Now;
                                db.ClassGroups.Add(newClassGroup);
                                db.SaveChanges();
                            }
                            int classGroupID = db.ClassGroups.Where(x => x.DegreeCourse == classgroup.DegreeCourse && x.Graduate == classgroup.Graduate && x.FullTimeStudies == classgroup.FullTimeStudies && x.Semester == classgroup.Semester && x.Speciality == classgroup.Speciality).Select(x => x.ClassGroupID).FirstOrDefault();
                            if (db.StudentsAndClassGroups.Where(x => x.StudentNo == student.StudentNo && classGroupID == x.ClassGroupID).Count() > 0)
                            {
                                var studentClassGroupToEdit = db.StudentsAndClassGroups.Where(x => x.StudentNo == student.StudentNo && classGroupID == x.ClassGroupID).FirstOrDefault();
                                studentClassGroupToEdit.AverageGrade = classgroup.AverageGrade;
                                studentClassGroupToEdit.LastEditedBy = (int)Session["AdminID"];
                                studentClassGroupToEdit.LastEdit = DateTime.Now;
                                db.Entry(studentClassGroupToEdit).State = EntityState.Modified;
                            }
                            else
                            {
                                var newStudentClassGroup = new StudentsAndClassGroups();
                                newStudentClassGroup.StudentNo = student.StudentNo;
                                newStudentClassGroup.AverageGrade = classgroup.AverageGrade;
                                newStudentClassGroup.CreatedBy = (int)Session["AdminID"];
                                newStudentClassGroup.CreationDate = DateTime.Now;
                                newStudentClassGroup.ClassGroupID = classGroupID;
                                db.StudentsAndClassGroups.Add(newStudentClassGroup);
                            }
                            db.SaveChanges();
                        }
                    }
                }
                else if (nameOfImportedData == "Przedmioty obieralne i specjelności")
                {
                    List<StudentChoices.Models.Import.Subject.ClassGroup> subjects = (List<StudentChoices.Models.Import.Subject.ClassGroup>)Session["Data"];
                    foreach (var classgroup in subjects.Where(x => x.ToRemove == false))
                    {
                        if (classgroup.Speciality == "")
                        {
                            classgroup.Speciality = "-";
                        }
                        int classGroupID = -1;
                        if (db.ClassGroups.Where(x => x.DegreeCourse == classgroup.DegreeCourse && x.Graduate == classgroup.Graduate && x.FullTimeStudies == classgroup.FullTimeStudies && x.Semester == classgroup.Semester && x.Speciality == classgroup.Speciality).Select(x => x.ClassGroupID).Count() > 0)
                        {
                            classGroupID = db.ClassGroups.Where(x => x.DegreeCourse == classgroup.DegreeCourse && x.Graduate == classgroup.Graduate && x.FullTimeStudies == classgroup.FullTimeStudies && x.Semester == classgroup.Semester && x.Speciality == classgroup.Speciality).Select(x => x.ClassGroupID).FirstOrDefault();
                        }
                        else
                        {
                            var newClassGroup = new ClassGroups();
                            newClassGroup.DegreeCourse = classgroup.DegreeCourse;
                            newClassGroup.Graduate = classgroup.Graduate;
                            newClassGroup.FullTimeStudies = classgroup.FullTimeStudies;
                            newClassGroup.Semester = classgroup.Semester;
                            newClassGroup.Speciality = classgroup.Speciality;
                            newClassGroup.CreatedBy = (int)Session["AdminID"];
                            newClassGroup.CreationDate = DateTime.Now;
                            db.ClassGroups.Add(newClassGroup);
                            db.SaveChanges();
                            classGroupID = newClassGroup.ClassGroupID;
                        }

                        foreach (var category in classgroup.Category.Where(x => x.ToRemove == false))
                        {
                            int categoryID = -1;
                            if (category.MaxNoChoices > category.ElectiveSubjectAndSpeciality.Count)
                            {
                                category.MaxNoChoices = category.ElectiveSubjectAndSpeciality.Count;
                            }
                            if (db.Categories.Where(x => x.Name == category.Name && classGroupID == x.ClassGroupID).Count() > 0)
                            {
                                var categoryToEdit = db.Categories.Where(x => x.Name == category.Name && classGroupID == x.ClassGroupID).FirstOrDefault();
                                categoryToEdit.Information = category.Information;
                                categoryToEdit.MaxNoChoices = category.MaxNoChoices;
                                categoryToEdit.LastEditedBy = (int)Session["AdminID"];
                                categoryToEdit.LastEdit = DateTime.Now;
                                db.Entry(categoryToEdit).State = EntityState.Modified;
                                db.SaveChanges();
                                categoryID = categoryToEdit.CategoryID;
                            }
                            else
                            {
                                var newCategory = new Categories();
                                newCategory.ClassGroupID = classGroupID;
                                newCategory.Name = category.Name;
                                newCategory.Information = category.Information;
                                newCategory.MaxNoChoices = category.MaxNoChoices;
                                newCategory.CreatedBy = (int)Session["AdminID"];
                                newCategory.CreationDate = DateTime.Now;
                                db.Categories.Add(newCategory);
                                db.SaveChanges();
                                categoryID = newCategory.CategoryID;
                            }

                            foreach (var subject in category.ElectiveSubjectAndSpeciality.Where(x => x.ToRemove == false))
                            {
                                int subjectID = -1;
                                if (subject.UpperLimit != "" && subject.LowerLimit != "")
                                {
                                    if (Int16.Parse(subject.LowerLimit) > Int16.Parse(subject.UpperLimit))
                                    {
                                        subject.LowerLimit = subject.UpperLimit;
                                    }
                                }
                                if (db.ElectiveSubjectsAndSpecialities.Where(x => x.Name == subject.Name && categoryID == x.CategoryID).Count() > 0)
                                {
                                    var subjectToEdit = db.ElectiveSubjectsAndSpecialities.Where(x => x.Name == subject.Name && categoryID == x.CategoryID).FirstOrDefault();
                                    subjectToEdit.Name = subject.Name;
                                    subjectToEdit.Information = subject.Information;
                                    if (subject.UpperLimit != "")
                                    {
                                        subjectToEdit.UpperLimit = Int16.Parse(subject.UpperLimit);
                                    }
                                    if (subject.LowerLimit != "")
                                    {
                                        subjectToEdit.LowerLimit = Int16.Parse(subject.LowerLimit);
                                    }
                                    subjectToEdit.LastEditedBy = (int)Session["AdminID"];
                                    subjectToEdit.LastEdit = DateTime.Now;
                                    db.Entry(subjectToEdit).State = EntityState.Modified;
                                    db.SaveChanges();
                                    subjectID = subjectToEdit.ElectiveSubjectAndSpecialityID;
                                }
                                else
                                {
                                    var newSubject = new ElectiveSubjectsAndSpecialities();
                                    newSubject.Name = subject.Name;
                                    newSubject.CategoryID = categoryID;
                                    newSubject.Information = subject.Information;
                                    if (subject.UpperLimit != "")
                                    {
                                        newSubject.UpperLimit = Int16.Parse(subject.UpperLimit);
                                    }
                                    if (subject.LowerLimit != "")
                                    {
                                        newSubject.LowerLimit = Int16.Parse(subject.LowerLimit);
                                    }
                                    newSubject.CreatedBy = (int)Session["AdminID"];
                                    newSubject.CreationDate = DateTime.Now;
                                    db.ElectiveSubjectsAndSpecialities.Add(newSubject);
                                    db.SaveChanges();
                                    subjectID = newSubject.ElectiveSubjectAndSpecialityID;
                                }

                                foreach (var file in subject.Files.Where(x => x.ToRemove == false))
                                {
                                    if (db.Files.Where(x => x.Filename == file.Filename && subjectID == x.ElectiveSubjectAndSpecialityID).Count() > 0)
                                    {
                                        var fileToEdit = db.Files.Where(x => x.Filename == file.Filename && subjectID == x.ElectiveSubjectAndSpecialityID).FirstOrDefault();
                                        fileToEdit.Path = file.Path;
                                        fileToEdit.LastEditedBy = (int)Session["AdminID"];
                                        fileToEdit.LastEdit = DateTime.Now;
                                        db.Entry(fileToEdit).State = EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        var newFile = new Files();
                                        newFile.Filename = file.Filename;
                                        newFile.ElectiveSubjectAndSpecialityID = subjectID;
                                        newFile.Path = file.Path;
                                        newFile.CreatedBy = (int)Session["AdminID"];
                                        newFile.CreationDate = DateTime.Now;
                                        db.Files.Add(newFile);
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                return RedirectToAction("", "Home");
            }
            Session["Data"] = null;
            TempData["Success"] = "Zaimportowane dane (" + nameOfImportedData + ") zapisano w bazie danych pomyślnie!";
            return RedirectToAction("", "Home");
        }


    }
}
