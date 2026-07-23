using PoliticalLeaderPortal.Areas.Admin.Services;
using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Controllers
{
    public class DownloadsController : Controller
    {
        private readonly DownloadDocumentService _service;

        public DownloadsController()
        {
            _service =
                new DownloadDocumentService();
        }

        public ActionResult Index()
        {
            return View(
                _service.GetActive());
        }

        public ActionResult Category(int id)
        {
            ViewBag.CategoryId = id;

            return View(
                "Index",
                _service.GetByCategory(id));
        }

        public ActionResult Download(int id)
        {
            DownloadDocumentVM document =
                _service.GetActive()
                .FirstOrDefault(x =>
                    x.DownloadDocumentId == id);

            if (document == null)
            {
                return HttpNotFound();
            }

            if (System.Uri.IsWellFormedUriString(document.FilePath, System.UriKind.Absolute))
            {
                _service.IncrementDownload(id);
                return Redirect(document.FilePath);
            }

            string physicalPath =
                Server.MapPath(document.FilePath);

            if (!System.IO.File.Exists(physicalPath))
            {
                return HttpNotFound();
            }

            _service.IncrementDownload(id);

            return File(
                physicalPath,
                MimeMapping.GetMimeMapping(physicalPath),
                document.FileName);
        }
    }
}
