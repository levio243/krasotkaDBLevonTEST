using System;

namespace krasotkaDBLevonTEST
{
    public class Client
    {
        public int ClientCode { get; set; }
        public string ClientName { get; set; }
        public string ClientTel { get; set; }
        public string ClientActivity { get; set; }
    }

    public class Master
    {
        public int MasterCode { get; set; }
        public string MasterName { get; set; }
        public string MasterTel { get; set; }
        public int ServTypeCode { get; set; }
        public string ServTypeName { get; set; }
        public string Activity { get; set; }
    }

    public class Service
    {
        public int ServiceCode { get; set; }
        public string ServiceName { get; set; }
        public int ServicePrice { get; set; }
        public int ServiceDuration { get; set; }
        public int ServiceTypeCode { get; set; }
        public string ServiceTypeName { get; set; }
        public string Activity { get; set; }
    }

    public class ServType
    {
        public int ServTypeCode { get; set; }
        public string TypeName { get; set; }
        public string Activity { get; set; }
    }

    public class Appointment
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string ServiceName { get; set; }
        public string ClientName { get; set; }
        public string ClientPhone { get; set; }
        public decimal Cost { get; set; }
        public string Status { get; set; }
    }
}