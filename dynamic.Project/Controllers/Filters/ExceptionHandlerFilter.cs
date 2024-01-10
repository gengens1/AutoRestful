using dynamic.Project.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace dynamic.Project.Controllers.Filters
{
    public class ExceptionHandlerFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            context.Result =  new BadRequestObjectResult(ApiResult.Fail(context.Exception.Message));
        }
    }
}
