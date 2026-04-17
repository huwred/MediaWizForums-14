namespace WWWTest
{
    using System.Security.Cryptography;
    using Umbraco.Cms.Core.Composing;
    using Umbraco.Cms.Core.Configuration.Models;

    public class HMACSecretKeyComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
            => builder.Services.Configure<ImagingSettings>(options =>
            {
                if (options.HMACSecretKey.Length == 0)
                {
                    byte[] secret = new byte[64]; // Change to 128 when using HMACSHA384 or HMACSHA512
                    RandomNumberGenerator.Create().GetBytes(secret);
                    options.HMACSecretKey = secret;

                    var logger = builder.BuilderLoggerFactory.CreateLogger<HMACSecretKeyComposer>();
                    logger.LogInformation("Imaging settings is now using HMACSecretKey: {HMACSecretKey}", Convert.ToBase64String(secret));
                }
            });
    }
}
