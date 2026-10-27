using Azure;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ice_Task_5
{
    public class NoteEntity
    {
        // PartitionKey can be the UserId for efficient querying per user
        public string PartitionKey { get; set; } = string.Empty; // userId
        public string RowKey { get; set; } = Guid.NewGuid().ToString(); // noteId

        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}
