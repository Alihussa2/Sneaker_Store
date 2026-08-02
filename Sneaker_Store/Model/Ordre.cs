namespace Sneaker_Store.Model;

public class Ordre
{
    public int OrdreId { get; set; }
    public int KundeId { get; set; }
    public int SkoId { get; set; }
    public int Antal { get; set; }
    public double TotalPris { get; set; }

    public Ordre()
    {
    }

    public Ordre(int ordreId, int kundeId, int skoId, int antal, double totalPris)
    {
        OrdreId = ordreId;
        KundeId = kundeId;
        SkoId = skoId;
        Antal = antal;
        TotalPris = totalPris;
    }

    public override string ToString()
    {
        return $"{nameof(OrdreId)}: {OrdreId}, {nameof(KundeId)}: {KundeId}, {nameof(SkoId)}: {SkoId}, {nameof(Antal)}: {Antal}, {nameof(TotalPris)}: {TotalPris}";
    }
}
