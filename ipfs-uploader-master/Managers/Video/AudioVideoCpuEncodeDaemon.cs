using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Uploader.Daemons;
using Uploader.Managers.Common;
using Uploader.Managers.Front;
using Uploader.Managers.Ipfs;
using Uploader.Models;

namespace Uploader.Managers.Video
{
    public class AudioVideoCpuEncodeDaemon : BaseDaemon
    {
        public static AudioVideoCpuEncodeDaemon Instance { get; private set; }

        static AudioVideoCpuEncodeDaemon()
        {
            Instance = new AudioVideoCpuEncodeDaemon();
            Instance.Start(VideoSettings.NbAudioVideoCpuEncodeDaemon);
        }

        protected override void ProcessItem(FileItem fileItem)
        {
            // si le client a pas demandé le progress depuis plus de 20s, annuler l'opération
            if (!fileItem.AudioVideoCpuEncodeProcess.CanProcess())
            {
                string message = "FileName " + Path.GetFileName(fileItem.OutputFilePath) + " car le client est déconnecté";
                LogManager.AddEncodingMessage(message, "Annulation");
                fileItem.AudioVideoCpuEncodeProcess.CancelCascade("Le client est déconnecté.");
                return;
            }

            if (EncodeManager.AudioVideoCpuEncoding(fileItem))
            {
                // rechercher le 480p pour le sprite
                if(fileItem.VideoSize == VideoSize.F480p && fileItem.FileContainer.SpriteVideoFileItem != null)
                {
                    fileItem.FileContainer.SpriteVideoFileItem.SetSourceFilePath(fileItem.OutputFilePath);
                    SpriteDaemon.Instance.Queue(fileItem.FileContainer.SpriteVideoFileItem, "Waiting sprite creation...");
                }

                IpfsDaemon.Instance.Queue(fileItem);
            }
        }

        protected override void LogException(FileItem fileItem, Exception ex)
        {
            LogManager.AddEncodingMessage(ex.ToString(), "Exception non gérée");                        
            fileItem.AudioVideoCpuEncodeProcess.SetErrorMessage("Exception non gérée");
        }

        public void Queue(FileItem fileItem, string messageIpfs)
        {
            base.Queue(fileItem, fileItem.AudioVideoCpuEncodeProcess);

            fileItem.IpfsProcess.SetProgress(messageIpfs, true);
        }
    }
}