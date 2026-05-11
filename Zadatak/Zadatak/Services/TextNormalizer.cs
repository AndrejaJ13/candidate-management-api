namespace Zadatak.Services;

public static class TextNormalizer
{
    public static string NormalizeEmail(string email)
    {
        return email.Trim().ToUpperInvariant();
    }

    public static string NormalizeSkillName(string skillName)
    {
        return NormalizeWhiteSpace(skillName).ToUpperInvariant();
    }

    public static string NormalizeWhiteSpace(string value)
    {
        return string.Join(' ', value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
