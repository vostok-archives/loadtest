using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace KafkaLoadService.Core
{
    public static class MapHttpRouteCollectionExtentions
    {
        public static void AddRoute<TColtroller>(this IRouteBuilder routeBuilder, string routeTemplate, Expression<Action<TColtroller>> expression)
            where TColtroller : ControllerBase
        {
            var type = typeof(TColtroller);
            var methodName = expression.Name
                             ?? ((MethodCallExpression)expression.Body).Method.Name;
            const string controllerSuffix = "Controller";
            var controllerName = type.Name.EndsWith(controllerSuffix)
                ? type.Name.Substring(0, type.Name.Length - controllerSuffix.Length)
                : type.Name;

            routeBuilder.MapRoute(routeTemplate, routeTemplate, new { controller = controllerName, action = methodName });
        }
    }
}