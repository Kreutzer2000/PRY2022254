using System.Web;
using System.Web.Mvc;

namespace PRY2022254.PresentacionAdmin
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
