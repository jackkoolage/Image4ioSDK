using Image4ioSDK.JSON;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Image4ioSDK
{
    public interface IClient
    {
        //public ITransformation Transformation(JSON_ImageMetadata ImageMetadata) { get; }
        ITransformation Transformation(JSON_ImageMetadata ImageMetadata);


        Task<JSON.JSON_ImageMetadata> ImageMetadata(string ImagePath);
        Task<JSON_Uploadedfile> Upload(object FileToUpload, Utilitiez.UploadTypes UploadType, string DestinationFolderPath, string FileName, bool UseFilename, bool OverwriteIfExists, IProgress<ReportStatus> ReportCls = null, System.Threading.CancellationToken token = default(CancellationToken));
        Task<JSON_MultiUpload> UploadMultiable(List<OClient.MultiableUpload> FilesToUpload, Utilitiez.UploadTypes UploadType, string DestinationFolderPath, bool UseFilename, bool OverwriteIfExists, IProgress<ReportStatus> ReportCls = null, System.Threading.CancellationToken token = default(CancellationToken));
        Task<JSON_Uploadedfile> UploadRemotely(string ImgUrl, string DestinationFolderPath);

        Task<JSON_Uploadedfile> ImageCopy(string ImagePath, string DestinationFolderPath);
        Task<JSON_Uploadedfile> ImageMove(string ImagePath, string DestinationFolderPath);
        Task<JSON_Uploadedfile> ImageDelete(string ImagePath);

        Task<JSON_Uploadedfile> FolderDelete(string FolderPath);
        Task<JSON_Uploadedfile> FolderCreate(string FolderName, string DestinationFolderPath);
        Task<JSON_FolderList> FolderList(string FolderPath);
        Task Download(string ImageUrl, string ImageSaveDir, string FileName, IProgress<ReportStatus> ReportCls = null, CancellationToken token = default(CancellationToken));


    }
}
