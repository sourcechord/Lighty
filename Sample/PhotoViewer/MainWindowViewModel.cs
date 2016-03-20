using PhotoViewer.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoViewer
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly List<string> supportExt = new List<string>() { ".jpg", ".jpeg", ".bmp", ".png", ".tif", ".tiff" };



        private ObservableCollection<Uri> imageList;
        public ObservableCollection<Uri> ImageList
        {
            get { return imageList; }
            set { this.SetProperty(ref this.imageList, value); }
        }


        public MainWindowViewModel()
        {
            this.ImageList = new ObservableCollection<Uri>()
            {
                new Uri("Images/1.jpg", UriKind.Relative),
                new Uri("Images/2.jpg", UriKind.Relative),
                new Uri("Images/3.jpg", UriKind.Relative),
                new Uri("Images/4.jpg", UriKind.Relative),
                new Uri("Images/5.jpg", UriKind.Relative),
            };
        }



        public void SetSourceDirectory(string path)
        {
            var isExist = Directory.Exists(path);
            if (!isExist) { return; }

            var files = this.GetFiles(path, supportExt)
                            .Select(o => new Uri(o.FullName));
            this.ImageList = new ObservableCollection<Uri>(files);
        }


        protected IEnumerable<FileInfo> GetFiles(string dir, IEnumerable<string> supportExts)
        {
            if (!Directory.Exists(dir))
            {
                // ディレクトリが無いときは、空リストを返す。
                return new List<FileInfo>();
            }

            var dirInfo = new DirectoryInfo(dir);
            return dirInfo.GetFiles()
                          .Where(f => supportExts.Contains(f.Extension, StringComparer.OrdinalIgnoreCase))
                          .OrderBy(f => f.Name);
        }
    }
}
