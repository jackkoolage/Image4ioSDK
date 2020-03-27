using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Image4ioSDK
{
    public class TransformationClient : ITransformation
    {

        private JSON.JSON_ImageMetadata ImageMetadata;
        public TransformationClient(JSON.JSON_ImageMetadata ImageMetadata)
        {
            this.ImageMetadata = ImageMetadata;
        }


        #region ToWebP
        public string ToWebP(int Quality, int? Width = null, int? Height = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("f_webp");
            sb.AppendFormat(",q_{0}", Quality);
            if (Width.HasValue) sb.Append(",w_").Append(Width.Value);
            if (Height.HasValue) sb.Append(",h_").Append(Height.Value);

            return $"https://cdn.image4.io/{Basic.CloudName}/{sb.ToString()}{ImageMetadata.Path}";
        }
        #endregion

        #region Resize
        public string Resize(int Quality, int? Width = null, int? Height = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("q_{0}", Quality);
            if (Width.HasValue) sb.Append(",w_").Append(Width.Value);
            if (Height.HasValue) sb.Append(",h_").Append(Height.Value);

            return $"https://cdn.image4.io/{Basic.CloudName}/{sb.ToString()}{ImageMetadata.Path}";
        }
        #endregion

        #region Compress
        public string Compress(int Quality)
        {
            return Resize(Quality);
        }
        #endregion


    }
}
