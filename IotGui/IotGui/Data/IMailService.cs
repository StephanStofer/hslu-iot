namespace IotGui.Data
{
    public interface IMailService
    {
        void SendAlertMail(string device, string value);
    }
}