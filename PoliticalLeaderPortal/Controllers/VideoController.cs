using PoliticalLeaderPortal.Services;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Controllers
{
    public class VideoController : Controller
    {
        private readonly VideoGalleryService _videoService;

        public VideoController()
        {
            _videoService = new VideoGalleryService();
        }

        public ActionResult Index()
        {
            var model =
                _videoService.GetPublicVideos();

            return View(model);
        }

        public ActionResult Details(int id)
        {
            var model =
                _videoService.GetPublicVideo(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }
    }
}