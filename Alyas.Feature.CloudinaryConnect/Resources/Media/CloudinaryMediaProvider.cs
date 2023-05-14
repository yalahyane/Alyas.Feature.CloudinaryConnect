using System;
using CloudinaryDotNet;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Links.UrlBuilders;
using Sitecore.Resources.Media;
using Convert = System.Convert;

namespace Alyas.Feature.CloudinaryConnect.Resources.Media
{
    public class CloudinaryMediaProvider : MediaProvider
    {
        private static string CloudinaryCloud => Settings.GetSetting("Alyas.Connect.Cloudinary.Cloud");
        private static string CloudinaryApiKey => Settings.GetSetting("Alyas.Connect.Cloudinary.ApiKey");
        private static string CloudinaryApiSecret => Settings.GetSetting("Alyas.Connect.Cloudinary.ApiSecret");
        private static string CloudinaryBaseUrl => Settings.GetSetting("Alyas.Connect.Cloudinary.BaseUrl");
        private static bool IsResponsive => Convert.ToBoolean(Settings.GetSetting("Alyas.Connect.Cloudinary.IsResponsive"));
        private static bool QualityAuto => Convert.ToBoolean(Settings.GetSetting("Alyas.Connect.Cloudinary.QualityAuto"));
        private static bool FormatAuto => Convert.ToBoolean(Settings.GetSetting("Alyas.Connect.Cloudinary.FormatAuto"));

        public override string GetMediaUrl(MediaItem item, MediaUrlBuilderOptions options)
        {
            var mediaUrl = base.GetMediaUrl(item, options);
            try
            {
                if (Context.Database != null && !Context.Database.Name.Equals("web", StringComparison.OrdinalIgnoreCase))
                {
                    return mediaUrl;
                }

                var cloudinary = new Cloudinary(new Account(CloudinaryCloud, CloudinaryApiKey, CloudinaryApiSecret));
                var results = cloudinary.Search().Expression($"resource_type:image AND public_id:{item.MediaPath.Substring(1)}").Execute();
                if (results.TotalCount > 0)
                {
                    return BuildImageUrl(item, options);
                }
                return mediaUrl;
            }
            catch (Exception e)
            {
                Log.Error($"An unexpected error happened while getting the image {item.Name} from Cloudinary. Exception: {e}", this);
                return mediaUrl;
            }
        }

        private string BuildImageUrl(MediaItem item, MediaUrlBuilderOptions options)
        {
            var url = $"{CloudinaryBaseUrl}/{CloudinaryCloud}/image/upload/";
            if (QualityAuto)
            {
                url = $"{url}q_auto,";
            }

            if (FormatAuto)
            {
                url = $"{url}w_auto,";
            }

            if (IsResponsive)
            {
                url = $"{url}c_limit,w_auto/dpr_auto";
            }
            else
            {
                if (options.Width.HasValue)
                {
                    url = $"{url}w_{options.Width.Value}";
                }
                if (options.Height.HasValue)
                {
                    url = $"{url}w_{options.Height.Value}";
                }
            }
            return  $"{url}{item.MediaPath}";
        }
    }
}