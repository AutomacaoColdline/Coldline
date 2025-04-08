using System.Reflection;
using System.Web;

namespace ColdlineWeb.Util
{
    public static class QueryBuilderExtensions
    {
        public static string ToQueryString(this object obj)
        {
            var properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var query = HttpUtility.ParseQueryString(string.Empty);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(obj);
                if (value == null) continue;

                if (value is Enum)
                    query[prop.Name] = ((int)value).ToString();
                else
                    query[prop.Name] = value.ToString();
            }

            return query.ToString();
        }
    }
}