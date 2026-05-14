namespace ToDoList.Models
{
    public class TodoTask
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string TaskName { get; set; } = string.Empty;
        public DateTime ExpectedDelivery { get; set; }
        public string Comment { get; set; } = string.Empty;
        public TaskCategory Category { get; set; }
        public string AssignedBy { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? CompletedDate { get; set; }

        public int DaysUntilDue => (ExpectedDelivery.Date - DateTime.Now.Date).Days;
        public bool IsOverdue => !IsCompleted && DateTime.Now.Date > ExpectedDelivery.Date;
        public bool IsDueToday => !IsCompleted && DateTime.Now.Date == ExpectedDelivery.Date;
        public bool IsDueInNext3Days => !IsCompleted && DaysUntilDue >= 0 && DaysUntilDue <= 3;
    }

    public enum TaskCategory
    {
        PD,
        PS,
        Personal
    }
}
