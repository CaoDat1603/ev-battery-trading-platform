namespace Identity.Domain.Abtractions
{
    public interface ISystemJwtProvider
    {
        string GenerateToken(string serviceName);
    }
}
