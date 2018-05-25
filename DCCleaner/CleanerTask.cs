using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCCleaner
{
    public enum CleanerTask
    {
        LoadGallogArticles,
        DeleteGallogArticles,
        LoadGallogComments,
        DeleteGallogComments,
        SearchGalleryArticles,
        DeleteGalleryArticles,
        None
    }
}
