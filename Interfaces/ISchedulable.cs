namespace HospitalApp.Interfaces
{
    
    public interface ISchedulable
    {
        bool   IsAvailable(DateTime dateTime);
        void   AddToSchedule(DateTime dateTime);
        void   RemoveFromSchedule(DateTime dateTime);
        string GetScheduleSummary();
        IReadOnlyList<DateTime> GetUpcomingSlots(int days = 7);
    }
}
