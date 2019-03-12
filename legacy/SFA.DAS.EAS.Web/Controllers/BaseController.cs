using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using SFA.DAS.Authentication;
using SFA.DAS.EAS.Domain.Interfaces;
using SFA.DAS.EAS.Web.Helpers;
using SFA.DAS.EAS.Web.ViewModels;
using SFA.DAS.Validation;

namespace SFA.DAS.EAS.Web.Controllers
{
    public class BaseController : Controller
    {
        public IAuthenticationService OwinWrapper;

        private const string FlashMessageCookieName = "sfa-das-employerapprenticeshipsservice-flashmessage";

        private readonly ICookieStorageService<FlashMessageViewModel> _flashMessage;

        public BaseController(IAuthenticationService owinWrapper, ICookieStorageService<FlashMessageViewModel> flashMessage)
        {
            OwinWrapper = owinWrapper;
            _flashMessage = flashMessage;
        }

        protected override ViewResult View(string viewName, string masterName, object model)
        {
            var orchestratorResponse = model as OrchestratorResponse;

            if (orchestratorResponse == null)
            {
                return base.View(viewName, masterName, model);
            }

            var invalidRequestException = orchestratorResponse.Exception as InvalidRequestException;

            if (invalidRequestException != null)
            {
                foreach (var errorMessageItem in invalidRequestException.ErrorMessages)
                {
                    ModelState.AddModelError(errorMessageItem.Key, errorMessageItem.Value);
                }

                return ReturnViewResult(viewName, masterName, orchestratorResponse);
            }

            if (orchestratorResponse.Status == HttpStatusCode.BadRequest)
            {
                return ReturnViewResult(viewName, masterName, orchestratorResponse);
            }

            if (orchestratorResponse.Status == HttpStatusCode.NotFound)
            {
                return base.View(ControllerConstants.NotFoundViewName);
            }

            if (orchestratorResponse.Status == HttpStatusCode.OK)
            {
                return ReturnViewResult(viewName, masterName, orchestratorResponse);
            }

            if (orchestratorResponse.Status == HttpStatusCode.Unauthorized)
            {
                var accountId = Request.Params[ControllerConstants.AccountHashedIdRouteKeyName];

                if (accountId != null)
                {
                    ViewBag.AccountId = accountId;
                }

                return base.View(ControllerConstants.AccessDeniedViewName, masterName, orchestratorResponse);
            }

            if (orchestratorResponse.Exception != null)
            {
                throw orchestratorResponse.Exception;
            }

            throw new Exception($"Orchestrator response of type '{model.GetType()}' could not be handled.");
        }

        private ViewResult ReturnViewResult(string viewName, string masterName, OrchestratorResponse orchestratorResponse)
        {
            return base.View(viewName, masterName, orchestratorResponse);
        }

        public void AddFlashMessageToCookie(FlashMessageViewModel model)
        {
            _flashMessage.Delete(FlashMessageCookieName);

            _flashMessage.Create(model, FlashMessageCookieName);
        }

        public FlashMessageViewModel GetFlashMessageViewModelFromCookie()
        {
            var flashMessageViewModelFromCookie = _flashMessage.Get(FlashMessageCookieName);
            _flashMessage.Delete(FlashMessageCookieName);
            return flashMessageViewModelFromCookie;
        }

        public void RemoveFlashMessageFromCookie()
        {
            _flashMessage.Delete(FlashMessageCookieName);
        }
    }
}