using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FinancialFilesMergingPortal.Models;
using System.Data.Entity;
using System.Data.SqlClient;


namespace FinancialFilesMergingPortal.Controllers
{
    public class UserController : Controller
    {
        //
        // GET: /User/
        [HttpGet]
        public ActionResult AddorEdit(int id = 0)
        {
            User userModel = new User();

            return View(userModel);
        }

        [HttpPost]
        public ActionResult AddorEdit(User userModel)
        {
            if (ModelState.IsValid)
            {
                var u = new User { };
                u.Username = userModel.Username;
                u.Password = u.CreatePasswordHash(userModel.Password);
                u.Email = userModel.Email;

                SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\Galileo\documents\visual studio 2013\Projects\FinancialFilesMergingPortal\FinancialFilesMergingPortal\App_Data\DatabaseForTask.mdf;Integrated Security=True;MultipleActiveResultSets=True;Application Name=EntityFramework");
                con.Open();
                SqlCommand cmd = new SqlCommand(@"insert into Users VALUES (@un, @ps, @em);", con);
                cmd.Parameters.Add("@un", u.Username);
                cmd.Parameters.Add("@ps", u.Password);
                cmd.Parameters.Add("@em", u.Email);
                cmd.ExecuteNonQuery();
                //SqlDataAdapter da;
                con.Close();

                //userModel.Password = userModel.CreatePasswordHash(userModel.Password);
                ModelState.Clear();
                ViewBag.SuccessMessage = "Registration Successflly";
            }

            return View("AddorEdit", new User());
        }
        [HttpGet]
        public ActionResult Login(int id = 0)
        {
            User userModel = new User();

            return View(userModel);
        }

        [HttpPost]
        public ActionResult Login(User userModel)
        {
            SqlCommand cmd;
            if (ModelState.IsValid)
            {
                var u = new User { };
                u.Username = userModel.Username;
                u.Password = u.CreatePasswordHash(userModel.Password);

                try
                {
                    SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\Galileo\documents\visual studio 2013\Projects\FinancialFilesMergingPortal\FinancialFilesMergingPortal\App_Data\DatabaseForTask.mdf;
                                                            Integrated Security=True;MultipleActiveResultSets=True;Application Name=EntityFramework");
                    con.Open();
                    //SqlCommand cmd = new SqlCommand(@"insert into Users VALUES (@un, @ps, @em);", con);
                    cmd = new SqlCommand(@"select Password from Users where Username = '" + u.Username + "';", con);

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string pass = reader["Password"].ToString();
                            if(pass.Equals(u.Password))
                            {
                                TempData["userName"] = u.Username;
                                return RedirectToAction("Upload", "Files");
                            }
                        }


                    }
                }
                catch(Exception e)
                {
                }

                //userModel.Password = userModel.CreatePasswordHash(userModel.Password);
                ModelState.Clear();
            }

            return View("Login", new User());
        }
	}
}