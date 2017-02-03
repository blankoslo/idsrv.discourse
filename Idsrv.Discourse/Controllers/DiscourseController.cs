using System.Web.Mvc;

namespace Idsrv.Discourse.Controllers
{
    [RoutePrefix("/oauth2")]
    public class DiscourseController : Controller
    {

        [Route("/discourse")]
        public ActionResult Index()
        {
            return View();
        }
    }
}