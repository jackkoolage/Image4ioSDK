using Image4ioSDK.JSON;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Handlers;
using System.Threading;
using System.Threading.Tasks;
using static Image4ioSDK.Basic;

namespace Image4ioSDK
{
    public class OClient : IClient
    {

        public OClient(string _CloudName, string _APIkey, string _APISecret, ConnectionSettings Settings = null)
        {
            ServicePointManager.Expect100Continue = true; ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

            if (Settings == null)
            {
                m_proxy = null;
            }
            else
            {
                m_proxy = Settings.Proxy;
                m_CloseConnection = Settings.CloseConnection ?? true;
                m_TimeOut = Settings.TimeOut ?? TimeSpan.FromMinutes(60);
            }
            APIkey = _APIkey;
            APISecret = _APISecret;
            CloudName = _CloudName;
        }



        public ITransformation Transformation(JSON_ImageMetadata ImageMetadata) => new TransformationClient(ImageMetadata);


        #region ImageMetadata
        public async Task<JSON_ImageMetadata> ImageMetadata(string ImagePath)
        {
            using (HtpClient localHtpClient = new HtpClient(new HCHandler { }))
            {
                using (HttpResponseMessage response = await localHtpClient.GetAsync(new pUri("get?" + string.Format("name={0}", ImagePath.Slash())), HttpCompletionOption.ResponseContentRead).ConfigureAwait(false))
                {
                    string result = await response.Content.ReadAsStringAsync();
                    return response.StatusCode == HttpStatusCode.OK ? JsonConvert.DeserializeObject<JSON_ImageMetadata>(result, JSONhandler) : throw ShowError(result);
                }
            }
        }
        #endregion

        #region UploadFile
        public async Task<JSON_Uploadedfile> Upload(object FileToUpload, Utilitiez.UploadTypes UploadType, string DestinationFolderPath, string FileName, bool UseFilename, bool OverwriteIfExists, IProgress<ReportStatus> ReportCls = null, CancellationToken token = default)
        {
            //'FileName.Validation()
            ReportCls = ReportCls ?? new Progress<ReportStatus>();
            ReportCls.Report(new ReportStatus { Finished = false, TextStatus = "Initializing..." });
            try
            {
                var progressHandler = new ProgressMessageHandler(new HCHandler());
                progressHandler.HttpSendProgress += (sender, e) => { ReportCls.Report(new ReportStatus { ProgressPercentage = e.ProgressPercentage, BytesTransferred = e.BytesTransferred, TotalBytes = e.TotalBytes ?? 0, TextStatus = "Uploading..." }); };
                using (HtpClient localHtpClient = new HtpClient(progressHandler))
                {
                    HttpRequestMessage HtpReqMessage = new HttpRequestMessage(HttpMethod.Post, new pUri("upload?" + string.Format("path={0}", DestinationFolderPath.Trim('/'))));
                    MultipartFormDataContent MultipartsformData = new MultipartFormDataContent();
                    HttpContent strContent = null;
                    switch (UploadType)
                    {
                        case Utilitiez.UploadTypes.FilePath:
                            strContent = new StreamContent(new System.IO.FileStream(Convert.ToString(FileToUpload), System.IO.FileMode.Open, System.IO.FileAccess.Read));
                            break;
                        case Utilitiez.UploadTypes.Stream:
                            strContent = new StreamContent((System.IO.Stream)FileToUpload);
                            break;
                        case Utilitiez.UploadTypes.BytesArry:
                            strContent = new StreamContent(new System.IO.MemoryStream((byte[])FileToUpload));
                            break;
                    }

                    MultipartsformData.Add(new StringContent(UseFilename.ToString()), "use_filename");
                    MultipartsformData.Add(new StringContent(OverwriteIfExists.ToString()), "overwrite");
                    MultipartsformData.Add(strContent, "Image", FileName.Trim('/'));
                    HtpReqMessage.Content = MultipartsformData;
                    //'''''''''''''''''will write the whole content to H.D WHEN download completed'''''''''''''''''''''''''''''
                    using (HttpResponseMessage ResPonse = await localHtpClient.SendAsync(HtpReqMessage, HttpCompletionOption.ResponseContentRead, token).ConfigureAwait(false))
                    {
                        string result = await ResPonse.Content.ReadAsStringAsync();

                        token.ThrowIfCancellationRequested();
                        if (ResPonse.StatusCode == HttpStatusCode.OK)
                        {
                            ReportCls.Report(new ReportStatus { Finished = true, TextStatus = string.Format("[{0}] Uploaded successfully", FileName) });
                            return JsonConvert.DeserializeObject<JSON_Uploadedfile>(JObject.Parse(result).SelectToken("uploadedFiles[*]").ToString(), JSONhandler);
                        }
                        else
                        {
                            ReportCls.Report(new ReportStatus { Finished = true, TextStatus = string.Format("The request returned with HTTP status code {0}", JObject.Parse(result).SelectToken("message").ToString()) });
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ReportCls.Report(new ReportStatus { Finished = true });
                if (ex.Message.ToString().ToLower().Contains("a task was canceled"))
                {
                    ReportCls.Report(new ReportStatus { TextStatus = ex.Message });
                }
                else
                {
                    throw new Image4ioException(ex.Message, 1001);
                }
                return null;
            }
        }
        #endregion

        #region UploadMultiableFiles
        public struct MultiableUpload
        {
            public object FileToUpload;
            public string FileName;
        }
        public async Task<JSON_MultiUpload> UploadMultiable(List<MultiableUpload> FilesToUpload, Utilitiez.UploadTypes UploadType, string DestinationFolderPath, bool UseFilename, bool OverwriteIfExists, IProgress<ReportStatus> ReportCls = null, CancellationToken token = default(CancellationToken))
        {
            ReportCls = ReportCls ?? new Progress<ReportStatus>();
            ReportCls.Report(new ReportStatus { Finished = false, TextStatus = "Initializing..." });
            try
            {
                ProgressMessageHandler progressHandler = new ProgressMessageHandler(new HCHandler { });
                progressHandler.HttpSendProgress += (sender, e) =>
                {
                    ReportCls.Report(new ReportStatus { ProgressPercentage = e.ProgressPercentage, BytesTransferred = e.BytesTransferred, TotalBytes = e.TotalBytes ?? 0, TextStatus = "Uploading..." });
                };
                using (HtpClient localHtpClient = new HtpClient(progressHandler))
                {
                    HttpRequestMessage HtpReqMessage = new HttpRequestMessage(HttpMethod.Post, new pUri("upload?" + string.Format("path={0}", DestinationFolderPath.Trim('/'))));
                    MultipartFormDataContent MultipartsformData = new MultipartFormDataContent();

                    foreach (var Fle in FilesToUpload)
                    {
                        HttpContent strContent = null;
                        switch (UploadType)
                        {
                            case Utilitiez.UploadTypes.FilePath:
                                strContent = new StreamContent(new System.IO.FileStream(Fle.FileToUpload.ToString(), System.IO.FileMode.Open, System.IO.FileAccess.Read));
                                break;
                            case Utilitiez.UploadTypes.Stream:
                                strContent = new StreamContent((System.IO.Stream)Fle.FileToUpload);
                                break;
                            case Utilitiez.UploadTypes.BytesArry:
                                strContent = new StreamContent(new System.IO.MemoryStream((Byte[])Fle.FileToUpload));
                                break;
                        }
                        MultipartsformData.Add(strContent, Fle.FileName.Trim('/'), Fle.FileName.Trim('/'));
                    }


                    MultipartsformData.Add(new StringContent(UseFilename.ToString()), "use_filename");
                    MultipartsformData.Add(new StringContent(OverwriteIfExists.ToString()), "overwrite");
                    HtpReqMessage.Content = MultipartsformData;
                    //'''''''''''''''''will write the whole content to H.D WHEN download completed'''''''''''''''''''''''''''''
                    using (HttpResponseMessage ResPonse = await localHtpClient.SendAsync(HtpReqMessage, HttpCompletionOption.ResponseContentRead, token).ConfigureAwait(false))
                    {
                        string result = await ResPonse.Content.ReadAsStringAsync();

                        token.ThrowIfCancellationRequested();
                        if (ResPonse.StatusCode == HttpStatusCode.OK)
                        {
                            ReportCls.Report(new ReportStatus { Finished = true, TextStatus = string.Format("[{0}] Files Uploaded successfully", FilesToUpload.Count) });
                            return JsonConvert.DeserializeObject<JSON_MultiUpload>(result, JSONhandler);
                        }
                        else
                        {
                            ReportCls.Report(new ReportStatus { Finished = true, TextStatus = string.Format("The request returned with HTTP status code {0}", Newtonsoft.Json.Linq.JObject.Parse(result).SelectToken("message").ToString()) });
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ReportCls.Report(new ReportStatus { Finished = true });
                if (ex.Message.ToString().ToLower().Contains("a task was canceled"))
                {
                    ReportCls.Report(new ReportStatus { TextStatus = ex.Message });
                }
                else
                {
                    throw new Image4ioException(ex.Message, 1001);
                }
                return null;
            }
        }
        #endregion

        #region UploadRemotely
        public async Task<JSON_Uploadedfile> UploadRemotely(string ImgUrl, string DestinationFolderPath)
        {
            using (HtpClient localHtpClient = new HtpClient(new HCHandler { }))
            {
                using (HttpResponseMessage response = await localHtpClient.PostAsync(new pUri("fetch?" + string.Format("from={0}&target_path={1}", ImgUrl, DestinationFolderPath.Trim('/'))), null).ConfigureAwait(false))
                {
                    string result = await response.Content.ReadAsStringAsync();
                    return response.StatusCode == HttpStatusCode.OK ? JsonConvert.DeserializeObject<JSON_Uploadedfile>(JObject.Parse(result).SelectToken("fetchedFile").ToString(), JSONhandler) : throw ShowError(result);
                }
            }
        }
        #endregion

        #region CopyImage
        public async Task<JSON_Uploadedfile> ImageCopy(string ImagePath, string DestinationFolderPath)
        {
            using (HtpClient localHtpClient = new HtpClient(new HCHandler { }))
            {
                using (HttpResponseMessage response = await localHtpClient.PutAsync(new pUri("copy?" + string.Format("source={0}&Target_path={1}", ImagePath.Slash(), DestinationFolderPath.Trim('/'))), null).ConfigureAwait(false))
                {
                    string result = await response.Content.ReadAsStringAsync();
                    return response.StatusCode == HttpStatusCode.OK ? JsonConvert.DeserializeObject<JSON_Uploadedfile>(JObject.Parse(result).SelectToken("copiedFile").ToString(), JSONhandler) : throw ShowError(result);
                }
            }
        }
        #endregion

        #region "MoveImage"
        public async Task<JSON_Uploadedfile> ImageMove(string ImagePath, string DestinationFolderPath)
        {
            using (HtpClient localHtpClient = new HtpClient(new HCHandler { }))
            {
                using (HttpResponseMessage response = await localHtpClient.PutAsync(new pUri("move?" + string.Format("source={0}&Target_path={1}", ImagePath.Slash(), DestinationFolderPath.Trim('/'))), null).ConfigureAwait(false))
                {
                    string result = await response.Content.ReadAsStringAsync();
                    return response.StatusCode == HttpStatusCode.OK ? JsonConvert.DeserializeObject<JSON_Uploadedfile>(JObject.Parse(result).SelectToken("movedFile").ToString(), JSONhandler) : throw ShowError(result);
                }
            }
        }
        #endregion

        #region DeleteImage
        public async Task<JSON_Uploadedfile> ImageDelete(string ImagePath)
        {
            using (HtpClient localHtpClient = new HtpClient(new HCHandler { }))
            {
                using (HttpResponseMessage response = await localHtpClient.DeleteAsync(new pUri("deletefile?" + string.Format("name={0}", ImagePath.Slash()))).ConfigureAwait(false))
                {
                    string result = await response.Content.ReadAsStringAsync();
                    return response.StatusCode == HttpStatusCode.OK ? JsonConvert.DeserializeObject<JSON_Uploadedfile>(JObject.Parse(result).SelectToken("deletedFile").ToString(), JSONhandler) : throw ShowError(result);
                }
            }
        }
        #endregion

        #region DeleteFolder
        public async Task<JSON_Uploadedfile> FolderDelete(string FolderPath)
        {
            using (HtpClient localHtpClient = new HtpClient(new HCHandler { }))
            {
                using (HttpResponseMessage response = await localHtpClient.DeleteAsync(new pUri("deletefolder?" + string.Format("path={0}", FolderPath.Trim('/')))).ConfigureAwait(false))
                {
                    string result = await response.Content.ReadAsStringAsync();
                    return response.StatusCode == HttpStatusCode.OK ? JsonConvert.DeserializeObject<JSON_Uploadedfile>(JObject.Parse(result).SelectToken("deletedFolder").ToString(), JSONhandler) : throw ShowError(result);
                }
            }
        }
        #endregion

        #region CreateFolder
        public async Task<JSON_Uploadedfile> FolderCreate(string FolderName, string DestinationFolderPath)
        {
            string folderPath = string.Concat(DestinationFolderPath.Trim('/'), "/", FolderName);

            using (HtpClient localHtpClient = new HtpClient(new HCHandler { }))
            {
                using (HttpResponseMessage response = await localHtpClient.PostAsync(new pUri("CreateFolder?" + string.Format("path={0}", folderPath)), null).ConfigureAwait(false))
                {
                    string result = await response.Content.ReadAsStringAsync();
                    return response.StatusCode == HttpStatusCode.OK ? JsonConvert.DeserializeObject<JSON_Uploadedfile>(JObject.Parse(result).SelectToken("createdFolder").ToString(), JSONhandler) : throw ShowError(result);
                }
            }
        }
        #endregion

        #region ListFolder
        public async Task<JSON_FolderList> FolderList(string FolderPath)
        {
            using (HtpClient localHtpClient = new HtpClient(new HCHandler { }))
            {
                using (HttpResponseMessage response = await localHtpClient.GetAsync(new pUri("listfolder?" + string.Format("path={0}", FolderPath.Trim('/'))), HttpCompletionOption.ResponseContentRead).ConfigureAwait(false))
                {
                    string result = await response.Content.ReadAsStringAsync();
                    return response.StatusCode == HttpStatusCode.OK ? JsonConvert.DeserializeObject<JSON_FolderList>(result, JSONhandler) : throw ShowError(result);
                }
            }
        }
        #endregion

        #region DownloadFile
        public async Task Download(string ImageUrl, string ImageSaveDir, string FileName, IProgress<ReportStatus> ReportCls = null, CancellationToken token = default)
        {
            ReportCls = ReportCls ?? new Progress<ReportStatus>();
            ReportCls.Report(new ReportStatus { Finished = false, TextStatus = "Initializing..." });
            try
            {
                var progressHandler = new ProgressMessageHandler(new HCHandler());
                progressHandler.HttpReceiveProgress += (sender, e) => { ReportCls.Report(new ReportStatus { ProgressPercentage = e.ProgressPercentage, BytesTransferred = e.BytesTransferred, TotalBytes = e.TotalBytes ?? 0, TextStatus = "Downloading..." }); };
                using (HtpClient localHtpClient = new HtpClient(progressHandler))
                {
                    HttpRequestMessage HtpReqMessage = new HttpRequestMessage() { Method = HttpMethod.Get, RequestUri = new Uri(ImageUrl) };
                    //'''''''''''''''''will write the whole content to H.D WHEN download completed'''''''''''''''''''''''''''''
                    using (HttpResponseMessage ResPonse = await localHtpClient.SendAsync(HtpReqMessage, HttpCompletionOption.ResponseContentRead, token).ConfigureAwait(false))
                    {
                        token.ThrowIfCancellationRequested();
                        if (ResPonse.IsSuccessStatusCode)
                        {
                            ReportCls.Report(new ReportStatus { Finished = true, TextStatus = (string.Format("[{0}] Downloaded successfully.", FileName)) });
                        }
                        else
                        {
                            ReportCls.Report(new ReportStatus { Finished = true, TextStatus = ((string.Format("Error code: {0}", ResPonse.StatusCode))) });
                        }

                        ResPonse.EnsureSuccessStatusCode();
                        var stream_ = await ResPonse.Content.ReadAsStreamAsync();
                        string FPathname = System.IO.Path.Combine(ImageSaveDir, FileName);
                        using (System.IO.Stream fileStream = new System.IO.FileStream(FPathname, System.IO.FileMode.Append, System.IO.FileAccess.Write))
                        {
                            stream_.CopyTo(fileStream);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ReportCls.Report(new ReportStatus { Finished = true });
                if (ex.Message.ToString().ToLower().Contains("a task was canceled"))
                {
                    ReportCls.Report(new ReportStatus { TextStatus = ex.Message });
                }
                else
                {
                    throw new Image4ioException(ex.Message, 1001);
                }
            }
        }
        #endregion


    }
}
