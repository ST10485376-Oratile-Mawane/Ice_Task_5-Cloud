using Azure;
using Azure.Data.Tables;
using Ice_Task_5;

namespace Ice_Task_5.Services
{
    public class NoteStorageService
    {
        private readonly TableClient _tableClient;

        public NoteStorageService(string connectionString)
        {
            // "NotesTable" is the name of your table in Azure Table Storage or Azurite
            _tableClient = new TableClient(connectionString, "NotesTable");
            _tableClient.CreateIfNotExists();
        }

        // Get all notes for a specific user
        public async Task<List<NoteTable>> GetNotesByUserAsync(string userId)
        {
            var notes = new List<NoteTable>();
            AsyncPageable<NoteTable> queryResults = _tableClient.QueryAsync<NoteTable>(ent => ent.PartitionKey == userId);

            await foreach (var note in queryResults)
            {
                notes.Add(note);
            }
            return notes;
        }

        // Get a single note securely (ensuring it matches the user's PartitionKey)
        public async Task<NoteTable?> GetNoteAsync(string userId, string noteId)
        {
            try
            {
                var response = await _tableClient.GetEntityAsync<NoteTable >(userId, noteId);
                return response.Value;
            }
            catch
            {
                return null; // Note not found or unauthorized access
            }
        }

        // Create a new note
        public async Task UpsertNoteAsync(NoteTable note)
        {
            await _tableClient.UpsertEntityAsync(note);
        }

        // Delete a note
        public async Task DeleteNoteAsync(string userId, string noteId)
        {
            await _tableClient.DeleteEntityAsync(userId, noteId);
        }
    }
}