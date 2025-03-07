using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Juster_Project.Models;
using System.Net;
using System.IO;
using System.Data;

namespace Juster_Project.Controllers
{
    public class HomeController : Controller
    {
        //Connection Strings
        private readonly string contection = ConfigurationManager.ConnectionStrings["connectionStrings"].ConnectionString;


        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(user_data userdata)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(contection))
                {
                    string query = "insert into user_data (email,password) values (@email,@password )";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", userdata.email);
                        cmd.Parameters.AddWithValue("@password", userdata.password);
                        conn.Open();
                        cmd.ExecuteNonQuery();

                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View();
            }

        }


       public ActionResult Index()
        {
            return View();
        }

        //[HttpPost]
        //public async Task<ActionResult> Index(user_data data, string email)
        //{
        //    Session["email"] = email;

        //    string holder=Session["email"].ToString();


        //        if (holder == data.email)
        //        {
        //            //Generate Random otp
        //            Random random = new Random();
        //            string otp = random.Next(100000, 999999).ToString();

        //            //SqlConnection 
        //            using (SqlConnection conn = new SqlConnection(contection))
        //            {

        //                //Set Query
        //                string query = "UPDATE user_data SET otp = @otp WHERE email = @email";
        //                using (SqlCommand cmd = new SqlCommand(query, conn))
        //                {
        //                    cmd.Parameters.AddWithValue("@otp", otp);
        //                    cmd.Parameters.AddWithValue("@email", email);
        //                    conn.Open();
        //                    await cmd.ExecuteNonQueryAsync();
        //                }

        //            }



        //            //Sending email Process
        //            string senderEmail = "Rakesh1214@barrownz.com";
        //            string password = "jcpr bope hlcv mizb";
        //            string smtpServer = "smtp.gmail.com";

        //            MailMessage mail = new MailMessage
        //            {
        //                From = new MailAddress(senderEmail),
        //                Subject = "Dear User, Your OTP",
        //                Body = $"Dear User,\n\nYour login OTP is: {otp}\n\n!",
        //                IsBodyHtml = false
        //            };

        //            mail.To.Add(email);

        //            SmtpClient smtp = new SmtpClient
        //            {
        //                Host = smtpServer,
        //                Port = 587,
        //                Credentials = new NetworkCredential(senderEmail, password),
        //                EnableSsl = true
        //            };

        //            await smtp.SendMailAsync(mail);
        //            return RedirectToAction("otp_verificatin");

        //        }

        //        else
        //        {
        //            TempData["msg"] = "This email not exist in database";
        //            return RedirectToAction("Index");


        //        }

        //}

        [HttpPost]
        public async Task<ActionResult> Index(string email)
        {
            string enteredEmail = email;
            using (SqlConnection conn = new SqlConnection(contection))
            {
                string query = "SELECT COUNT(*) FROM user_data WHERE email = @email";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@email", enteredEmail);
                    conn.Open();
                    int count = (int)await cmd.ExecuteScalarAsync(); 

                    if (count == 0)
                    {
                        TempData["msg"] = "This email does not exist in database";
                        return RedirectToAction("Index");
                    }
                }
            }

            // Generate OTP
            Random random = new Random();
            string otp = random.Next(100000, 999999).ToString();

            // Update OTP in database
            using (SqlConnection conn = new SqlConnection(contection))
            {
                string query = "UPDATE user_data SET otp = @otp WHERE email = @Email";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@otp", otp);
                    cmd.Parameters.AddWithValue("@Email", enteredEmail);
                    conn.Open();
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            // Sending email process
            string senderEmail = "Rakesh1214@barrownz.com";
            string password = "jcpr bope hlcv mizb";
            string smtpServer = "smtp.gmail.com";

            MailMessage mail = new MailMessage
            {
                From = new MailAddress(senderEmail),
                Subject = "Dear User, Your OTP",
                Body = $"Dear User,\n\nYour login OTP is: {otp}\n\n!",
                IsBodyHtml = false
            };

            mail.To.Add(enteredEmail);

            SmtpClient smtp = new SmtpClient
            {
                Host = smtpServer,
                Port = 587,
                Credentials = new NetworkCredential(senderEmail, password),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mail);

            return RedirectToAction("otp_verificatin");
        }


        public ActionResult otp_verificatin()
        {
            return View();
        }


        //verification Process
        [HttpPost]
        public ActionResult verification(user_data data)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(contection))
                {
                    string Query ="SELECT *from user_data Where otp=@otp";
                    using(SqlCommand cmd = new SqlCommand(Query, conn))
                    {
                        cmd.Parameters.AddWithValue("@otp", data.otp);
                        conn.Open();
                        using(SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while(reader.Read())
                            {
                                Session["UserLogin"]=true;
                                Session["otp"]=data.otp;
                            }
                        }
                    }

                    TempData["msg"] ="Successfully Login";
                    return RedirectToAction("dashboard"); 
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error=ex.Message;
                return RedirectToAction("dashboard");
            }       
        }

        public ActionResult dashboard()
        {
            if (Session["UserLogin"] == null)
            {
                return RedirectToAction("otp_verificatin");
            }
            return View();
        }
        
        public ActionResult Logout()
        {
            try
            {
                string otp = (string)Session["otp"];

                if (!string.IsNullOrEmpty(otp))
                {
                    using (SqlConnection conn = new SqlConnection(contection))
                    {
                        string Query ="UPDATE user_data SET otp = NULL WHERE otp =@otp ";
                        using (SqlCommand cmd = new SqlCommand(Query, conn))
                        {
                            cmd.Parameters.AddWithValue("@otp",otp);
                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }

                    }
                }

                Session.Clear();
                Session.Abandon();

                TempData["msg"] ="Logout Successfully";
                return RedirectToAction("otp_verificatin");

            }
            catch (Exception ex) 
                {
                    ViewBag.ErrorLog = ex.Message;
                    return View();
                }

         }
        


        public ActionResult Multiple_image()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Multiple_Image(IEnumerable<HttpPostedFileBase> image)
        {
            if(Session["UserLogin"] == null)
            {
                return RedirectToAction("otp_verificatin");
            }

            if (image != null)
            {
                string folder_path = Server.MapPath("~/Content/Upload_Image");

                if (!Directory.Exists(folder_path))
                {
                    Directory.CreateDirectory(folder_path);
                }

                List<string> filepaths = new List<string>();

                foreach (var file in image)
                {
                    string filename = file.FileName;
                    string filepath = Path.Combine(folder_path, filename);

                    file.SaveAs(filepath);
                    filepaths.Add(filename);


                    using (SqlConnection con = new SqlConnection(contection))
                    {
                        string Query ="Insert into Multiple_image (image) values(@image)";
                        using (SqlCommand cmd = new SqlCommand(Query, con))
                        {
                            cmd.Parameters.AddWithValue("@image", filename);
                            con.Open();
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }

                }

            }
            TempData["msg"] = "Image insert";
            return RedirectToAction("Multiple_image");

        }

        //[HttpPost]
        //public async Task<ActionResult> Multiple_image(IEnumerable<HttpPostedFileBase> image)
        //{
        //    if(image != null)
        //    {
        //        string folder_path = Server.MapPath("~/Content/Upload_image");

        //        if(!Directory.Exists(folder_path))
        //        {
        //            Directory.CreateDirectory(folder_path);
        //        }

        //        List<string> files_Paths = new List<string>();    
        //        foreach(var file in image)
        //        {
        //            string filename=file.FileName;
        //            string filepath = Path.Combine(folder_path, filename);

        //            file.SaveAs(filepath);
        //            files_Paths.Add(filename);

        //        }

        //        await  BulkInsertImages(files_Paths);

        //    }
        //    TempData["msg"] = "Image Successfully insert";
        //    return RedirectToAction("Multiple_image");
        //}


        //private async Task BulkInsertImages(List<string> imagePaths)
        //{

        //    DataTable dataTable = new DataTable();
        //    dataTable.Columns.Add("image",typeof(string));

        //    foreach(var image in imagePaths)
        //    {
        //        dataTable.Rows.Add(image);
        //    }

        //    using (SqlConnection con = new SqlConnection(contection))
        //    {
        //        await con.OpenAsync();
        //        using(SqlBulkCopy bulkCopy=new SqlBulkCopy(con))
        //        {
        //            bulkCopy.DestinationTableName = "Multiple_image";
        //            bulkCopy.ColumnMappings.Add("image", "image");
        //            await bulkCopy.WriteToServerAsync(dataTable);
        //        }
        //    }
        //}
    
        public ActionResult show()
        {
            List<image_data> list = new List<image_data>();

            using (SqlConnection con = new SqlConnection(contection))
            {
                string query = "select *from Multiple_image";

                using(SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();

                    using(SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new image_data()
                            {
                                id = Convert.ToInt32(reader["ID"]),
                                image = reader["image"].ToString(),

                            });
                        }
                    }
                }
            }
          return View(list);
        }

        public ActionResult delete(int id)
        {

            try
            {
                string folder_path = Server.MapPath("~/Content/Upload_Image/");
                string filename = string.Empty;

                using (SqlConnection con = new SqlConnection(contection))
                {
                    string query = "select image from Multiple_image where id=@id";
                    using( SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        con.Open();
                       filename=(string)cmd.ExecuteScalar();
                    }
                }

                string filepath = Path.Combine(folder_path,filename);
                if (System.IO.File.Exists(filepath))
                {
                    System.IO.File.Delete(filepath);
                }

                using(SqlConnection con = new SqlConnection(contection))
                {
                    string query = "Delete from Multiple_image where id=@id";
                    using(SqlCommand cdm=new SqlCommand(query, con))
                    {
                        cdm.Parameters.AddWithValue("@id", id);
                        con.Open();
                        cdm.ExecuteNonQuery();
                    }
                }
                TempData["msg"] = "Delete image Successfully";
                return RedirectToAction("show");

            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View();
            }
        }


        [HttpGet]
        public ActionResult edit_page(int id)
        {
            image_data data = new image_data();

                using (SqlConnection con = new SqlConnection(contection))
                {
                    string query = "Select *from Multiple_image where id=@id";
                    using(SqlCommand cmd=new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        con.Open ();
                        using(SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            if (rdr.Read())
                            {
                                data.id=Convert.ToInt32(rdr["ID"]);
                                data.image=rdr["image"].ToString();
                            }
                        }
                    }
                }
             return View(data);
        }

        [HttpPost]
        public ActionResult Update_image(image_data data, HttpPostedFileBase image)
        {
            string folder_path = Server.MapPath("~/Content/Upload_Image/");
            string filename=image.FileName;
            string filepath=Path.Combine(folder_path, filename);

            if (System.IO.File.Exists(filepath))
            {
                System.IO.File.Delete(filepath);
            }

            data.image=filename;
            image.SaveAs(filepath);

            using (SqlConnection con = new SqlConnection(contection))
            {
                string query = "Update Multiple_image set image=@image where id=@id";
                using( SqlCommand cmd=new SqlCommand(query,con))
                {
                    cmd.Parameters.AddWithValue("@image",filename);
                    cmd.Parameters.AddWithValue("@id",data.id);
                    con.Open ();
                    cmd.ExecuteNonQuery();
                }
            }
            TempData["msg"] = "Update successfully";
            return RedirectToAction("show");
        }

        [HttpGet]
        public ActionResult User_list()
        {
            LinkedList<user_data> list = new LinkedList<user_data>();

            try
            {
                using (SqlConnection con = new SqlConnection(contection))
                {
                    string query = "Select *from user_data";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        con.Open();
                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                list.AddFirst(new user_data()
                                {
                                    Id = Convert.ToInt32(rdr["ID"]),
                                    otp = rdr["otp"].ToString(),
                                    email = rdr["email"].ToString(),
                                    password = rdr["password"].ToString()

                                });
                            }
                        }
                    }
                }
                
                return View(list);
            }
            catch (Exception ex)
            {
                {
                    ViewBag.ErrorMessage = ex.Message;
                    return View();
                }


            }
        }


        public ActionResult remove(int id)
        {
            if (Session["otp"]==null)
            {
                return RedirectToAction("Index");
            }
            using (SqlConnection con = new SqlConnection(contection))
            {
                string query = "Delete from user_data where id=@id";
                using(SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            return RedirectToAction("User_list");
        }
    }
}

