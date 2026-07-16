using vCardLib.Enums;
using vCardLib.Models;
using vCardLib.Serialization;

namespace QrScreen;

public class SessionCache
{

	public event EventHandler<VCardField>? OnFieldChanged;

	public string VCard
	{
		get
		{
			MyCard.Uid = null;
			return vCardSerializer.Serialize(MyCard);
		}
	}

	
	private vCard MyCard = new vCard(vCardLib.Enums.vCardVersion.v4)
	{
		EmailAddresses = new List<EmailAddress> { new EmailAddress("") },
		PhoneNumbers = new List<TelephoneNumber> { new TelephoneNumber("") }
	};

		public void SetFieldValue(VCardField field, string value)
	{

		Console.WriteLine($"Setting field {field} to value: {value}");

		switch (field)
		{
			case VCardField.Email:
				MyCard.EmailAddresses = new List<EmailAddress> { new EmailAddress(value) { Type = EmailAddressType.Work } };
				break;
			case VCardField.Phone:
				MyCard.PhoneNumbers = new List<TelephoneNumber> { new TelephoneNumber(value) { Type = TelephoneNumberType.Work } };
				break;
			case VCardField.GivenName:
				MyCard.Name = new Name(MyCard.Name?.FamilyName ?? string.Empty, value, null,null,null);
				break;
			case VCardField.FamilyName:
				MyCard.Name = new Name(value, MyCard.Name?.GivenName ?? string.Empty, null,null,null);
				break;
			case VCardField.NickName:
				MyCard.NickName = value;
				break;
			case VCardField.Title:
				MyCard.Title = value;
				break;
			case VCardField.Organization:
				MyCard.Organization = new Organization(value, null,null);
				break;
			case VCardField.Url:	
				var valueToSet = value;
				if (!valueToSet.StartsWith("http")) valueToSet = $"https://{valueToSet}";
				Console.WriteLine($"Setting MyCard.Url to: {valueToSet}");
				MyCard.Url = new Url(valueToSet);
				break;
		}

		OnFieldChanged?.Invoke(this, field);
		
	}

	public string GetFieldValue(VCardField field)
	{
		switch (field)
		{
			case VCardField.Email:
				return MyCard.EmailAddresses.FirstOrDefault().Value ?? string.Empty;
			case VCardField.Phone:
				return MyCard.PhoneNumbers.FirstOrDefault().Number ?? string.Empty;
			case VCardField.Url:	
				return MyCard.Url?.Value ?? string.Empty;
			case VCardField.GivenName:
				return MyCard.Name?.GivenName ?? string.Empty;
			case VCardField.FamilyName:
				return MyCard.Name?.FamilyName ?? string.Empty;
			case VCardField.NickName:
				return MyCard.NickName ?? string.Empty;
			case VCardField.Title:
				return MyCard.Title ?? string.Empty;
			case VCardField.Organization:
				return MyCard.Organization?.Name ?? string.Empty;
			default:
				return string.Empty;
		}
	}

	

}

