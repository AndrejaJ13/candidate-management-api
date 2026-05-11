using System.Net.Mail;

namespace Zadatak.Validation;

public static class CandidateValidator
{
    public static void Validate(string fullName, DateOnly dateOfBirth, string contactNumber, string email)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Full name is required.", nameof(fullName));
        }

        if (dateOfBirth == default)
        {
            throw new ArgumentException("Date of birth is required.", nameof(dateOfBirth));
        }

        if (dateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ArgumentException("Date of birth cannot be in the future.", nameof(dateOfBirth));
        }

        if (string.IsNullOrWhiteSpace(contactNumber))
        {
            throw new ArgumentException("Contact number is required.", nameof(contactNumber));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        try
        {
            _ = new MailAddress(email);
        }
        catch (FormatException exception)
        {
            throw new ArgumentException("Email format is invalid.", nameof(email), exception);
        }
    }
}
