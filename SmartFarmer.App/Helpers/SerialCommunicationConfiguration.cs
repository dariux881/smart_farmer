namespace SmartFarmer.Helpers;

public class SerialCommunicationConfiguration
{
    public string SerialPortName { get; set; }
    public int BaudRate { get; set; }
    public int? ReadTimeout { get; set; }
    public int? WriteTimeout { get; set; }
    public int? MaxReadAttempts { get; set; }
    public int? DelayBetweenReadAttempts { get; set; }
}