namespace ShopVRG.Domain.Models.ValueObjects;

/// <summary>
/// Value object for ShippingAddress representing a delivery address
/// Validation rules:
/// - Street must be between 5 and 200 characters
/// - City must be between 2 and 100 characters
/// - PostalCode must be between 4 and 10 characters
/// - Country must be between 2 and 60 characters
/// </summary>
public sealed class ShippingAddress : IEquatable<ShippingAddress>
{
    public string Street { get; }
    public string City { get; }
    public string PostalCode { get; }
    public string Country { get; }

    private ShippingAddress(string street, string city, string postalCode, string country)
    {
        Street = street;
        City = city;
        PostalCode = postalCode;
        Country = country;
    }

    public static bool TryCreate(
        string? street,
        string? city,
        string? postalCode,
        string? country,
        out ShippingAddress? address,
        out string? error)
    {
        address = null;
        error = null;

        // Validate Street
        if (string.IsNullOrWhiteSpace(street))
        {
            error = "Street must not be empty";
            return false;
        }
        var streetTrimmed = street.Trim();
        if (streetTrimmed.Length < 5 || streetTrimmed.Length > 200)
        {
            error = "Street must be between 5 and 200 characters";
            return false;
        }

        // Validate City
        if (string.IsNullOrWhiteSpace(city))
        {
            error = "City must not be empty";
            return false;
        }
        var cityTrimmed = city.Trim();
        if (cityTrimmed.Length < 2 || cityTrimmed.Length > 100)
        {
            error = "City must be between 2 and 100 characters";
            return false;
        }

        // Validate PostalCode
        if (string.IsNullOrWhiteSpace(postalCode))
        {
            error = "Postal code must not be empty";
            return false;
        }
        var postalCodeTrimmed = postalCode.Trim();
        if (postalCodeTrimmed.Length < 4 || postalCodeTrimmed.Length > 10)
        {
            error = "Postal code must be between 4 and 10 characters";
            return false;
        }

        // Validate Country
        if (string.IsNullOrWhiteSpace(country))
        {
            error = "Country must not be empty";
            return false;
        }
        var countryTrimmed = country.Trim();
        if (countryTrimmed.Length < 2 || countryTrimmed.Length > 60)
        {
            error = "Country must be between 2 and 60 characters";
            return false;
        }

        address = new ShippingAddress(streetTrimmed, cityTrimmed, postalCodeTrimmed, countryTrimmed);
        return true;
    }

    public string ToFullAddress() => $"{Street}, {City}, {PostalCode}, {Country}";

    public override string ToString() => ToFullAddress();

    public override bool Equals(object? obj) =>
        obj is ShippingAddress other &&
        Street == other.Street &&
        City == other.City &&
        PostalCode == other.PostalCode &&
        Country == other.Country;

    public bool Equals(ShippingAddress? other) =>
        other is not null &&
        Street == other.Street &&
        City == other.City &&
        PostalCode == other.PostalCode &&
        Country == other.Country;

    public override int GetHashCode() => HashCode.Combine(Street, City, PostalCode, Country);

    public static bool operator ==(ShippingAddress? left, ShippingAddress? right) =>
        left?.Equals(right) ?? right is null;

    public static bool operator !=(ShippingAddress? left, ShippingAddress? right) =>
        !(left == right);
}
