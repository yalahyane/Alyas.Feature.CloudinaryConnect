using System;
using System.Net;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Events;

namespace Alyas.Feature.CloudinaryConnect.Events
{
    public class CloudinaryItemDeletingHandler
    {
        private static string CloudinaryCloud => Settings.GetSetting("Alyas.Connect.Cloudinary.Cloud");
        private static string CloudinaryApiKey => Settings.GetSetting("Alyas.Connect.Cloudinary.ApiKey");
        private static string CloudinaryApiSecret => Settings.GetSetting("Alyas.Connect.Cloudinary.ApiSecret");
        public void OnItemDeleting(object sender, EventArgs args)
        {
            try
            {
                var deletingItem = Event.ExtractParameter(args, 0) as Item;

                if (deletingItem == null ||
                    !"master".Equals(deletingItem.Database?.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (!deletingItem.Paths.FullPath.ToLower().StartsWith("/sitecore/media library/") ||
                    (!deletingItem.TemplateName.Equals("Jpeg", StringComparison.OrdinalIgnoreCase) &&
                     !deletingItem.TemplateName.Equals("Image", StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }


                var cloudinary = new Cloudinary(new Account(CloudinaryCloud, CloudinaryApiKey, CloudinaryApiSecret));
                var publicId = deletingItem.Paths.FullPath.Replace("/sitecore/media library/", "");
                var response = cloudinary.Destroy(new DeletionParams(publicId));
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Log.Info($"Cloudinary Sync - Successfully Deleted {publicId}", this);
                }
                else
                {
                    Log.SingleError($"Cloudinary Sync - Failed to Delete {publicId}. Error: {response.Error?.Message}", this);
                }
            }
            catch (Exception e)
            {
                Log.SingleError($"Cloudinary Sync - Unexpected error while trying to to Delete Image from Cloudinary. Exception: {e}", this);
            }
            
        }
    }
}