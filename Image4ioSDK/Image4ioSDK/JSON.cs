using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Image4ioSDK;

namespace Image4ioSDK.JSON
{
    public enum ResultsStatusEnum { Uploaded, fetched, Copied, Moved, Deleted, Created, AlreadyExist, Overwrited, Error }


    public class JSON_Error
    {
        [JsonProperty("message")]
        public string _ErrorMessage { get; set; }
        [JsonProperty("statusCode")]
        public int _ErrorCode { get; set; }
    }


    #region "JSON_ImageMetadata"
    public class JSON_ImageMetadata
    {
        //'https://stackoverflow.com/posts/43715009/revisions
        [JsonProperty("userGivenName")]
        public string Filename { get; set; }

        [JsonProperty("orginal_name")]
        public string Filename2
        {
            set
            {
                Filename = value;
            }
        }

        public string folder { get; set; }//''list only, not in single metadata
        [JsonProperty("name")]
        public string Path { get; set; }
        public int size { get; set; }
        public string format { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string createdAtUTC { get; set; }
        public string updatedAtUTC { get; set; }
        public string DirectUrl
        {
            get { return String.Format("https://cdn.image4.io/{0}/f_auto{1}", Image4ioSDK.Basic.CloudName, Path); }
        }

    }
    #endregion


    #region "JSON_Upload"
    public class JSON_MultiUpload
    {
        public List<JSON_Uploadedfile> uploadedFiles;
    }
    public class JSON_Uploadedfile
    {
        public string orginal_name;
        public string name;
        public ResultsStatusEnum status;
    }
    #endregion

    #region "JSON_FolderList"
    public class JSON_FolderList
    {
        public List<JSON_FolderMetadata> folders { get; set; }

        public List<JSON_ImageMetadata> files { get; set; }
    }
    public class JSON_FolderMetadata
    {
        public string name;
        public string parent;
    }
    #endregion






}
