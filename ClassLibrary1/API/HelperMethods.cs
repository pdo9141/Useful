using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ClassLibrary1.API
{
    public class HelperMethods
    {
        // Patches (or merges) an object with a name-value collection
        public T PatchObject<T>(T resource, NameValueCollection nameValues) {
            string values;
            if (HttpContext.Current.Request.Form.Count == 1
                && HttpContext.Current.Request.Form[0] != null
                && (HttpContext.Current.Request.Form[0].StartsWith("{")
                || HttpContext.Current.Request.Form[0].StartsWith("["))) {
                values = HttpContext.Current.Request.Form[0];
            }
            else {
                var map = nameValues.AllKeys.ToDictionary(key => key, nameValues.Get);
                values = JsonConvert.SerializeObject(map);
            }

            try
            {
                JsonConvert.PopulateObject(values, resource);
            }
            catch (JsonReaderException)
            {
            }

            return resource;
        }

        // Deserializes an object from the form body. The form body may contain name-values or JSON format
        public T GetObjectFromFormBody<T>() {
            string values;
            if (HttpContext.Current.Request.Form.Count == 1
                && HttpContext.Current.Request.Form[0] != null
                && (HttpContext.Current.Request.Form[0].StartsWith("{")
                || HttpContext.Current.Request.Form[0].StartsWith("[")))
            {
                values = HttpContext.Current.Request.Form[0];
            }
            else
            {
                var nameValues = HttpContext.Current.Request.Form;
                values = JsonConvert.SerializeObject(nameValues.AllKeys.ToDictionary(key => key, nameValues.Get));
            }

            return JsonConvert.DeserializeObject<T>(values);
        }

        // Help with Patch
        public T MergeModels<T>(T from, T into) {
            if (from == null) return default(T);
            if (into == null) throw new ArgumentNullException("into", "No entity found to patch");
            var jFrom = JObject.FromObject(from);
            var jInto = JObject.FromObject(into);

            jInto.Merge(jFrom, new JsonMergeSettings {
                MergeArrayHandling = MergeArrayHandling.Replace,
                MergeNullValueHandling = MergeNullValueHandling.Ignore
            });

            return jInto.ToObject<T>();
        }
    }
}
