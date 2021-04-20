using System;

namespace Veracity.Services.Api.Models
{
    public class ProfilePicture
    {
        public byte[] Image { get; set; }
        public string MimeType { get; set; }

        public string AsBase64Image()
        {
            return $"data:{MimeType};base64,{Convert.ToBase64String(Image)}";
        }

        public string Base64
        {
            get => AsBase64Image();
        }
    }
}