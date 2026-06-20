namespace Ceiba.CourierMax.API.Services.Interfaces;

public interface IJwtTokenService
{
    void IssueToken(string username, string role);
    void RevokeToken();
}
