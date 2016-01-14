using System.Web.Mvc;

namespace Ui.Controllers
{
    using System;

    using Messages.Commands;

    using NServiceBus;

    public class HomeController : Controller
    {
        private readonly ISendOnlyBus sendOnlyBus;

        public HomeController(ISendOnlyBus sendOnlyBus)
        {
            this.sendOnlyBus = sendOnlyBus;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public RedirectToRouteResult SubmitOnlineAgreement()
        {
            SendCommand(456);
            return RedirectToAction("Complete");
        }

        [HttpPost]
        public RedirectToRouteResult SubmitRemoteAgreement()
        {
            SendCommand(123);
            return RedirectToAction("Complete");
        }

        private void SendCommand(int brokerId)
        {
            sendOnlyBus.Send(
                new SubmitAgreementCommand
                    {
                    CorrelationId = Guid.NewGuid(),
                        BrokerId = brokerId,
                        AgreementDocumentUrl =
                            "https://upload.wikimedia.org/wikipedia/en/f/f4/The_Best_Best_of_Fela_Kuti.jpg"
                    });
        }

        [HttpGet]
        public ViewResult Complete()
        {
            return View();
        }        
    }
}