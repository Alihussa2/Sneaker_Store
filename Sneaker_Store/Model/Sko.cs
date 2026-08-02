namespace Sneaker_Store.Model
{
    public class Sko
    {
        public int SkoId { get; set; }
        public string Maerke { get; set; }
        public string Model { get; set; }
        public int Str { get; set; }
        public double Pris { get; set; }
        public int LagerAntal { get; set; }
        public string? Billede { get; set; }

        public Sko(int skoid, string maerke, string model, int str, double pris, int lagerAntal = 0, string? billede = null)
        {
            SkoId = skoid;
            Maerke = maerke;
            Model = model;
            Str = str;
            Pris = pris;
            LagerAntal = lagerAntal;
            Billede = billede;
        }

        public Sko() // default
        {
            Maerke = "";
            Model = "";
        }

        public override string ToString()
        {
            return $"{{{nameof(SkoId)}={SkoId}, {nameof(Maerke)}={Maerke}, {nameof(Model)}={Model}, {nameof(Str)}={Str}, {nameof(Pris)}={Pris}, {nameof(LagerAntal)}={LagerAntal}, {nameof(Billede)}={Billede}}}";
        }
    }
}
