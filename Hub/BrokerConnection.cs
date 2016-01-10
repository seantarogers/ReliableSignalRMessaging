namespace Hub
{
    using System;

    public class BrokerConnection
    {
        public string ConnectionId { get; set; }
        public int BrokerId { get; set; }
        public DateTime TokenExpiresOn { get; set; }
        public bool Expired { get; set; }
    }
}

