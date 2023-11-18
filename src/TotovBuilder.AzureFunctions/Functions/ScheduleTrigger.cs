namespace TotovBuilder.AzureFunctions.Functions
{
    /// <summary>
    /// Represents data about the schedule triggering an Azure Function.
    /// </summary>
    public class ScheduleTrigger
    {
        /// <summary>
        /// Status.
        /// </summary>
        public ScheduleStatus ScheduleStatus { get; set; } = new ScheduleStatus();

        /// <summary>
        /// Indicates whether the trigger occurs later than scheduled.
        /// </summary>
        public bool IsPastDue { get; set; }
    }

    /// <summary>
    /// Represents a schedule status of a schedule trigger.
    /// </summary>
    public class ScheduleStatus
    {
        /// <summary>
        /// Date of the last scheduled trigger.
        /// </summary>
        public DateTime Last { get; set; }

        /// <summary>
        /// Date of the last time the function was really triggered.
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Date of the next scheduled trigger.
        /// </summary>
        public DateTime Next { get; set; }
    }
}
