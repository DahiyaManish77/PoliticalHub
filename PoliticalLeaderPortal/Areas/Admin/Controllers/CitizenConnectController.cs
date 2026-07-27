using PoliticalLeaderPortal.Areas.Admin.Infrastructure;
using PoliticalLeaderPortal.Services;
using System;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    [RoleMenuAuthorize]
    public class CitizenConnectController : Controller
    {
        private readonly CitizenConnectService service = new CitizenConnectService();

        public ActionResult Index(string requestType = null, string status = null, string keyword = null)
        {
            ViewBag.RequestType = requestType;
            ViewBag.Status = status;
            ViewBag.Keyword = keyword;
            return View(service.GetAll(requestType, status, keyword));
        }

        public ActionResult Details(int id)
        {
            var model = service.GetById(id);
            if (model == null) return HttpNotFound();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string status, string adminRemarks)
        {
            try
            {
                bool updated = service.UpdateStatus(id, status, adminRemarks);
                TempData[updated ? "SuccessMessage" : "ErrorMessage"] = updated
                    ? "Request status updated successfully."
                    : "Request was not found.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction("Details", new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConvertVolunteer(int id)
        {
            try
            {
                int personId = service.ConvertVolunteerToPerson(id, CurrentUserId());
                TempData["SuccessMessage"] = "Volunteer application converted to People Master successfully.";
                return RedirectToAction("Edit", "People", new { area = "Admin", id = personId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Details", new { id = id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Delete(int id)
        {
            try
            {
                bool success = service.Delete(id);
                return Json(new { success = success, message = success ? "Request deleted successfully." : "Request not found." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private int CurrentUserId()
        {
            int id;
            return Int32.TryParse(Convert.ToString(Session["UserId"]), out id) ? id : 0;
        }
    }
}
