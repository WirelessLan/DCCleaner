﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAdapter
{
    public class GallogCommentDeleteParameters
    {
        public string UserId;
        public string GalleryId;
        public string GalleryNo;
        public string ArticleId;
        public string CommentId;
        public string LogNo;
        public ParameterStorage AdditionalParameters = new ParameterStorage();
    }
}
