namespace xbridge
{
    public interface IXBridgeSharedObject
    {
        int ID { get; set; }
        int TypeID { get; set; }

        void Destroy();
    }
}