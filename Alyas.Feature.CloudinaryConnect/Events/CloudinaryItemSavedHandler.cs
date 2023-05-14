using System;
using System.Net;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Events;
using Sitecore.Links.UrlBuilders;
using Sitecore.Resources.Media;

namespace Alyas.Feature.CloudinaryConnect.Events
{
    public class CloudinaryItemSavedHandler
    {
        private static string CloudinaryCloud => Settings.GetSetting("Alyas.Connect.Cloudinary.Cloud");
        private static string CloudinaryApiKey => Settings.GetSetting("Alyas.Connect.Cloudinary.ApiKey");
        private static string CloudinaryApiSecret => Settings.GetSetting("Alyas.Connect.Cloudinary.ApiSecret");
        public void OnItemSaved(object sender, EventArgs args)
        {
            try
            {
                var savedItem = Event.ExtractParameter(args, 0) as Item;
                if (savedItem == null || !"master".Equals(savedItem.Database?.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (!savedItem.Paths.FullPath.ToLower().StartsWith("/sitecore/media library/") ||
                    (!savedItem.TemplateName.Equals("Jpeg", StringComparison.OrdinalIgnoreCase) &&
                     !savedItem.TemplateName.Equals("Image", StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }

                var image = new MediaItem(savedItem);
                var mediaUrlOptions = new MediaUrlBuilderOptions
                {
                    AlwaysIncludeServerUrl = true
                };
                var imgUrl = MediaManager.GetMediaUrl(image, mediaUrlOptions).Replace("/sitecore/shell/", "/");

                var cloudinary = new Cloudinary(new Account(CloudinaryCloud, CloudinaryApiKey, CloudinaryApiSecret));
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(imgUrl),
                    Overwrite = true,
                    PublicId = savedItem.Paths.FullPath.Replace("/sitecore/media library/", "")
                };
                var response = cloudinary.Upload(uploadParams);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Log.Info($"Cloudinary Sync - Successfully Uploaded {response.PublicId}", this);
                }
                else
                {
                    Log.SingleError($"Cloudinary Sync - Failed to Upload {uploadParams.PublicId}. Error: {response.Error?.Message}", this);
                }
            }
            catch (Exception e)
            {
                Log.SingleError($"Cloudinary Sync - Unexpected error while trying to to Upload Images to Cloudinary. Exception: {e}", this);
            }
            
        }
    }
}