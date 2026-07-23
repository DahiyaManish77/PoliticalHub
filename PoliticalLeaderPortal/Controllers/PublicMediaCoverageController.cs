using PoliticalLeaderPortal.Areas.Admin.Services;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Controllers
{
    public class PublicMediaCoverageController : Controller
    {
        private readonly MediaCoverageService _service;

        public PublicMediaCoverageController()
        {
            _service = new MediaCoverageService();
        }

        public ActionResult Index()
        {
            var model =
                _service.GetActiveMediaCoverage();

            return View(model);
        }

        public ActionResult Details(int id)
        {
            _service.IncreaseViewCount(id);

            var model =
                _service.GetPublicDetails(id);

            if (model == null)
            {
                return RedirectToAction(
                    "Index");
            }

            ViewBag.RelatedMediaCoverage =
                _service.GetRelatedMediaCoverage(id);

            return View(model);
        }
    }
}