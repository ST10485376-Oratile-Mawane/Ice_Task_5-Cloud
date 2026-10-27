using Azure;
using Azure.Data.Tables;
using System;

namespace Ice_Task_5
{
    public class NoteTable : ITableEntity
    {
        public string PartitionKey { get; set; } = default!; // userId
        public string RowKey { get; set; } = default!;    // noteId (Guid)

        public string Title { get; set; } = default!;
        public string Content { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Required by ITableEntity
        public ETag ETag { get; set; } = ETag.All;
        public DateTimeOffset? Timestamp { get; set; }
    }
}