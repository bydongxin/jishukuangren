using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace JSKR.Web.Controllers
{
    public class CSharpController : Controller
    {
        // GET: CSharp
        public ActionResult Index(string[] key, string[] value)
        {
            Model model = new Model();
            
            List<KeyValueList> list = new List<KeyValueList>();
            if (key != null)
            {
                for (int i = 0; i < key.Length; i++)
                {
                    list.Add(new KeyValueList(key[i], value[i]));
                }
            }
            model.List = list;
            string str = new JavaScriptSerializer().Serialize(model);
            return View();
        }
    }

    public class Model
    {
        public object List { get; set; }
    }

    public class KeyValueList
    {
        public KeyValueList(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }
        public string Value { get; set; }
    }

}