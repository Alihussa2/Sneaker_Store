namespace Sneaker_Store.Model;

public class Kunde
{
    public int KundeId { get; se; }
    public string Navn { get; set; }
    public string Efternavn { get; set; }
    public string Email { get; set; }
    public string Adresse { get; set; }
    public string By { get; set; }
    public int Postnr { get; set; }
    public string Kode { get; set; }
    public bool IsAdmin { get; set; }

    public Kunde()
    {
        Navn = "";
        Efternavn = "";
        Email = "";
        Adresse = "";
        By = "";
        Kode = "";
    }

    public Kunde(int kundeId, string navn, string efternavn, string email, string adresse, string by, int postnr, string kode, bool isAdmin)
    {
        KundeId = kundeId;
        Navn = navn;
        Efternavn = efternavn;
        Email = email;
        Adresse = adresse;
        By = by;
        Postnr = postnr;
        Kode = kode;
        IsAdmin = isAdmin;
    }

    public override string ToString()
    {
        return $"{nameof(KundeId)}: {KundeId}, {nameof(Navn)}: {Navn}, {nameof(Efternavn)}: {Efternavn}, {nameof(Email)}: {Email}, {nameof(Adresse)}: {Adresse}, {nameof(By)}: {By}, {nameof(Postnr)}: {Postnr}, {nameof(IsAdmin)}: {IsAdmin}";
    }
}
