namespace SamFirm
{
    internal class Csc
    {
        private int id;
        private string cscCode;
        private string region;

        public Csc(int id, string cscCode, string region)
        {
            this.id = id;
            this.cscCode = cscCode;
            this.region = region;
        }

        public int Id { get => id; set => id = value; }
        public string CscCode { get => cscCode; set => cscCode = value; }
        public string Region { get => region; set => region = value; }
    }
}