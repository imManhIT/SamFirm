namespace SamFirm
{
    internal class Device
    {
        private int id;
        private string deviceName;
        private string cscCode;
        private string model;

        public Device(int id, string deviceName, string cscCode, string model)
        {
            this.id = id;
            this.deviceName = deviceName;
            this.cscCode = cscCode;
            this.model = model;
        }

        public Device()
        {
        }

        public int Id { get => id; set => id = value; }
        public string DeviceName { get => deviceName; set => deviceName = value; }
        public string CscCode { get => cscCode; set => cscCode = value; }
        public string Model { get => model; set => model = value; }
    }
}