using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using VCM.Models;
using System.Web.Security;

namespace VCM.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        [HttpGet]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        [HttpPost]
        public ViewResult Contact(string email, string button, ContactResponse contactResponse)
        {
            if (ModelState.IsValid)
            {
                // Email to itself
                var fromEmail = new MailAddress("vetcycle@gmail.com", "Vet Cycle Minnesota");
                // Send email to vetcycle
                var toEmail = new MailAddress("vetcycle@gmail.com");
                // Email password
                var fromEmailPassword = "V3tCycle1!";
                // Generate the subject in the email
                string subject = contactResponse.Subject;
                //Body in the email
                string body = DateTime.Now +
                    "<br />" + "<br />" +
                    "You got a new message from," +
                    "<br />" + "<br />" +
                    "------------------------------------------------" +
                    "<br />" + "<br />" +
                    "First and Last Name: " + contactResponse.Name +
                    "<br />" + "<br />" +
                    "Email: " + contactResponse.Email +
                    "<br />" + "<br />" +
                    "Phone: " + contactResponse.Phone +
                    "<br />" + "<br />" +
                    "Subject: " + contactResponse.Subject +
                    "<br />" + "<br />" +
                    "Message: " +
                    "<br />" +
                    "<br />" +
                    contactResponse.Message +
                    "<br />" + "<br />" +
                    "------------------------------------------------";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
                };

                using (var message = new MailMessage(fromEmail, toEmail)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                    smtp.Send(message);

                // If all fields are input correctly, alert pops up and clear previous input

                // Pop up message
                TempData["message"] = "Thank you, " + contactResponse.Name + " someone will contact you within 48 hours!";
                ModelState.Clear();
                return View();
            }
            else if (button == "Clear")  //I can see code is executed here, I need to clear all UI fields starts from there.
            {
                //here, I need to clear all Input text fields, text, listbox, dropdownlist, etc.   
                ModelState.Clear();
                return View(); //this will return the original UI razor view,

            }
            else
            {
                return View();
            }
        }

        public ActionResult Events()
        {
            ViewBag.Message = "Your event page.";
            return View();
        }

        public ActionResult WeeklyRides()
        {
            ViewBag.Message = "Weekly Rides Go HERE.";
            return View();
        }

        public ActionResult CommunityRides()
        {
            ViewBag.Message = "Community Rides Go Here.";
            return View();
        }
        //Registration Action
        public ActionResult Registration()
        {
            ViewBag.Message = "Register yo self.";
            return View();
        }


        //Registration POST Action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Exclude = "IsEmailVerified,ActivationCode")] User user)
        {
            bool Status = false;
            string message = "";
            //
            // Model Validation 
            if (ModelState.IsValid)
            {

                #region //Email is already Exist 
                var isExist = IsEmailExist(user.Email);
                if (isExist)
                {
                    ModelState.AddModelError("EmailExist", "Email already exists");
                    return View(user);
                }
                #endregion

                #region Generate Activation Code 
                user.ActivationCode = Guid.NewGuid();
                #endregion

                #region  Password Hashing 
                user.Password = Crypto.Hash(user.Password);
                user.ConfirmPassword = Crypto.Hash(user.ConfirmPassword); //
                #endregion
                user.IsEmailVerified = false;

                #region Save to Database
                using (MyDatabaseEntities dc = new MyDatabaseEntities())
                {
                    dc.Users.Add(user);
                    dc.SaveChanges();

                    //Send Email to User
                    SendVerificationLinkEmail(user.Email, user.ActivationCode.ToString());
                    message = " Thank you for registering with Vet Cycle Minnesota! We have sent a confirmation email to " + user.Email + ".";
                    Status = true;
                }
                #endregion

            }
            else
            {
                message = "Invalid Request";
            }

            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View(user);
        }

        //Verify Email
        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            bool Status = false;
            using (MyDatabaseEntities dc = new MyDatabaseEntities())
            {
                //this line is to avoid confirm password does not match issue on save changes
                dc.Configuration.ValidateOnSaveEnabled = false;
                var v = dc.Users.Where(a => a.ActivationCode == new Guid(id)).FirstOrDefault();
                if (v != null)
                {
                    v.IsEmailVerified = true;
                    dc.SaveChanges();
                    Status = true;

                }
                else
                {
                    ViewBag.Message = "Invalid Request";
                }
            }
            ViewBag.Status = Status;
            return View();

        }


        //login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        //Login POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLogin login, string ReturnUrl)
        {
            string message = "";
            using (MyDatabaseEntities dc = new MyDatabaseEntities())
            {
                var v = dc.Users.Where(a => a.Email == login.Email).FirstOrDefault();
                if (v != null)
                {
                    if (string.Compare(Crypto.Hash(login.Password), v.Password) == 0)
                    {
                        int timeout = login.RememberMe ? 525600 : 20; //525600 min = 1 year
                        var ticket = new FormsAuthenticationTicket(login.Email, login.RememberMe, timeout);
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                        cookie.Expires = DateTime.Now.AddMinutes(timeout);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);


                        if (Url.IsLocalUrl(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        message = "Invalid credentials provided";
                    }

                }
                else
                {
                    message = "Invalid credentials provided";
                }
            }

            ViewBag.Message = message;
            return View();
        }

        //logout
        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "User");
        }

        [NonAction]
        public bool IsEmailExist(string email)
        {
            using (MyDatabaseEntities dc = new MyDatabaseEntities())
            {
                var v = dc.Users.Where(a => a.Email == email).FirstOrDefault();
                return v != null;
            }
        }

        [NonAction]
        public void SendVerificationLinkEmail(string email, string activationCode)
        {
            var verifyUrl = "/User/VerifyAccount/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("vetcycle@gmail.com", "Vet Cycle Minnesota");
            var toEmail = new MailAddress(email);
            //Fake email I created: VetCycle@gmail.com Password: V3TCycle1!
            var fromEmailPassword = "V3tCycle1!"; // Replace with actual password
            //Message that will be displayed in users email
            string subject = "Your account is successfully created!";
            //body that will show in confirmation email to user
            string body = "<br/><br/>Thank you for registering with Vet Cycle Minnesota! " +
                " has been successfully created. Please click on the below link to verify your account" +
                " <br/><br/><a href='" + link + "'>" + link + "</a> ";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }
    }
}