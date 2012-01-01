using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DietJournal.Web.Models;

namespace DietJournal.Web.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Journal");
            }
            
            return View();
        }

        public ActionResult Donate()
        {
            return View();
        }

        public ActionResult Feedback()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Feedback(FeedbackModel model)
        {
            if (ModelState.IsValid)
            {
                using (var smtp = new System.Net.Mail.SmtpClient())
                {
                    var message = new System.Net.Mail.MailMessage(!String.IsNullOrEmpty(model.EmailAddress) ? model.EmailAddress : "support@fastdietjournal.com", "support@fastdietjournal.com");
                    message.Subject = "Feedback Comment";
                    message.Body = model.Comments;
                    smtp.Send(message);
                }
            }

            return View();
        }

        public ActionResult Privacy()
        {
            return View();
        }

        public ActionResult Conditions()
        {
            return View();
        }
    }
}
