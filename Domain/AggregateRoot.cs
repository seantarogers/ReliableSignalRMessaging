namespace Domain
{
    using System;

    public abstract class AggregateRoot
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
    }
}