using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using Uploader.Managers.Common;
using Uploader.Managers.Ipfs;
using Uploader.Models;

namespace Uploader.Managers.Front
{
    public static class SubtitleManager
    {
        public static async Task<Guid> ComputeSubtitle(string text)
        {
            FileContainer fileContainer = FileContainer.NewSubtitleContainer();
            string outputfilePath = Path.ChangeExtension(TempFileManager.GetNewTempFilePath(), ".vtt");

            if (!IsValidVTT(text))
                return fileContainer.ProgressToken;

            try
            {
                await System.IO.File.WriteAllTextAsync(outputfilePath, text);
                fileContainer.SubtitleFileItem.SetOutputFilePath(outputfilePath);
                IpfsDaemon.Instance.Queue(fileContainer.SubtitleFileItem);
            }
            catch(Exception ex)
            {
                TempFileManager.SafeDeleteTempFile(outputfilePath);
                LogManager.AddSubtitleMessage(ex.ToString(), "Exception");
                return fileContainer.ProgressToken;
            }

            return fileContainer.ProgressToken;
        }

        private static bool IsValidVTT(string text)
        {
            Debug.WriteLine(text);
            if (!text.StartsWith("WEBVTT"))
                return false;

            // eventuellement rajouter plus de verifs
            // mais peu d'interet
            
            return true;
        }
    }
}