using System;
using System.Linq.Expressions;
using System.Web.Http;

namespace KafkaService
{
    public static class MapHttpRouteCollectionExtentions
    {
        public static void AddRoute<TColtroller>(this HttpRouteCollection routes, string routeTemplate, Expression<Action<TColtroller>> expression)
            where TColtroller : ApiController
        {
            var type = typeof(TColtroller);
            var methodName = expression.Name
                             ?? ((MethodCallExpression) expression.Body).Method.Name;
            const string controllerSuffix = "Controller";
            var controllerName = type.Name.EndsWith(controllerSuffix)
                ? type.Name.Substring(0, type.Name.Length - controllerSuffix.Length)
                : type.Name;

            routes.MapHttpRoute(routeTemplate, routeTemplate, new {controller = controllerName, action = methodName});
        }
    }
}