namespace MuMuSDK.Desktop.Models
{
    /// <summary>
    /// Holds read-only text primitives mapping background memory data directly to the user interface grid.
    /// </summary>
    public class TelemetryRowItem
    {
        public string ComponentName { get; set; }
        public string GuestPhysicalAddress { get; set; }
        public string HexByteView { get; set; }
        public string MaterializedValue { get; set; }

        public TelemetryRowItem(string name, string gpa, string bytes, string val)
        {
            ComponentName = name;
            GuestPhysicalAddress = gpa;
            HexByteView = bytes;
            MaterializedValue = val;
        }
    }
}
