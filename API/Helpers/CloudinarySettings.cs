using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers
{
    // this C# class will be used to strongly type our Cloudinary settings and use them in our app
    public class CloudinarySettings
    {
        public string Cloudname { get; set; }
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
    }
}