﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAdapter
{
    public class GallogArticleDeleteParameter
    {
        public string UserId;
        public string GalleryId;
        public string GalleryNo;
        public string ArticleId;
        public string LogNo;
        public string DCCKey;
        public ParameterStorage AdditionalParameter = new ParameterStorage();
    }
}
