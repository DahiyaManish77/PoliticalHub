using PoliticalLeaderPortal.Areas.Admin.Infrastructure;
using PoliticalLeaderPortal.Services;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    [RoleMenuAuthorize]
    public class CitizenConnectController : Controller
    {
        private readonly CitizenConnectService service;

        public CitizenConnectController()
        {
            service = new CitizenConnectService();
        }

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
            service.UpdateStatus(id, status, adminRemarks);
            TempData["SuccessMessage"] = "Request status updated successfully.";
            return RedirectToAction("Details", new { id = id });
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            bool success = service.Delete(id);
            return Json(new { success = success, message = success ? "Request deleted successfully." : "Request not found." });
        }
    }
}
