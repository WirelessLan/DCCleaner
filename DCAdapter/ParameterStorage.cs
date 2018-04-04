﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DCAdapter
{
    public class ParameterStorage
    {
        public Dictionary<string, string> Parameters { get; } = new Dictionary<string, string>();

        public string this[string key]
        {
            get
            {
                if (Parameters.ContainsKey(key))
                    return Parameters[key];
                return null;
            }
            set
            {
                Parameters[key] = value;
            }
        }

        public override string ToString()
        {
            string retVal = "";
            foreach (KeyValuePair<string, string> kv in Parameters)
            {
                retVal += HttpUtility.UrlEncode(kv.Key) + "=" + HttpUtility.UrlEncode(kv.Value) + "&";
            }

            if (retVal.Length > 0)
                retVal = retVal.Substring(0, retVal.Length - 1);
            return retVal;
        }

        public void Push(string key, string val) => Parameters.Add(key, val);
    }
}