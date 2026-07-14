using vCardLib.Models;
using vCardLib.Serialization;

public class SessionCache
{
	public string VCard { get
		{
			MyCard.Uid = null;
			return vCardSerializer.Serialize(MyCard);
		}
	}

	
	public vCard MyCard = new vCard(vCardLib.Enums.vCardVersion.v4)
	{
		EmailAddresses = new List<EmailAddress> { new EmailAddress("") },
		PhoneNumbers = new List<TelephoneNumber> { new TelephoneNumber("") }
	};

	

}