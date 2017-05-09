namespace Step1.Controllers
{
    using System;
    using System.Web.Http;
    using Step1.API;

    public class IssuesController : ApiController
    {
        [HttpPost]
        public IHttpActionResult Post(IssueDTO issue)
        {
            // do something with issue
            Console.WriteLine("Issue accepted: category:" + issue.Category + " severity:" + issue.Severity + " description:" + issue.Description);

            return this.Ok();
        }
    }
}
