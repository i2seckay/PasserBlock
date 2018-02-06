using System;
using System.Linq;

using Uploader.Managers.Ipfs;
using Uploader.Managers.Video;
using Uploader.Models;

namespace Uploader.Managers.Front
{
    public static class VideoManager
    {
        public static Guid ComputeVideo(string originFilePath, string videoEncodingFormats, bool? sprite)
        {
            VideoSize[] formats = new VideoSize[0];

            if (!string.IsNullOrWhiteSpace(videoEncodingFormats))
            {
                formats = videoEncodingFormats
                    .Split(',')
                    .Select(v =>
                    {
                        switch (v)
                        {
                            case "360p":
                                return VideoSize.F360p;
                            case "480p":
                                return VideoSize.F480p;
                            case "720p":
                                return VideoSize.F720p;
                            case "1080p":
                                return VideoSize.F1080p;
                            default:
                                throw new InvalidOperationException("Format non reconnu.");
                        }
                    })
                    .ToArray();
            }

            FileContainer fileContainer = FileContainer.NewVideoContainer(originFilePath, sprite??false, formats);

            if(IpfsSettings.AddVideoSource)
            {
                IpfsDaemon.Instance.Queue(fileContainer.SourceFileItem);
            }

            if(VideoSettings.GpuEncodeMode)
            {
                // encoding audio de la source puis ça sera encoding videos Gpu
                AudioCpuEncodeDaemon.Instance.Queue(fileContainer.SourceFileItem, "waiting audio encoding...");
            }
            else
            {
                // si encoding est demandé, et gpuMode -> encodingAudio
                foreach (FileItem file in fileContainer.EncodedFileItems)
                {
                    AudioVideoCpuEncodeDaemon.Instance.Queue(file, "Waiting encode...");
                }
            }

            return fileContainer.ProgressToken;
        }
    }
}