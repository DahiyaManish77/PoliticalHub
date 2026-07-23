using PoliticalLeaderPortal.Services;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Controllers
{
    public class GalleryController : Controller
    {
        private readonly GalleryService _galleryService;

        public GalleryController()
        {
            _galleryService = new GalleryService();
        }

        public ActionResult Index()
        {
            var model = _galleryService.GetPublicGalleryCategories();

            return View(model);
        }

        public ActionResult Album(int id)
        {
            var model = _galleryService.GetPublicGalleryAlbum(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }
    }
}