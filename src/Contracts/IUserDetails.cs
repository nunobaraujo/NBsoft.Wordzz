namespace NBsoft.Wordzz.Contracts
{
    public interface  IUserDetails
    {
        string UserName { get; }
        string FirstName { get; }
        string LastName { get; }
        string Address { get; }
        string PostalCode { get; }
        string City { get; }
        string Country { get; }
        string Email { get; }
    }
}
