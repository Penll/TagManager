﻿using System.Globalization;
using System.Resources;

namespace BooruDatasetTagManager
{
    public class LangManager
    {
        public CultureInfo CultureInfo;
        public ResourceManager _ResourceManager;
        public ResourceManager ResourceManager
        {
            get
            {
                if (_ResourceManager == null)
                {
                    _ResourceManager= new ResourceManager("BooruDatasetTagManager.Lang", typeof(Program).Assembly);
                }
                return _ResourceManager;
            }
            set
            {
                _ResourceManager = value;
            }
        }

        public LangManager(string culture)
        {
            CultureInfo = new CultureInfo(culture);
        }

        public string GetString(string name)
        {
            return ResourceManager.GetString(name, CultureInfo);
        }
    }


}
