namespace E_commerce.Application.Helper
{
    public class JWT
    {
        public string key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int Expiration { get; set; }
        public string SecretKey { get; set; }
        public int RefreshTokenExpiryInDays { get; set; }
        public int ExpiryInMinutes { get; set; }
    }
}
